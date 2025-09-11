using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Update()
    {
        currentMoveSpeed = Library.SmoothingFuncitons.ApproachReferenceLinear(currentMoveSpeed, targetMoveSpeed, context.attributes.movement.acceleration * Time.deltaTime);
        currentMoveDirection = Library.SmoothingFuncitons.ApproachReferenceLinear(currentMoveDirection, targetMoveDirection, context.attributes.movement.turnSpeed * Time.deltaTime);
        currentAimDirection = Library.SmoothingFuncitons.ApproachReferenceLinear(currentAimDirection, targetAimDirection, context.attributes.movement.aimSpeed * Time.deltaTime);

        context.rigidBody.velocity = currentMoveDirection * currentMoveSpeed;

        float roll = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(roll, Vector3.forward);

        context.rigidBody.MoveRotation(targetRotation);
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
