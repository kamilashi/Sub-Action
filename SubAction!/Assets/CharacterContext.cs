using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterContext : MonoBehaviour
{
    public CharacterMovement movement;
    public CharacterAction action;
    public CharacterAttributes attributes;
    public Rigidbody body;

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        action = GetComponent<CharacterAction>();
        attributes = GetComponent<CharacterAttributes>();
        body = GetComponent<Rigidbody>();

    }
}
