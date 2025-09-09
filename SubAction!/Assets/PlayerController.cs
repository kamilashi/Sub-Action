using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CurrencyData
{
    public int currentAmount;
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterContext сontext;
    [SerializeField] private CurrencyData currencyData;

    public Sensor currencySensor;

    void Start()
    {
        сontext = GetComponent<CharacterContext>();

        InputHandler.onMoveInput.AddListener(OnMoveInputSubmitted);
        InputHandler.onAimInput.AddListener(OnAimInputSubmitted);
        InputHandler.onStopMoveInput.AddListener(OnMoveInputStopped);
        
        InputHandler.onPrimaryAction.AddListener(OnPrimaryAction);
        InputHandler.onSecondaryAction.AddListener(OnSecondaryAction);

        Debug.Assert(currencySensor != null);

        currencySensor.onSensorEnter.AddListener(CollectCurrency);
    }

    private void OnPrimaryAction()
    {
        сontext.action.TryRun(0);
    }
    private void OnSecondaryAction()
    {
        сontext.action.TryRun(1);
    }

    private void OnMoveInputSubmitted(Vector2 input)
    {
        сontext.movement.SetTargetMoveDirection(input);
        сontext.movement.SetTargetMoveSpeed(сontext.attributes.movement.maxSpeed);

        if(сontext.movement.currentMoveSpeed == 0.0f)
        {
            сontext.movement.SetCurrentMoveDirection(input);
        }
    }

    private void OnAimInputSubmitted(Vector2 input)
    {
        сontext.movement.SetTargetAimDirection(input);
    }

    private void OnMoveInputStopped()
    {
        сontext.movement.SetTargetMoveSpeed(0.0f);
    }


    // this belongs in the upgrade manager / inventory
    private void CollectCurrency(Collider other)
    {
        AddCurrency(1);
    }
    
    private void AddCurrency(int amount)
    {
        currencyData.currentAmount += amount;
    }
    private void RemoveCurrency(int amount)
    {
        currencyData.currentAmount += amount;
    }
}
