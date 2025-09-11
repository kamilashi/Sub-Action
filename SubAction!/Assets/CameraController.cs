using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public struct CameraConstraints
{
    public float minZ;
    public float maxZ;
}

public class CameraController : MonoBehaviour
{
    public CameraConstraints constraints;

    public float zoomSensitivity = 0.5f;
    public float targetZ;

    void Start()
    {
        targetZ = transform.position.z;
        InputHandler.Current.onVerticalScoll.AddListener(OnZoomInput);
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
}
