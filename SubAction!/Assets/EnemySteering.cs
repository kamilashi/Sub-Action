using System;
using System.Collections.Generic;
using UnityEngine;
using static EnemyAIUtility;

public enum Label
{
    Danger,
    Interest
}
public struct ScoreInputData
{
    public Vector2 position;
    public int rank;
}

public class EnemySteering : MonoBehaviour
{
    struct DirectionScore
    {
        public float score;
        public Vector2 targetPosition;
        public Vector2 normalizedDirection;
    }

    [Min(0.01f)]public float distanceWeight = 1.0f; // how much distance matters when calculating score = rank / distance
    [Min(0.01f)]public float rankWeight = 1.0f;     // how much rank matters when calculating score = rank / distance
    public float fear = 1.0f;                       // how fast we repel from dangers

    public float maxGapDegForClusters = 20.0f;

    public Transform target;
    public bool hasTarget;
    public Label targetLabel;

    public List<CharacterContext> sensedCreatures;
    public HashSet<int> sensedCreatureIds;
    CharacterContext context;

#if UNITY_EDITOR
    public static bool debugDrawAll = false;
    //public static bool debugDrawClustersAll = true;
    public bool debugDrawThis;
#endif

    void Start()
    {
        context = GetComponent<CharacterContext>();
        foreach(Sensor sensor in context.sensors)
        {
            sensor.onSensorEnter.AddListener(OnNewCreatureSensed);
            sensor.onSensorExit.AddListener(OnCreatureLost);
        }
        sensedCreatures = new List<CharacterContext>();
        sensedCreatureIds = new HashSet<int>();
    }

    void LateUpdate()
    {
        ProcessCreatures();
    }

    void OnNewCreatureSensed(Collider2D otherCollider, Sensor otherSensor)
    {
        if ( ((1 << otherCollider.gameObject.layer) & GameManager.Instance.attractorLayers) == 0)
        {
            return;
        }

        int id = otherSensor.entityId;
        if (!sensedCreatureIds.Contains(id))
        {
            sensedCreatures.Add(otherSensor.context);
            sensedCreatureIds.Add(id);
            //Debug.Log(this.name  + ": added " + (1 << otherCollider.gameObject.layer) + id);
        }
    }

    void OnCreatureLost(Collider2D otherCollider, Sensor otherSensor)
    {
        
        if (((1 << otherCollider.gameObject.layer) & GameManager.Instance.attractorLayers) == 0)
        {
            return;
        }

        int id = otherSensor.entityId;
        if (sensedCreatureIds.Contains(id))
        {
            sensedCreatureIds.Remove(id);
            sensedCreatures.Remove(otherSensor.context);
            //Debug.Log(this.name + ": removed " + (1 << otherCollider.gameObject.layer) + id);
        }
    }

    void ProcessCreatures()
    {
        target = null;
        hasTarget = false;

        if (sensedCreatures.Count == 0)
        {
            return;
        }

        List<ScoreInputData> interestList = new List<ScoreInputData>();
        List<ScoreInputData> dangerList = new List<ScoreInputData>();
        foreach (CharacterContext creature in sensedCreatures)
        {
            ScoreInputData scoreInput = new ScoreInputData();
            scoreInput.position = creature.transform.position;
            scoreInput.rank = creature.attributes.rank;

            if (scoreInput.rank > context.attributes.rank)
            {
                dangerList.Add(scoreInput);
            }
            else if (scoreInput.rank < context.attributes.rank)
            {
                interestList.Add(scoreInput);
            }
            // else flocking, or ignore
        }

        Func < List<List<int>>, List<ScoreInputData>, Color, List<DirectionScore>> processClusters = (clusters, scores, debugColor) =>
        {
            List<DirectionScore> processedScores = new List<DirectionScore>();

            if(clusters.Count == 0)
            {
                return processedScores;
            }

            foreach (List<int> cluster in clusters)
            {
                int totalRank = 0;
                Vector2 averagePosition = Vector2.zero;
                foreach (int creatureIndex in cluster)
                {
                    averagePosition += scores[creatureIndex].position;
                    totalRank += scores[creatureIndex].rank;
                }

                averagePosition /= cluster.Count;

                processedScores.Add(GetRawScore(averagePosition, totalRank));

#if UNITY_EDITOR
                if ((debugDrawAll || debugDrawThis)/* && debugDrawClustersAll */)
                {
                    DebugRenderer.DrawLine(transform.position, averagePosition, transform.position.z, debugColor);
                }
#endif
            }

            return processedScores;
        };

        List<List<int>> directionClusters = AngleToleranceCluster.ClusterByMaxGap(transform.position, interestList, maxGapDegForClusters);
        List<DirectionScore> interestScores = processClusters(directionClusters, interestList, Color.green);

        directionClusters = AngleToleranceCluster.ClusterByMaxGap(transform.position, dangerList, maxGapDegForClusters);
        List<DirectionScore> dangerScores = processClusters(directionClusters, dangerList, Color.red);

        List<DirectionScore> topInterest = Library.Misc.TopBy(interestScores, t => t.score);
        List<DirectionScore> topDanger = Library.Misc.TopBy(dangerScores, t => t.score);

        if (topInterest.Count > 0 && topDanger.Count > 0) 
        {
            Vector2 fittestPosition = Vector2.zero;

            if(topInterest.Count > 1 || topDanger.Count > 1)
            {
                Debug.LogWarning("Steering prio for multiple top choices is not implemented!");
            }

            // todo: better selection logic for ties
            if(IsInterestFit(ref fittestPosition, topInterest[0], topDanger[0], (1 - fear) * 1.0f))
            {
                SetClosestInterestTarget(fittestPosition);
                return;
            }
        }
        
        if (topDanger.Count > 0) // no fit interests or no interests at all
        {
            SetClosestDangerTarget(topDanger[0].targetPosition);
        }
        else if (topInterest.Count > 0) // no dangers
        {
            SetClosestInterestTarget(topInterest[0].targetPosition);
        }
    }

    DirectionScore GetRawScore(Vector3 position, int rank)
    {
        Vector2 direction = position - context.transform.position;
        float distance = (direction).magnitude;

        // might need to have different weights for interest and danger cases
        float rawScore = (rank * rankWeight) / (distance * distanceWeight);

        DirectionScore score = new DirectionScore();
        score.score = rawScore;
        score.targetPosition = position;
        score.normalizedDirection = direction.normalized;

        return score;
    }

    bool IsInterestFit(ref Vector2 fitPosition, DirectionScore interest, DirectionScore danger, float maxAlignment)
    {
        if (Vector2.Dot(interest.normalizedDirection, danger.normalizedDirection) < maxAlignment)
        {
            fitPosition = interest.targetPosition;
            return true;
        }

        return false;
    }

    void SetClosestInterestTarget(Vector3 refPosition)
    {
        target = GetClosestTo(refPosition, sensedCreatures);
        targetLabel = Label.Interest;
        hasTarget = true;
    }

    void SetClosestDangerTarget(Vector3 refPosition)
    {
        target = GetClosestTo(refPosition, sensedCreatures);
        targetLabel = Label.Danger;
        hasTarget = true;
    }

    Transform GetClosestTo(Vector3 refPosition, List<CharacterContext> creatures)
    {
        Transform closestSoFar = null;
        float closestSquareDist = float.MaxValue;
        foreach (CharacterContext context in creatures)
        {
            float squareDist = (context.transform.position - refPosition).sqrMagnitude;
            if (squareDist < closestSquareDist) 
            {
                closestSquareDist = squareDist;
                closestSoFar = context.transform;
            }
        }

        return closestSoFar;
    }
}
