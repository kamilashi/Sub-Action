using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterContext сontext;

    void Start()
    {
        сontext = GetComponent<CharacterContext>();

        InputHandler.onMoveInput.AddListener(OnMoveInputSubmitted);
        InputHandler.onStopMoveInput.AddListener(OnMoveInputStopped);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

       
    }

    private void OnMoveInputSubmitted(Vector2 input)
    {
        сontext.movement.SetTargetDirection(input);
        сontext.movement.SetTargetSpeed(сontext.attributes.movement.maxSpeed);
    }
    private void OnMoveInputStopped()
    {
        сontext.movement.SetTargetSpeed(0.0f);
    }
}
