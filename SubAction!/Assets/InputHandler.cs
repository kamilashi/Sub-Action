using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    // if there is ever a multiplayer these must be changed to member events
    public static UnityEvent<Vector2> onMoveInput = new UnityEvent<Vector2>();
    public static UnityEvent onStopMoveInput = new UnityEvent();
    public static UnityEvent onPrimaryAction = new UnityEvent();
    public static UnityEvent onSecondaryAction = new UnityEvent();
    public static UnityEvent onSpecialAction = new UnityEvent();

    public Vector2 lastInput;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        Vector2 newInput = new Vector2(0.0f, 0.0f);

        Keyboard keyboard = Keyboard.current;
        if (keyboard.wKey.isPressed)
        {
            newInput.y = 1;
        }

        if (keyboard.sKey.isPressed)
        {
            newInput.y = -1;
        }

        if (keyboard.dKey.isPressed)
        {
            newInput.x = 1;
        }

        if (keyboard.aKey.isPressed)
        {
            newInput.x = -1;
        }

        if(newInput.sqrMagnitude > 0)
        {
            newInput = newInput.normalized;
            onMoveInput?.Invoke(newInput);
        }
        else if (lastInput.sqrMagnitude > 0)
        {
            newInput = newInput.normalized;
            onStopMoveInput?.Invoke();
        }

        lastInput = newInput;

        Mouse mouse = Mouse.current;
        if (mouse.leftButton.isPressed)
        {
            onPrimaryAction?.Invoke();
        }

        if (mouse.rightButton.isPressed)
        {
            onSecondaryAction?.Invoke();
        }

        if (keyboard.leftShiftKey.isPressed)
        {
            onSpecialAction?.Invoke();
        }
    }
}
