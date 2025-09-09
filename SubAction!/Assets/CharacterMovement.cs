using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    CharacterContext context;

    public Vector2 targetMoveDirection;
    public Vector2 currentMoveDirection;

    public float tagetMoveSpeed;
    public float currentMoveSpeed;

    void Start()
    {
        context = GetComponent<CharacterContext>();
    }

    void FixedUpdate()
    {
        currentMoveSpeed = Library.SmoothingFuncitons.ApproachReferenceLinear(currentMoveSpeed, tagetMoveSpeed, context.attributes.movement.acceleration);
        currentMoveDirection = Library.SmoothingFuncitons.ApproachReferenceLinear(currentMoveDirection, targetMoveDirection, context.attributes.movement.turnSpeed);

        context.body.velocity = currentMoveDirection * currentMoveSpeed;
    }

    public void SetTargetDirection(Vector2 direction)
    {
        targetMoveDirection = direction;
    }

    public void SetTargetSpeed(float speed)
    {
        tagetMoveSpeed = speed;
    }
}
