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
    public Rigidbody rigidBody;


    public List<HitReceiver> hitReceivers;
    //public List<HitEmitter> hitEmitters;


    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        action = GetComponent<CharacterAction>();
        attributes = GetComponent<CharacterAttributes>();
        health = GetComponent<CharacterHealth>();
        rigidBody = GetComponent<Rigidbody>();

        visualizer = GetComponentInChildren<Visualizer>();

        hitReceivers = GetComponentsInChildren<HitReceiver>().ToList();
        //hitEmitters = GetComponentsInChildren<HitEmitter>().ToList();

        Debug.Assert(hitReceivers.Count > 0);
    }
}
