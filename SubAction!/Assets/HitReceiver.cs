using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class HitReceiver : MonoBehaviour
{
    public int entityId;

    public UnityEvent<int> onBodyHit = new UnityEvent<int>();

    private Dictionary<Collider2D, Coroutine> DOTs = new Dictionary<Collider2D, Coroutine>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        HitEmitter hitEmitter = other.GetComponent<HitEmitter>();

        if (hitEmitter.entityId == entityId)
        {
            return;
        }

        onBodyHit?.Invoke(hitEmitter.damage);

        if (hitEmitter.isDamageOverTime) 
        {
            Coroutine coroutine = StartCoroutine(DamageOverTimeCoroutine(hitEmitter.damage));
            DOTs[other] = coroutine;
        }

 /*       if(hitEmitter.attachOnContact)
        {

        }*/

        Debug.Log("Hit!");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Coroutine dot = null;
        if(DOTs.TryGetValue(other, out dot))
        {
            StopCoroutine(dot);
            DOTs.Remove(other);
        }
    }

    IEnumerator DamageOverTimeCoroutine(int damage)
    {
        while (true)
        {
            yield return new WaitForSeconds(HitEmitter.timeIntervalInSeconds);
            onBodyHit?.Invoke(damage);
        }
    }
}
