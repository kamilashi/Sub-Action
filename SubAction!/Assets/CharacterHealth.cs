using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Health
{
    public int maxHealth;
}

public class CharacterHealth : MonoBehaviour
{
    public UnityEvent<int> onDamaged = new UnityEvent<int>();
    public int currentHealth;
    public bool isInvincible;

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

    public void RemoveHealth(int delta)
    {
        if(isInvincible)
        {
            return;
        }

        int oldHealth = currentHealth;
        currentHealth -= delta;
        currentHealth = Math.Max(currentHealth, 0);

        onDamaged?.Invoke(oldHealth - currentHealth);
    }

    public void AddHealth(int delta)
    {
        currentHealth += delta;
        currentHealth = Math.Min(currentHealth, context.attributes.health.maxHealth);
    }

    // might need to be changed to bitfield to support differetn sources
    public void SetInvincible(bool enabled)
    {
        isInvincible = enabled;
    }

}
