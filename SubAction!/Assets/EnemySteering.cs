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
public struct FlockingInputData
{
    public Vector2 position;
    public Vector2 direction;
}

public class EnemySteering : MonoBehaviour
{
    struct DirectionScore
    {
        public float score;
        public Vector2 targetPosition;
        public Vector2 normalizedDirection;
    }

    [Header("Detection")]
    public float maxGapDegForClusters = 20.0f;

    [Header("Chasing / Fleeing")]
    [Min(0.01f)]public float distanceWeight = 1.0f; // how much distance matters when calculating score = rank / distance
    [Min(0.01f)]public float rankWeight = 1.0f;     // how much rank matters when calculating score = rank / distance
    public float fear = 1.0f;                       // how fast we repel from dangers

    [Header("Flocking")]
    [Range(0, 1000)] public float separationSpeed;
    [Min(0.1f)] public float separationSubRadius = 1.0f;
    [Range(0, 1000)] public float alignmentSpeed;
    [Min(0.1f)] public float alignmentSubRadius = 1.0f;
    [Range(0, 1000)] public float cohernceSpeed;
    [Min(0.1f)] public float cohernceSubRadius = 1.0f;

    [Header("Output")]
    public Transform target;
    public bool hasTarget;
    public Label targetLabel;
    public Vector2 steeringDirectionRaw; // just flocking for now
    public bool hasSteeringDirection;

    public List<CharacterContext> sensedCreatures;
    HashSet<int> sensedCreatureIds;
    CharacterContext context;

#if UNITY_EDITOR
    public static bool debugDrawAll = false;
    public static bool debugDrawClustersAll = false;
    public static bool debugDrawFlockingAll = true;
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
        List<FlockingInputData> flockingList = new List<FlockingInputData>();

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
            else
            {
                FlockingInputData flockingInput = new FlockingInputData();
                flockingInput.position = creature.transform.position;
                flockingInput.direction = creature.movement.targetMoveDirection;
                flockingList.Add(flockingInput);
            }
        }

        ProcessFlocking(flockingList);

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
                if ((debugDrawAll || debugDrawThis) && debugDrawClustersAll)
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

    void ProcessFlocking(List<FlockingInputData> otherBoids)
    {
        if(otherBoids.Count != 0)
        {
            Vector2 thisPosition = (Vector2)transform.position;
            Vector2 result = Vector2.zero;

            Vector2 separation = Vector2.zero;
            Vector2 alignment = Vector2.zero;
            Vector2 coherence = Vector2.zero;

            float avgDistance = 0.0f;
            float closestDistance = float.MaxValue;
            int count = otherBoids.Count;
            int coherenceCount = 0;

            foreach (FlockingInputData neighbor in otherBoids)
            {
                Vector2 fromNeightbor = thisPosition - neighbor.position;
                float distance = fromNeightbor.magnitude;

                float sepFactor = separationSubRadius / Mathf.Max(distance, 0.1f); // separate from closest 
                sepFactor *= sepFactor;
                separation += (fromNeightbor).normalized * sepFactor;

                coherenceCount++;
                coherence += neighbor.position;

                float alignmentFactor = alignmentSubRadius / Mathf.Max(distance, 0.1f); // align with closest 
                alignmentFactor *= alignmentFactor;
                alignment += neighbor.direction * alignmentFactor;

                avgDistance += distance;
                closestDistance = Mathf.Min(closestDistance, distance);
            }

            avgDistance /= count;

            separation /= count;
            if (separation.sqrMagnitude > 1) separation.Normalize();

            coherence /= count;
            coherence -= thisPosition;
            if(coherence.sqrMagnitude > 1) coherence.Normalize();

            alignment /= count;
            if (alignment.sqrMagnitude > 1) alignment.Normalize();

            float coherenceFactor = (1.0f - closestDistance / cohernceSubRadius);
            coherenceFactor *= coherenceFactor;

            result = (separation * separationSpeed 
                    + alignment * alignmentSpeed 
                    + coherence * cohernceSpeed * coherenceFactor) * Time.deltaTime;

            if(result.magnitude > 0.0f)
            {
                //result.Normalize();
#if UNITY_EDITOR
                if ((debugDrawAll || debugDrawThis) && debugDrawFlockingAll)
                {
                    DebugRenderer.DrawLine(thisPosition, thisPosition + separation * separationSpeed * Time.deltaTime, transform.position.z, Color.red);
                    DebugRenderer.DrawLine(thisPosition, thisPosition + alignment * alignmentSpeed * Time.deltaTime, transform.position.z, Color.yellow);
                    DebugRenderer.DrawLine(thisPosition, thisPosition + coherence * cohernceSpeed * Time.deltaTime, transform.position.z, Color.cyan);
                }
#endif
                hasSteeringDirection = true;
                steeringDirectionRaw = result;
                return;
            }
        }

        hasSteeringDirection = false;
        steeringDirectionRaw = Vector3.zero;
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
