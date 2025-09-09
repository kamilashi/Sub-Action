using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class HitReceiver : MonoBehaviour
{
    public UnityEvent<float> onBodyHit = new UnityEvent<float>();

    private void OnTriggerEnter(Collider other)
    {
        HitEmitter hitEmitter = other.GetComponent<HitEmitter>();
        onBodyHit?.Invoke(hitEmitter.damage);

        Debug.Log("Hit!");
    }
}
