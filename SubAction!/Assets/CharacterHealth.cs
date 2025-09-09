using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Health
{
    public float maxHealth;
}

public class CharacterHealth : MonoBehaviour
{
    public float currentHealth;

    private CharacterContext context;

    private void Start()
    {
        context = GetComponent<CharacterContext>();
        currentHealth = context.attributes.startData.health.maxHealth;

        foreach (HitReceiver hitReceiver in context.hitReceivers)
        {
            hitReceiver.onBodyHit.AddListener(RemoveHealth);
        }
    }

    public void RemoveHealth(float delta)
    {
        currentHealth -= delta;
        currentHealth = Mathf.Max(currentHealth, 0);

        //return currentHealth;
    }

    public void AddHealth(float delta)
    {
        currentHealth += delta;
        currentHealth = Mathf.Min(currentHealth, context.attributes.health.maxHealth);

        //return currentHealth;
    }

}
