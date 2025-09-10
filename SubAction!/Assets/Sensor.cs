using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Sensor : MonoBehaviour
{
    public UnityEvent<Collider2D> onSensorEnter = new UnityEvent<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject == this.gameObject)
        {
            return;
        }

        onSensorEnter?.Invoke(other);
    }
}
