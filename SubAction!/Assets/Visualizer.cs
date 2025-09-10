using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class VisualizerData
{
    public Color mainColor;
}

public class Visualizer : MonoBehaviour
{
    public Renderer bodyRenderer;
    public float hitDuration = 1.0f;
    public float dissolveDuration = 1.5f;

    private MaterialPropertyBlock visualizerMPB;
    private CharacterContext context;

    void Start()
    {
        context = GetComponentInParent<CharacterContext>();

        visualizerMPB = new MaterialPropertyBlock();
        Debug.Assert(bodyRenderer != null);

        bodyRenderer.GetPropertyBlock(visualizerMPB);
        visualizerMPB.SetColor("_MainColor", context.attributes.startData.visualizer.mainColor);
        bodyRenderer.SetPropertyBlock(visualizerMPB);


        context.health.onDamaged.AddListener(OnDamaged);
    }

    void Update()
    {
        
    }

    public void OnDamaged(int amount)
    {
        if(amount > 0)
        {
            StartCoroutine(ParameterBlendInOutCoroutine("_Damaged", hitDuration));
        }
    }

    public void OnStartDying(Action onFinichedAction)
    {
        StartCoroutine(ParameterBlendInCoroutine("_Dissolve", dissolveDuration, onFinichedAction));
    }

    public void SetFloatProperty(string parameterName, float value)
    {
        bodyRenderer.GetPropertyBlock(visualizerMPB);
        visualizerMPB.SetFloat(parameterName, value);
        bodyRenderer.SetPropertyBlock(visualizerMPB);
    }

    IEnumerator ParameterBlendInOutCoroutine(string parameterName, float duration)
    {
        bodyRenderer.GetPropertyBlock(visualizerMPB);

        float damagedValue = 0.0f;
        float time = 0.0f;

        while (time <= duration)
        {
            time += Time.deltaTime;

            float progress = time / duration;
            progress = Mathf.Clamp01(progress);
            damagedValue = 1.0f - 2.0f * Mathf.Abs(progress - 0.5f);
            damagedValue *= damagedValue;
            visualizerMPB.SetFloat(parameterName, damagedValue);
            bodyRenderer.SetPropertyBlock(visualizerMPB);
            yield return null;
        }
    }
    IEnumerator ParameterBlendInCoroutine(string parameterName, float duration, Action onFinishedAction = null)
    {
        bodyRenderer.GetPropertyBlock(visualizerMPB);

        float time = 0.0f;

        while (time <= duration)
        {
            time += Time.deltaTime;

            float progress = time / duration;
            progress = Mathf.Clamp01(progress);
            visualizerMPB.SetFloat(parameterName, progress);
            bodyRenderer.SetPropertyBlock(visualizerMPB);
            yield return null;
        }

        onFinishedAction?.Invoke();
    }
}
