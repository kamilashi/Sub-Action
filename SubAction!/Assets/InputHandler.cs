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
    public static UnityEvent<Vector2> onAimInput = new UnityEvent<Vector2>();
    public static UnityEvent onStopMoveInput = new UnityEvent();
    public static UnityEvent onPrimaryAction = new UnityEvent();
    public static UnityEvent onSecondaryAction = new UnityEvent();
    public static UnityEvent onSpecialAction = new UnityEvent();

    public Vector2 lastMoveInput;
    public Vector2 lastAimInput;

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
        else if (lastMoveInput.sqrMagnitude > 0)
        {
            newInput = newInput.normalized;
            onStopMoveInput?.Invoke();
        }

        lastMoveInput = newInput;

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

        //if(keyboard.leftAltKey.isPressed)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 aimDir = mouse.position.ReadValue() - screenCenter;
            aimDir = aimDir.normalized;

            if(aimDir.sqrMagnitude > 0)
            {
                onAimInput?.Invoke(aimDir);
            }

            lastAimInput = aimDir;
        }
    }
}
