using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;


[RequireComponent(typeof(Rigidbody2D))]
public class CentipedeController : MonoBehaviour
{
    [Header("Setup")]
    public GameObject segmentPrefab;
    public GameObject headVisualizer;

    [Header("Build")]
    [Min(2)] public int segmentCount = 14;        
    public float segmentSpacing = 0.35f;         
    [Range(0.05f, 0.5f)] public float tailFraction = 0.20f; 
    [Range(0f, 1f)] public float tailMinScale = 0.35f;     

    [Header("Movement")]
    public Transform target;                     
    public float maxSpeed = 6f;
    public float steeringForce = 30f;
    public float arriveRadius = 2.0f;            

    [Header("Simulation")]
    [Min(0)] public float distanceMultiplier = 1.0f;

    private Rigidbody2D headBody;
    private List<Rigidbody2D> bodies = new();
    private List<DistanceJoint2D> springs = new(); 
    private List<float> restDistances = new();     

    private CharacterContext context;

    void Start()
    {
        context = GetComponent<CharacterContext>();
        headBody = context.rigidBody;

        BuildChain();
    }

    void FixedUpdate()
    {
        ActionBehavior attack = context.action.activeAction;

        if (attack == null || (!attack.isRunning && !attack.isOnCoolDown))
        {
            SteerHeadTowardsTarget();
        }

        LerpLinkDistances(distanceMultiplier);
    }

    void BuildChain()
    {
        // Spawn head
        GameObject head = headVisualizer;
        head.name = "Centipede_Head";
        headBody = GetComponent<Rigidbody2D>();
        bodies.Add(headBody);

        // Spawn segments
        Rigidbody2D prev = headBody;
        Vector2 dir = (Vector2)transform.right;
        int tailCount = Mathf.Max(1, Mathf.RoundToInt(segmentCount * tailFraction));
        int bodyCount = Mathf.Max(0, segmentCount - 1);
        int tailStartIndex = Mathf.Max(1, bodyCount - tailCount + 1);

        for (int i = 1; i < segmentCount; ++i)
        {
            Vector3 pos = (Vector3)((Vector2)headVisualizer.transform.position - dir * (segmentSpacing * i));
            GameObject seg = Instantiate(segmentPrefab, pos, headVisualizer.transform.rotation, headVisualizer.transform);
            seg.name = $"Centipede_Segment_{i:D2}";
            Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
            bodies.Add(rb);

            if (i >= tailStartIndex)
            {
                float t = Mathf.InverseLerp(tailStartIndex, segmentCount - 1, i);
                float s = Mathf.Lerp(1f, tailMinScale, t);
                seg.transform.localScale = new Vector3(s, s, 1f);
            }

            HingeJoint2D hinge = seg.GetComponent<HingeJoint2D>();
            if (!hinge) hinge = seg.AddComponent<HingeJoint2D>();
            hinge.autoConfigureConnectedAnchor = true;
            hinge.connectedBody = prev;

            DistanceJoint2D dist = seg.GetComponent<DistanceJoint2D>();
            if (!dist) dist = seg.AddComponent<DistanceJoint2D>();
            dist.connectedBody = prev;
            dist.autoConfigureDistance = false;
            dist.enableCollision = false;
            dist.maxDistanceOnly = false;
            dist.distance = segmentSpacing;

            springs.Add(dist);
            restDistances.Add(segmentSpacing);

            prev = rb;

            context.hitReceivers.AddRange(seg.GetComponentsInChildren<HitReceiver>().ToList());
        }
    }

    void SteerHeadTowardsTarget()
    {
        if (!target || !headBody) return;

        Vector2 toTarget = (Vector2)(target.position - headBody.transform.position);
        float dist = toTarget.magnitude;
        float speed = context.attributes.movement.maxSpeed;

        if (dist < arriveRadius)
        {
            speed = 0.0f;
        }

        //headBody.AddForce(steer, ForceMode2D.Force);

        context.movement.SetTargetMoveDirection(toTarget.normalized);
        context.movement.SetTargetAimDirection(toTarget.normalized);
        context.movement.SetTargetMoveSpeed(speed);
/*

        if (headBody.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(headBody.velocity.y, headBody.velocity.x) * Mathf.Rad2Deg;
            headBody.MoveRotation(angle);
        }*/
    }

    public void LerpLinkDistances(float targetMultiplier)
    {
        for (int i = 0; i < springs.Count; i++)
        {
            springs[i].distance = Mathf.Lerp(springs[i].distance, restDistances[i] * targetMultiplier, 0.22f);
        }
    }

    public void SetLinkDistances(float multiplier)
    {
        for (int i = 0; i < springs.Count; i++)
        { 
            springs[i].distance = restDistances[i] * multiplier;
        }
    }

    public void ResetLinkDistances()
    {
        for (int i = 0; i < springs.Count; i++)
        { 
            springs[i].distance = restDistances[i]; 
        }
    }
}

