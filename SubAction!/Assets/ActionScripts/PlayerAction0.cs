using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction0 : ActionBehavior
{
    [Header("Primary Attack")]
    public float attackDuration;

    public Renderer attackRenderer;
    private MaterialPropertyBlock mpb;

    void OnEnable()
    {
        if(!isInitialized)
        {
            return;
        }

        StartCoroutine(ActionCoroutine(attackDuration));
    }

    IEnumerator ActionCoroutine(float duration)
    {
        isRunning = true;

        float time = 0.0f;

        float anticipationDuration = attackDuration * 0.3f;


        mpb = new MaterialPropertyBlock();
        attackRenderer.GetPropertyBlock(mpb);

        System.Action VisualizeAttack = () =>
        {
            time += Time.deltaTime;

            float progress = time / duration;
            progress = Mathf.Clamp01(progress);
            float value = 1.0f - 2.0f * Mathf.Abs(progress - 0.5f);
            value *= value;

            mpb.SetFloat("_Progress1", value);
            attackRenderer.SetPropertyBlock(mpb);
        };

        while (time <= anticipationDuration)
        {
            VisualizeAttack();
            yield return null;
        }

        hitEmitter.gameObject.SetActive(true);

        while (time <= duration)
        {
            VisualizeAttack();
            yield return null;
        }

        hitEmitter.gameObject.SetActive(false);

        isRunning = false;
        isOnCoolDown = true;


        yield return new WaitForSeconds(data.cooldown);

        isOnCoolDown = false;
        this.enabled = false;
    }
}
