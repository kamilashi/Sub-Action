using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static EnemyAIUtility;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.GraphicsBuffer;

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

    public float shyness = 1.0f; // how much we approach interest directions
    public float fear = 1.0f; // how fast we repel from dangers

    public float maxGapDegForClusters = 20.0f;

    public Transform target;
    public bool hasTarget;
    public Label targetLabel;

    public List<CharacterContext> sensedCreatures;
    private HashSet<int> sensedCreatureIds;
    CharacterContext context;

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

    // Update is called once per frame
    void Update()
    {
        ProcessCreatures();
    }

    void OnNewCreatureSensed(Collider2D collider, Sensor other)
    {
        if( ((1 << collider.gameObject.layer) & GameManager.Instance.attractorLayers) == 0)
        {
            return;
        }

        int id = other.entityId;
        if (!sensedCreatureIds.Contains(id))
        {
            sensedCreatures.Add(other.context);
            sensedCreatureIds.Add(id);
        }
    }

    void OnCreatureLost(Collider2D collider, Sensor other)
    {
        int id = other.entityId;
        if (sensedCreatureIds.Contains(id))
        {
            sensedCreatureIds.Remove(id);
            sensedCreatures.Remove(other.context);
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

        Func < List<List<int>>, List<ScoreInputData>, List<DirectionScore>> processClusters = (clusters, scores) =>
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

                processedScores.Add(GetInterestScore(averagePosition, totalRank));
            }

            return processedScores;
        };

        List<List<int>> directionClusters = AngleToleranceCluster.ClusterByMaxGap(transform.position, interestList, maxGapDegForClusters);
        List<DirectionScore> interestScores = processClusters(directionClusters, interestList);

        directionClusters = AngleToleranceCluster.ClusterByMaxGap(transform.position, dangerList, maxGapDegForClusters);
        List<DirectionScore> dangerScores = processClusters(directionClusters, dangerList);

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

    DirectionScore GetInterestScore(Vector3 position, int rank)
    {
        Vector2 direction = position - context.transform.position;
        float squareDistance = (direction).sqrMagnitude;

        float rawInterest = rank / squareDistance;

        DirectionScore score = new DirectionScore();
        score.score = rawInterest;
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
