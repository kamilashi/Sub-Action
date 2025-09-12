using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Sensor : MonoBehaviour
{
    public int entityId;
    public CharacterContext context;
/*
    public UnityEvent<Collider2D, CharacterContext> onSensorEnterCreature = new UnityEvent<Collider2D, CharacterContext>();
    public UnityEvent<Collider2D, CharacterContext> onSensorExitCreature = new UnityEvent<Collider2D, CharacterContext>();*/
    public UnityEvent<Collider2D, Sensor> onSensorEnter = new UnityEvent<Collider2D, Sensor>();
    public UnityEvent<Collider2D, Sensor> onSensorExit = new UnityEvent<Collider2D, Sensor>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        Sensor otherSensor;
        if (other.gameObject.TryGetComponent<Sensor>(out otherSensor))
        {
            if (otherSensor.entityId == entityId)
            {
                return;
            }

            onSensorEnter?.Invoke(other, otherSensor);
        }
    }
 
    private void OnTriggerExit2D(Collider2D other)
    {
        Sensor otherSensor;
        if (other.gameObject.TryGetComponent<Sensor>(out otherSensor))
        {
            if (otherSensor.entityId == entityId)
            {
                return;
            }

            onSensorExit?.Invoke(other, otherSensor);
        }
    }
}
