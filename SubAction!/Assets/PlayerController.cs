using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Inventory
{
    public int currencyAmount;
}

[Serializable]
public struct DamageResponce
{
    public int maxDamageRef;
    public RumbleSettings maxDamageRumble;
    public int minDamageRef;
    public RumbleSettings minDamageRumble;
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterContext сontext;
    [SerializeField] private CameraController cameraController;

    [SerializeField] private Inventory inevntory;

    public DamageResponce damageResponce;
    public Sensor currencySensor;

    void Start()
    {
        сontext = GetComponent<CharacterContext>();
        cameraController = CameraController.Instance;

        InputHandler.Own.onMoveInput.AddListener(OnMoveInputSubmitted);
        InputHandler.Own.onAimInput.AddListener(OnAimInputSubmitted);
        InputHandler.Own.onStopMoveInput.AddListener(OnMoveInputStopped);
        
        InputHandler.Own.onPrimaryAction.AddListener(OnPrimaryAction);
        InputHandler.Own.onSecondaryAction.AddListener(OnSecondaryAction);
        InputHandler.Own.onSpecialAction.AddListener(OnSpecialAction);

        сontext.health.onDamaged.AddListener(OnDamaged);

        Debug.Assert(currencySensor != null);

        currencySensor.onSensorEnter.AddListener(CollectCurrency);
    }

    private void OnPrimaryAction()
    {
        сontext.action.TryRun(0);
    }

    private void OnSecondaryAction()
    {
        сontext.action.TryRun(0);
    }

    private void OnSpecialAction()
    {
        сontext.action.TryRun(1);
    }

    private void OnMoveInputSubmitted(Vector2 input)
    {
        if (IsDashActive())
        {
            return;
        }

        сontext.movement.SetTargetMoveDirection(input);
        сontext.movement.SetTargetMoveSpeed(сontext.attributes.movement.maxSpeed);

        if(сontext.movement.currentMoveSpeed == 0.0f)
        {
            сontext.movement.ForceMoveDirection(input);
        }
    }

    private void OnAimInputSubmitted(Vector2 input)
    {
        сontext.movement.SetTargetAimDirection(input);
    }

    private void OnMoveInputStopped()
    {
        if(IsDashActive())
        {
            return;
        }

        сontext.movement.SetTargetMoveSpeed(0.0f);
    }

    // this belongs in the upgrade manager / inventory
    private void CollectCurrency(Collider2D other, Sensor otherSensor)
    {
        if(otherSensor.GetComponent<Collectible>() != null) // hacky for now, need an enum or something
        {
            AddCurrency(1);
        }
    }
    
    private void AddCurrency(int amount)
    {
        inevntory.currencyAmount += amount;
    }
    private void RemoveCurrency(int amount)
    {
        inevntory.currencyAmount += amount;
    }

    private void OnDamaged(int amount)
    {
        float intensity = (amount - damageResponce.minDamageRef) / damageResponce.maxDamageRef;

        float duration = Mathf.Lerp(damageResponce.minDamageRumble.duration, damageResponce.maxDamageRumble.duration, intensity);
        float roll = Mathf.Lerp(damageResponce.minDamageRumble.maxRollDeg, damageResponce.maxDamageRumble.maxRollDeg, intensity);

        cameraController.TriggerRumple(new RumbleSettings(duration, roll));
    }

    private bool IsDashActive()
    {
        ActionBehavior activeAction = сontext.action.activeAction;
        return activeAction != null && activeAction.isRunning && activeAction.id == 1;
    }
}
