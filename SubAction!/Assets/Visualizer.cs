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
    public List<Renderer> bodyRenderers;
    //public Material mainMaterial;

    public float hitDuration = 1.0f;
    public float dissolveDuration = 1.5f;
    public bool setPerMaterialInstace = true;

    private MaterialPropertyBlock visualizerMPB;
    private CharacterContext context;

    void Start()
    {
        context = GetComponentInParent<CharacterContext>();

        visualizerMPB = new MaterialPropertyBlock();

        Debug.Assert(bodyRenderers != null);

        /*bodyRenderer.GetPropertyBlock(visualizerMPB);
        visualizerMPB.SetColor("_MainColor", context.attributes.startData.visualizer.mainColor);
        bodyRenderer.SetPropertyBlock(visualizerMPB);*/

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

    public void SetFloatPropertyInstance(string parameterName, float value)
    {
        foreach (Renderer renderer in bodyRenderers)
        {
            renderer.GetPropertyBlock(visualizerMPB);
            visualizerMPB.SetFloat(parameterName, value);
            renderer.SetPropertyBlock(visualizerMPB);
        }
    }

    public void SetFloatPropertyCopy(string parameterName, float value)
    {
        foreach (Renderer renderer in bodyRenderers)
        {
            renderer.material.SetFloat(parameterName, value);
        }
    }

    IEnumerator ParameterBlendInOutCoroutine(string parameterName, float duration)
    {

        float damagedValue = 0.0f;
        float time = 0.0f;

        while (time <= duration)
        {
            time += Time.deltaTime;

            float progress = time / duration;
            progress = Mathf.Clamp01(progress);
            damagedValue = 1.0f - 2.0f * Mathf.Abs(progress - 0.5f);
            damagedValue *= damagedValue;

            if (setPerMaterialInstace)
            {
                SetFloatPropertyInstance(parameterName, damagedValue);
            }
            else
            {
                SetFloatPropertyCopy(parameterName, damagedValue);
            }

            yield return null;
        }
    }

    IEnumerator ParameterBlendInCoroutine(string parameterName, float duration, Action onFinishedAction = null)
    {
        float time = 0.0f;

        while (time <= duration)
        {
            time += Time.deltaTime;

            float progress = time / duration;
            progress = Mathf.Clamp01(progress);

            if (setPerMaterialInstace)
            {
                SetFloatPropertyInstance(parameterName, progress);
            }
            else
            {
                SetFloatPropertyCopy(parameterName, progress);
            }

            yield return null;
        }

        onFinishedAction?.Invoke();
    }
}
