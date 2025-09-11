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

        InputHandler.Current.onMoveInput.AddListener(OnMoveInputSubmitted);
        InputHandler.Current.onAimInput.AddListener(OnAimInputSubmitted);
        InputHandler.Current.onStopMoveInput.AddListener(OnMoveInputStopped);
        
        InputHandler.Current.onPrimaryAction.AddListener(OnPrimaryAction);
        InputHandler.Current.onSecondaryAction.AddListener(OnSecondaryAction);
        InputHandler.Current.onSpecialAction.AddListener(OnSpecialAction);

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
    private void CollectCurrency(Collider2D other)
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

    private bool IsDashActive()
    {
        ActionBehavior activeAction = сontext.action.activeAction;
        return activeAction != null && activeAction.isRunning && activeAction.id == 1;
    }
}
