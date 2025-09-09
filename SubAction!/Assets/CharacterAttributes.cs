using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[Serializable]
public class Movement
{
    public float acceleration = 2.0f;
    public float turnSpeed = 5.0f;
    public float maxSpeed = 10.0f;
}

public class CharacterAttributes : MonoBehaviour
{
    public SOCharacterAttributes startData;
    public Movement movement;

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
    }
}
