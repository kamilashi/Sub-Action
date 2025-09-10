using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class HitReceiver : MonoBehaviour
{
    public UnityEvent<int> onBodyHit = new UnityEvent<int>();

    private void OnTriggerEnter(Collider other)
    {
        HitEmitter hitEmitter = other.GetComponent<HitEmitter>();
        onBodyHit?.Invoke(hitEmitter.damage);

        Debug.Log("Hit!");
    }
}
