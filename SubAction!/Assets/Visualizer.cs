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

        foreach (HitReceiver hitReceiver in context.hitReceivers)
        {
            hitReceiver.onBodyHit.AddListener(OnDamaged);
        }
    }

    void Update()
    {
        
    }

    public void OnDamaged(float amount)
    {
        StartCoroutine(InOutParameterBlendCoroutine("_Damaged", hitDuration));
    }

    public void SetFloatProperty(string parameterName, float value)
    {
        bodyRenderer.GetPropertyBlock(visualizerMPB);
        visualizerMPB.SetFloat(parameterName, value);
        bodyRenderer.SetPropertyBlock(visualizerMPB);
    }

    IEnumerator InOutParameterBlendCoroutine(string parameterName, float duration)
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
}
