using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public class CameraConstraints
{
    public float minZ;
    public float maxZ;
}

[Serializable]
public class RumbleSettings
{
    public float duration;
    public float maxRollDeg;

    public RumbleSettings(float duration, float maxRoll)
    {
        this.duration = duration;
        maxRollDeg = maxRoll;
    }
}

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    public CameraConstraints constraints;

    public RumbleSettings testRumble;

    public float zoomSensitivity = 0.5f;
    public float targetZ;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        targetZ = transform.position.z;
        InputHandler.Own.onVerticalScoll.AddListener(OnZoomInput);
    }

    void FixedUpdate()
    {
        float currentTargetZ = transform.position.z;
        currentTargetZ = Mathf.Lerp(currentTargetZ, targetZ, Time.deltaTime);
        transform.Translate(new Vector3(0.0f, 0.0f, currentTargetZ - transform.position.z));
    }

    void OnZoomInput(float amount)
    {
        targetZ = transform.position.z + amount * zoomSensitivity;
        targetZ = Mathf.Clamp(targetZ, constraints.minZ, constraints.maxZ);
    }

    [ContextMenu("TestRumble")]
    void TestRumble()
    {
        TriggerRumple(testRumble);
    }

    public void TriggerRumple(RumbleSettings rumble)
    {
        StartCoroutine(RumbleCoroutine(rumble));
    }

    IEnumerator RumbleCoroutine(RumbleSettings rumble)
    {
        float time = 0.0f;
        while (time < rumble.duration) 
        {
            float progress = time / rumble.duration;
            progress = Mathf.Clamp01(progress);

            float shake = Library.SmoothingFuncitons.SyncDecay(progress, 1.65f, 10.0f, 0.33f, -7.85f);

            float angle = rumble.maxRollDeg * shake;

            transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            time += Time.deltaTime;

            yield return null;  
        }
    }
}
