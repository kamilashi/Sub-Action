using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;


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
    public float arriveRadius = 2.0f;            

    [Header("Simulation")]
    public float maxSegmentLength = 0.35f;
    [Range(0f, 1f)] public float damping = 0.7f;


    private Rigidbody2D headBody;
    private List<Transform> segmentTransforms = new();
    List<Vector2> pos;
    List<Vector2> prevPos;
    List<float> tightnessMultipliers;

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
            SteerHead();
        }

        UpdateSegments();
    }

    void BuildChain()
    {
        pos = new List<Vector2>(segmentCount);
        prevPos = new List<Vector2>(segmentCount);
        tightnessMultipliers = new List<float>(segmentCount - 1);

        // Spawn segments
        Vector2 dir = (Vector2)transform.right;

        pos.Add(transform.position);
        prevPos.Add(pos[0]);
        segmentTransforms.Add(transform);

        int tailCount = Mathf.Max(1, Mathf.RoundToInt(segmentCount * tailFraction));
        int bodyCount = Mathf.Max(0, segmentCount - 1);
        int tailStartIndex = Mathf.Max(1, bodyCount - tailCount + 1);

        GameObject parent = new GameObject("CentipedeRoot");

        for (int i = 1; i < segmentCount; ++i)
        {
            Vector3 position = (Vector3)((Vector2)headBody.transform.position - dir * (maxSegmentLength * i));
            GameObject seg = Instantiate(segmentPrefab, position, headBody.transform.rotation, parent.transform);
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
            tightnessMultipliers.Add(1.0f);

            context.hitReceivers.AddRange(seg.GetComponentsInChildren<HitReceiver>().ToList());
        }
    }

    void SteerHead()
    {
        if (!target || !headBody) return;

        Vector2 toTarget = (Vector2)(target.position - headBody.transform.position);
        float dist = toTarget.magnitude;
        toTarget = toTarget.normalized;

        float speed = context.attributes.movement.maxSpeed;

        if (dist < arriveRadius)
        {
            speed = 0.0f;
        }

        context.movement.SetTargetMoveDirection(toTarget);
        context.movement.SetTargetAimDirection(toTarget);
        context.movement.SetTargetMoveSpeed(speed);
    }

    void UpdateSegments()
    {
        pos[0] = headBody.position;

        for (int i = 1; i < segmentCount; i++)
        {
            Vector2 dir = -segmentTransforms[i-1].right;
            Vector2 newPos = pos[i-1] + dir * maxSegmentLength * tightnessMultipliers[i];

            Debug.DrawLine(pos[i - 1], newPos);
            Quaternion rotation = Quaternion.Slerp(segmentTransforms[i - 1].rotation, segmentTransforms[i].rotation, damping);

            segmentTransforms[i].Translate(newPos - pos[i], Space.World);
            segmentTransforms[i].rotation = rotation;

            prevPos[i] = pos[i];
            pos[i] = segmentTransforms[i].position;
        }
    }
}

