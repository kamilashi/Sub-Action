using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterContext : MonoBehaviour
{
    public CharacterMovement movement;
    public CharacterAction action;
    public CharacterAttributes attributes;
    public CharacterHealth health;
    public Visualizer visualizer;
    public Rigidbody2D rigidBody;


    public List<HitReceiver> hitReceivers;
    public List<HitEmitter> hitEmitters;
    public List<Sensor> sensors;

    public int entityId;


    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        action = GetComponent<CharacterAction>();
        attributes = GetComponent<CharacterAttributes>();
        health = GetComponent<CharacterHealth>();

        entityId = gameObject.GetInstanceID();

        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        visualizer = GetComponentInChildren<Visualizer>(true);

        hitReceivers = GetComponentsInChildren<HitReceiver>(true).ToList();
        hitEmitters = GetComponentsInChildren<HitEmitter>(true).ToList();
        sensors = GetComponentsInChildren<Sensor>(true).ToList();

        foreach (HitReceiver hitReceiver in hitReceivers)
        {
            hitReceiver.entityId = entityId;
        }

        foreach (HitEmitter hitEmitter in hitEmitters)
        {
            hitEmitter.entityId = entityId;
        }

        foreach(Sensor sensor in sensors)
        {
            sensor.entityId = entityId;
            sensor.context = this;
        }

        Debug.Assert(hitReceivers.Count > 0);
    }
}
