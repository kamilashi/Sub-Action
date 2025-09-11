using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    // if there is ever a multiplayer these must be changed to member events
    public Camera playerCamera;

    public  UnityEvent<Vector2> onMoveInput = new UnityEvent<Vector2>();
    public  UnityEvent<Vector2> onAimInput = new UnityEvent<Vector2>();
    public  UnityEvent onStopMoveInput = new UnityEvent();
    public  UnityEvent onPrimaryAction = new UnityEvent();
    public  UnityEvent onSecondaryAction = new UnityEvent();
    public  UnityEvent onSpecialAction = new UnityEvent();

    public UnityEvent<float> onVerticalScoll = new UnityEvent<float>();

    public static InputHandler Current;

    public Vector2 lastMoveInput;
    public Vector2 lastAimInput;

    void Awake()
    {
        Current = this;
        Debug.Assert(playerCamera != null);   
    }

    void Update()
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
        if (mouse.leftButton.wasPressedThisFrame)
        {
            onPrimaryAction?.Invoke();
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            onSecondaryAction?.Invoke();
        }

        if (keyboard.leftShiftKey.wasPressedThisFrame)
        {
            onSpecialAction?.Invoke();
        }

        //if(keyboard.leftAltKey.isPressed)
        {
            Vector2 aimDir = lastAimInput;
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Ray ray = playerCamera.ScreenPointToRay(mouseScreen);
            Plane xyPlane = new Plane(Vector3.forward, Vector3.zero);

            if (xyPlane.Raycast(ray, out float enter))
            {
                Vector3 mouseWorld = ray.GetPoint(enter);

                aimDir = (mouseWorld - transform.position);
                aimDir.Normalize();

                if (aimDir.sqrMagnitude > 0)
                {
                    onAimInput?.Invoke(new Vector2(aimDir.x, aimDir.y));
                }

                lastAimInput = aimDir;
            }
        }

        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if(scroll.y != 0)
        {
            onVerticalScoll?.Invoke(scroll.y);
        }
    }
}
