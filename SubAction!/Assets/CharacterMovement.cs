using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Movement
{
    public float acceleration = 2.0f;
    public float turnSpeed = 5.0f;
    public float aimSpeed = 5.0f;
    public float maxSpeed = 10.0f;
}
public class CharacterMovement : MonoBehaviour
{
    CharacterContext context;

    public Vector2 targetMoveDirection;
    public Vector2 currentMoveDirection;

    public float targetMoveSpeed;
    public float currentMoveSpeed;

    public Vector2 targetAimDirection;
    public Vector2 currentAimDirection;

    void Start()
    {
        context = GetComponent<CharacterContext>();
    }

    void FixedUpdate()
    {
        float timeStep = Time.fixedDeltaTime;
        currentMoveSpeed = Library.SmoothingFuncitons.ApproachReferenceLinear(currentMoveSpeed, targetMoveSpeed, context.attributes.movement.acceleration * timeStep);
        currentMoveDirection = Library.SmoothingFuncitons.ApproachReferenceLinear(currentMoveDirection, targetMoveDirection, context.attributes.movement.turnSpeed * timeStep);
        currentAimDirection = Library.SmoothingFuncitons.ApproachReferenceLinear(currentAimDirection, targetAimDirection, context.attributes.movement.aimSpeed * timeStep);

        context.rigidBody.MovePosition((Vector2)context.rigidBody.transform.position + currentMoveDirection * currentMoveSpeed * timeStep);

        float roll = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;

        context.rigidBody.MoveRotation(roll);
    }

    public void SetTargetMoveDirection(Vector2 direction)
    {
        targetMoveDirection = direction;
    }
    public void ForceMoveDirection(Vector2 direction)
    {
        currentMoveDirection = direction;
        targetMoveDirection = direction;
    }

    public void SetTargetAimDirection(Vector2 direction)
    {
        targetAimDirection = direction;
    }

    public void ForceAimDirection(Vector2 direction)
    {
        currentAimDirection = direction;
        targetAimDirection = direction;
    }

    public void SetTargetMoveSpeed(float speed)
    {
        targetMoveSpeed = speed;
    }
    public void ForceMoveSpeed(float speed)
    {
        currentMoveSpeed = speed;
        targetMoveSpeed = speed;
    }
}
