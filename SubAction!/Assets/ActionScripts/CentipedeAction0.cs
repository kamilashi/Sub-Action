using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class CentipedeAction0 : ActionBehavior
{
    [Header("Lunge Attack")]
    public CentipedeController controller;
    public float anticipationDuration = 1.0f;
    public float attackDuration = 0.5f;
    public float attackDistance = 2.5f;
    public float elongateMultiplier = 1.5f;
    public float shrinkMultiplier = 0.5f;
    public float cooldownDuration = 2.0f;

    public void OnEnable()
    {
        if(!isInitialized)
        {
            return;
        }

        StartCoroutine(LungeAttackCoroutine());
    }

    IEnumerator LungeAttackCoroutine()
    {
        isRunning = true;

        float defaultSegmentLength = controller.tightLength;
        float minSegmentLength = controller.tightLength * shrinkMultiplier;
        float maxSegmentLength = controller.tightLength * elongateMultiplier;
        float backOffDistance = 1.0f;
        float maxBackoffSpeed = (backOffDistance * 2.0f) / anticipationDuration;

        float initialDamping = controller.damping;


        float time = 0.0f;
        while (time < anticipationDuration) 
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / anticipationDuration);
            float regress = 1.0f - progress;

            controller.damping = regress * regress; // contract
            controller.tightLength = Mathf.Lerp(defaultSegmentLength, minSegmentLength, progress);

            context.movement.SetTargetMoveSpeed(Library.SmoothingFuncitons.EaseOutCubic(maxBackoffSpeed * (regress)));
            context.movement.SetTargetMoveDirection(-context.rigidBody.transform.right);
            Vector2 toTarget = (controller.target.position - context.rigidBody.transform.position);
            context.movement.SetTargetAimDirection(toTarget.normalized);

            yield return null;
        }

        Vector2 facing = context.rigidBody.transform.right;
        float attackSpeed = (attackDistance / attackDuration) * 2.0f;

        context.movement.ForceMoveSpeed(attackSpeed);
        context.movement.ForceAimDirection (facing);
        context.movement.ForceMoveDirection (facing);

        hitEmitter.gameObject.SetActive(true);
        time = 0.0f;
        while (time < attackDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / attackDuration);
            controller.tightLength = Mathf.Lerp(minSegmentLength, maxSegmentLength, progress  * 2.0f);
            context.movement.ForceMoveSpeed(attackSpeed * (1.0f - progress * progress));

            yield return null;
        }
        hitEmitter.gameObject.SetActive(false);


        time = 0.0f;
        context.movement.SetTargetMoveSpeed(0.0f);
        controller.damping = 1.0f;

        while (time < cooldownDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01( time / cooldownDuration);

            controller.tightLength = Mathf.Lerp(maxSegmentLength, defaultSegmentLength, progress);
            controller.damping = Mathf.Lerp(1.0f, initialDamping, progress * progress);

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
