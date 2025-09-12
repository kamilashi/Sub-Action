using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PopcornController : MonoBehaviour
{
    CharacterContext context;
    EnemySteering steering;
    public Transform targetTransform;
    public float stopDistance;
    public float movementOscillationFreq = 0.1f;
    public float movementOscillationAmpl = 1.0f;
    public float idleOscillationFreq = 0.5f;
    public float idleOscillationAmpl = 0.7f;

    void Start()
    {
        context = GetComponent<CharacterContext>();
        steering = GetComponent<EnemySteering>();
    }

    void Update()
    {
        targetTransform = steering.target;

        if (targetTransform != null && steering.targetLabel == Label.Danger)
        {
            SteerFromtarget();
        }
        else
        {
            Idle();
        }
    }

    void SteerFromtarget()
    {
        Vector2 dir = targetTransform.position - transform.position;
        float distance = dir.magnitude;
        float arrival = Mathf.Clamp01(distance / stopDistance);
        float fleeArrival = 1.0f - arrival;
        dir = -1.0f * dir.normalized;

        Vector2 offset = EnemyAIUtility.GetOscillationOverDirection(dir, Time.time, movementOscillationFreq, movementOscillationAmpl);

        dir += offset;
        dir = dir.normalized;

        float moveSpeed = context.attributes.movement.maxSpeed * fleeArrival;

        context.movement.SetTargetMoveDirection(dir);
        context.movement.SetTargetMoveSpeed(moveSpeed);

        VisualizeMovement(dir, moveSpeed);
    }

    void Idle()
    {
        Vector2 dir = EnemyAIUtility.GetOscillationInPlace(Time.time, idleOscillationFreq, idleOscillationAmpl, this.gameObject.GetInstanceID());
        float idleSpeed = context.attributes.movement.maxSpeed * 0.5f;

        context.movement.SetTargetMoveDirection(dir);
        context.movement.SetTargetMoveSpeed(idleSpeed);

        VisualizeMovement(dir, idleSpeed);
    }

    void VisualizeMovement(Vector2 dir, float speed)
    {
        float speedScale = speed / context.attributes.movement.maxSpeed;
        dir *= speedScale;
        context.visualizer.SetVec2PropertyInstance("_Direction", dir);
    }
}
