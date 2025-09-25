using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Sensor : MonoBehaviour
{
    public int entityId;
    public CharacterContext context;

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

            //Debug.Log(context.name + " sensor" + this.GetInstanceID() + ": sensed " + otherSensor.entityId);
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

            //Debug.Log(context.name + " sensor" + this.GetInstanceID() + ": lost " + otherSensor.entityId);
            onSensorExit?.Invoke(other, otherSensor);
        }
    }
}
