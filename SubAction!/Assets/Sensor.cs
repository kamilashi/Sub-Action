using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sensor : MonoBehaviour
{
    public UnityEvent<Collider> onSensorEnter = new UnityEvent<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == this.gameObject)
        {
            return;
        }

        onSensorEnter?.Invoke(other);
    }
}
