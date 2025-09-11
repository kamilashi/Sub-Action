using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.GraphicsBuffer;

public class CentipedeAction1 : ActionBehavior
{
    [Header("Charge")]
    public CentipedeController controller;
    public float frequencyHzMax = 1.5f;
    public float anticipationDuration = 1.0f;
    public float maxSpeed = 50.0f;
    public float maxOscillationAngleDeg = 20.0f;
    public float frequencyHzMin = 0.5f;
    public float chargeDuration = 0.5f;
    public float cooldownDuration = 1.0f;

    public float minDamping = 0.3f;

    public float stopBeforeTargetDistance = 2.0f;

    public void OnEnable()
    {
        if(!isInitialized)
        {
            return;
        }

        StartCoroutine(ChargeCoroutine());
    }

    IEnumerator ChargeCoroutine()
    {
        isRunning = true;

        Vector2 moveDirection = (controller.target.position - context.rigidBody.transform.position);

        context.movement.SetTargetMoveSpeed(maxSpeed);

        float defaultSegmentLength = controller.tightLength;
        float initialDamping = controller.damping;

        float time = 0.0f;

        while (time < (anticipationDuration + chargeDuration)) 
        {
            time += Time.deltaTime;

            float ramp = time / anticipationDuration;
            controller.damping = Mathf.Lerp(initialDamping, minDamping, ramp);

            float freq;
            if (time > anticipationDuration)
            {
                float blend = Mathf.Sin(time * 2f * Mathf.PI * (1 / (anticipationDuration * 4.0f)));
                freq = Mathf.Lerp(frequencyHzMax, frequencyHzMin, blend);
            }
            else
            {
                float blend = Mathf.Sin(time * 2f * Mathf.PI * (1 / (chargeDuration * 4.0f)));
                freq = Mathf.Lerp(frequencyHzMin, frequencyHzMax, blend);
            }

            float angle = Mathf.Sin(time * 2f * Mathf.PI * freq) * maxOscillationAngleDeg;

            moveDirection = (controller.target.position - context.rigidBody.transform.position);
            float squareDistance = moveDirection.sqrMagnitude;

            if(squareDistance <= stopBeforeTargetDistance * stopBeforeTargetDistance)
            {
                break;
            }

            moveDirection.Normalize();

            Vector2 facing = Quaternion.Euler(0, 0, angle) * moveDirection;
            context.movement.ForceAimDirection(facing);
            context.movement.ForceMoveDirection(moveDirection);

            yield return null;
        }

        time = 0.0f;
        context.movement.SetTargetMoveSpeed(0.0f);

        while (time < cooldownDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01( time / cooldownDuration);

            controller.damping = Mathf.Lerp(minDamping, initialDamping, progress * progress);
            context.movement.SetTargetMoveSpeed(1 - progress);

            yield return null;
        }


        controller.damping = initialDamping;

        isOnCoolDown = true;
        isRunning = false;

        yield return new WaitForSeconds(data.cooldown);

        isOnCoolDown = false;
        this.enabled = false;
    }


}
