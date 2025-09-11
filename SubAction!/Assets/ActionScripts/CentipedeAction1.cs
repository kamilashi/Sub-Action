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
        float initialiSpring = controller.spring;

        float time = 0.0f;
        float combinedLength = anticipationDuration + chargeDuration;

        while (time < combinedLength) 
        {
            time += Time.deltaTime;

            float ramp = Mathf.Clamp01(time / anticipationDuration);
            float progress = time / combinedLength;

            float freq;
            if (time > anticipationDuration) // charge 
            {
                float blend = Mathf.Sin(time * 2f * Mathf.PI * (1 / (anticipationDuration * 4.0f)));
                freq = Mathf.Lerp(frequencyHzMax, frequencyHzMin, blend);
                //controller.spring = Mathf.Lerp(0.0f, initialiSpring, progress * progress);
            }
            else // anticipation
            {
                float blend = Mathf.Sin(time * 2f * Mathf.PI * (1 / (chargeDuration * 4.0f)));
                freq = Mathf.Lerp(frequencyHzMin, frequencyHzMax, blend);
                controller.spring = Mathf.Lerp(initialiSpring , 0.0f, ramp * 3.0f);
                controller.damping = Mathf.Lerp(initialDamping, minDamping, ramp * 5.0f); // + no spring = rigid turns
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
            context.movement.SetTargetAimDirection(facing);
            context.movement.ForceMoveDirection(moveDirection);

            yield return null;
        }

        time = 0.0f;
        context.movement.SetTargetMoveSpeed(0.0f);

        float currentDamping = controller.damping;
        float currentSpring = controller.spring;

        while (time < cooldownDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01( time / cooldownDuration);
            float ease = progress * progress;

            controller.damping = Mathf.Lerp(/*minDamping*/ currentDamping, initialDamping, ease);
            controller.spring = Mathf.Lerp(currentSpring, initialiSpring, ease);
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
