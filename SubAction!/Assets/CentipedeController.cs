using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;


[RequireComponent(typeof(Rigidbody2D))]
public class CentipedeController : MonoBehaviour
{
    [Header("Setup")]
    public GameObject segmentPrefab;

    [Header("Build")]
    [Min(2)] public int segmentCount = 14;        
    [Range(0.05f, 0.5f)] public float tailFraction = 0.20f; 
    [Range(0f, 1f)] public float tailMinScale = 0.35f;      

    [Header("Movement")]
    public Transform target;                      
    public Vector2 targetPosition;                      
    public float arriveRadius = 2.0f;
    public float wanderTargetPositionMinDistance = 5.0f;
    public float wanderTargetPositionMaxDistance = 15.0f;

    [Header("Simulation")]
    public float tightLength = 0.4f;
    public float looseLength = 0.7f;
    [Range(0f, 1f)] public float damping = 0.7f;
    [Range(0f, 1f)] public float spring = 0.1f; // low spring might make the movement "slidy"
    public float rigidDirFallbackSlope = 1.0f;

    [Header("Debug")]
    public bool isSteering;
    public bool isDying;
    public bool wanderPositionReached;
    public float alignedWithTarget = 0.0f;

    private Rigidbody2D headBody;
    private List<Transform> segmentTransforms = new();
    List<Vector2> pos;
    List<Vector2> prevPos;
    List<float> looseMultiplier;

    private CharacterContext context;
    private EnemySteering steering;
    private EnemyDeath death;
    private GameObject segmentsParent;

    void Start()
    {
        context = GetComponent<CharacterContext>();
        death = GetComponent<EnemyDeath>();
        steering = GetComponent<EnemySteering>();

        headBody = context.rigidBody;

        death.onStartDying.AddListener(OnStartDying);

        BuildChain();
    }

    void FixedUpdate()
    {
        if(isDying)
        {
            segmentsParent.transform.parent = this.transform;
            if(context.action.activeAction != null)
            {
                context.action.activeAction.StopAllCoroutines();
            }

            return;
        }

        ActionBehavior attack = context.action.activeAction;
        alignedWithTarget = 1.0f;

        target = null;

        if(steering.hasTarget && steering.targetLabel == Label.Interest)
        {
            target = steering.target;
            targetPosition = target.position;
        }
        else //if(wanderPositionReached)
        {
            targetPosition = this.transform.position;
           // targetPosition = Library.Misc.GetRandomRadialPosition(wanderTargetPositionMinDistance, wanderTargetPositionMaxDistance, transform.position);
        }

        if (attack == null || !attack.isRunning)
        {
            //SteerHead();

            //FluctuateTightness();

            if (!context.action.TryRun(1))
            {
                SteerHead(targetPosition);
            }

            isSteering = true;
        }
        isSteering = false;

        UpdateSegments();
    }

    void BuildChain()
    {
        pos = new List<Vector2>(segmentCount);
        prevPos = new List<Vector2>(segmentCount);
        looseMultiplier = new List<float>(segmentCount - 1);

        // Spawn segments
        Vector2 dir = (Vector2)transform.right;

        pos.Add(transform.position);
        prevPos.Add(pos[0]);
        segmentTransforms.Add(transform);

        int tailCount = Mathf.Max(1, Mathf.RoundToInt(segmentCount * tailFraction));
        int bodyCount = Mathf.Max(0, segmentCount - 1);
        int tailStartIndex = Mathf.Max(1, bodyCount - tailCount + 1);

        segmentsParent = new GameObject("CentipedeRoot");

        for (int i = 1; i < segmentCount; ++i)
        {
            Vector3 position = (Vector3)((Vector2)headBody.transform.position - dir * (tightLength * i));
            GameObject seg = Instantiate(segmentPrefab, position, headBody.transform.rotation, segmentsParent.transform);
            seg.name = $"Centipede_Segment_{i:D2}";

            if (i >= tailStartIndex)
            {
                float t = Mathf.InverseLerp(tailStartIndex, segmentCount - 1, i);
                float s = Mathf.Lerp(1f, tailMinScale, t);
                seg.transform.localScale = new Vector3(seg.transform.localScale.x, s, 1f);
            }

            segmentTransforms.Add(seg.transform);
            pos.Add(seg.transform.position);
            prevPos.Add(seg.transform.position);
            looseMultiplier.Add(0.0f);

            context.hitReceivers.AddRange(seg.GetComponentsInChildren<HitReceiver>().ToList());
            context.hitEmitters.AddRange(seg.GetComponentsInChildren<HitEmitter>().ToList());
            context.visualizer.bodyRenderers.Add(seg.gameObject.GetComponentInChildren<Renderer>());
        }

        foreach (HitReceiver hitReceiver in context.hitReceivers) 
        {
            hitReceiver.onBodyHit.AddListener(context.health.RemoveHealth);
            hitReceiver.entityId = context.entityId;
        }
        
        foreach (HitEmitter hitEmitter in context.hitEmitters) 
        {
            hitEmitter.entityId = context.entityId;
        }
    }

    void SteerHead(Vector3 targetPosition)
    {
        if (!target || !headBody) return;

        Vector2 toTarget = (Vector2)(targetPosition - headBody.transform.position);
        float dist = toTarget.magnitude;
        toTarget = toTarget.normalized;

        context.movement.SetTargetAimDirection(toTarget);

        // wait until aligned
        alignedWithTarget = Vector2.Dot(toTarget, headBody.transform.right);

        float speed = context.attributes.movement.maxSpeed;

        if (dist < arriveRadius && target != null)
        {
            speed = 0.0f;

            if (context.action.TryRun(0))
            { 
                return; 
            }
        }

        context.movement.SetTargetMoveDirection(toTarget);
        context.movement.SetTargetMoveSpeed(speed * Mathf.Clamp01(alignedWithTarget));
    }

    void UpdateSegments()
    {
        pos[0] = headBody.position;

        for (int i = 1; i < segmentCount; i++)
        {
            Quaternion rotation = Quaternion.Slerp(segmentTransforms[i - 1].rotation, segmentTransforms[i].rotation, damping );

            float length = Mathf.Lerp(tightLength, looseLength, looseMultiplier[i - 1]);

            Vector2 rigidDir = -segmentTransforms[i - 1].right;

            Vector2 verletVelocity = (pos[i - 1] - prevPos[i - 1]);
            Vector2 elasticDir = Vector2.Lerp(rigidDir, verletVelocity.normalized, Mathf.Min(verletVelocity.sqrMagnitude * rigidDirFallbackSlope, 1.0f));

            Vector2 rigidPos = pos[i - 1] + rigidDir * length;
            Vector2 elasicPos = prevPos[i - 1] + elasticDir * length;

            Vector2 newPos = Vector2.Lerp(rigidPos, elasicPos, spring);

            segmentTransforms[i].Translate(newPos - pos[i], Space.World);

            prevPos[i] = pos[i];
            pos[i] = segmentTransforms[i].position;
            segmentTransforms[i].rotation = rotation;
        }

        prevPos[0] = pos[0];
    }

    void OscillateTightness()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            looseMultiplier[i] = Mathf.Sin(Time.time + i);
        }
    }

    void OnStartDying()
    {
        isDying = true;
    }
}

