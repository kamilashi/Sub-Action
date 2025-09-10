using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyDeath : MonoBehaviour
{
    CharacterContext context;
    public int rewardCurrencyAmount;

    public UnityEvent onStartDying = new UnityEvent();

    void Start()
    {
        context = GetComponent<CharacterContext>();
        context.health.onDamaged.AddListener(onDamaged);
    }

    void onDamaged(int delta)
    {
        if(context.health.currentHealth == 0)
        {
            SpawnReward();
        }

        onStartDying?.Invoke();

        context.visualizer.OnStartDying(Despawn);
    }

    void SpawnReward()
    {
        for (int i = 0; i < rewardCurrencyAmount; i++)
        {
            GameObject collectible = Instantiate(GameManager.Instance.currencyPrefab, transform.position, Quaternion.identity);
            collectible.transform.parent = null;
        }
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}
