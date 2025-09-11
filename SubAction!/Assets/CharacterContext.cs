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

        visualizer = GetComponentInChildren<Visualizer>();

        hitReceivers = GetComponentsInChildren<HitReceiver>().ToList();
        hitEmitters = GetComponentsInChildren<HitEmitter>().ToList();

        foreach (HitReceiver hitReceiver in hitReceivers)
        {
            hitReceiver.entityId = entityId;
        }

        foreach (HitEmitter hitEmitter in hitEmitters)
        {
            hitEmitter.entityId = entityId;
        }

        Debug.Assert(hitReceivers.Count > 0);
    }
}
