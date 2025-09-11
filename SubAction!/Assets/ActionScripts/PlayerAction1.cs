using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerAction1 : ActionBehavior
{
    [Header("Dash")]

    public float dashDistance;
    public float dashDuration;

    void OnEnable()
    {
        if(!isInitialized)
        {
            return;
        }

        StartCoroutine(DashCoroutine());
    }

    IEnumerator DashCoroutine()
    {
        isRunning = true;

        float dashSpeed = (dashDistance / dashDuration) * 2.0f;
        float time = 0.0f; 

        context.movement.ForceMoveSpeed(dashSpeed);
        context.movement.ForceAimDirection(context.movement.currentAimDirection); // freeze
        context.movement.ForceMoveDirection(context.movement.currentAimDirection);

        context.health.SetInvincible(true);

        while (time < dashDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / dashDuration);
            progress *= progress;
            context.movement.currentMoveSpeed = (dashSpeed * (1.0f - progress));
            context.movement.targetMoveSpeed = context.movement.currentMoveSpeed;

            yield return null;
        }

        context.health.SetInvincible(false);
        context.movement.ForceMoveSpeed(0.0f);

        isOnCoolDown = true;
        isRunning = false;

        yield return new WaitForSeconds(data.cooldown);

        isOnCoolDown = false;
        enabled = false;
    }
}
