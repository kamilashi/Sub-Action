using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class CharacterAttributes : MonoBehaviour
{
    public SOCharacterAttributes startData;
    public Movement movement;
    public Health health;
    public int rank;

    void Awake()
    {
        Debug.Assert(startData != null);

        InitializeData();
    }

    // might need to be split into static and dynamic data
    void InitializeData()
    {
        movement.acceleration = startData.movement.acceleration;
        movement.turnSpeed = startData.movement.turnSpeed;
        movement.maxSpeed = startData.movement.maxSpeed;

        health.maxHealth = startData.health.maxHealth;

        rank = startData.rank;
    }
}
