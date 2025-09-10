using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform followTarget;
    void Start()
    {
        Debug.Assert(followTarget != null);
    }

    void Update()
    {
        Vector3 delta = followTarget.position - this.transform.position;
        delta.z = 0.0f;

        this.transform.Translate(delta);
    }
}
