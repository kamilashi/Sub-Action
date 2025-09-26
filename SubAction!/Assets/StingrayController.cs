using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class StingrayController : MonoBehaviour
{
    CharacterContext context;
    EnemySteering steering;
    public float idleOscillationFreq = 0.3f;
    public float idleOscillationAmpl = 0.09f;
    [Range(0.0f, 1.0f)]public float steeringWeight = 0.5f;
    public float steeringAddedSpeedMulti = 3.0f;
    public float maxStrayFromSpawnerDistance = 20.0f;
    public bool followTarget = false;
    public bool stayWithinSpawner = true;

    void Start()
    {
        context = GetComponent<CharacterContext>();
        steering = GetComponent<EnemySteering>();
    }

    void Update()
    {
        float targetFollowWeight = 1.0f - steeringWeight;
        float speed = context.attributes.movement.maxSpeed;

        Vector2 movementDir = Vector2.zero;
        if (steering.hasSteeringDirection)
        {
            movementDir += steering.steeringDirectionRaw * steeringWeight;
            speed *= Mathf.Min(steeringAddedSpeedMulti,  1 / Mathf.Clamp(movementDir.magnitude, 0.01f, 1.0f));
        }

        if(stayWithinSpawner && transform.parent != null) // keep close to the spawner parent if there is one
        {
            Vector2 toTarget = transform.parent.position - transform.position;
            float toSpawnerFactor = toTarget.magnitude / maxStrayFromSpawnerDistance;
            toSpawnerFactor *= toSpawnerFactor;
            movementDir += toTarget.normalized * toSpawnerFactor;
        }

        if (followTarget && steering.hasTarget)
        {
            Vector2 toTarget = steering.target.position - transform.position; 
            float attract = steering.targetLabel == Label.Danger ? -1.0f : 1.0f;
            movementDir += attract * targetFollowWeight * toTarget.normalized;
            //speed += context.attributes.movement.maxSpeed * 1.0f / toTarget.magnitude;
        }
        else
        {
            movementDir += targetFollowWeight * EnemyAIUtility.GetOscillationInPlace(Time.time, idleOscillationFreq, idleOscillationAmpl, this.gameObject.GetInstanceID());
        }

        movementDir.Normalize();
             
        if (movementDir.magnitude > 0.0)
        {
            context.movement.SetTargetAimDirection(movementDir);
            context.movement.SetTargetMoveDirection(movementDir);
            context.movement.SetTargetMoveSpeed(speed);
        }
    }

}
