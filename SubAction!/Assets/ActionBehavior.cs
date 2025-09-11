using JetBrains.Annotations;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class ActionData
{
    public float minDistance;
    public float maxDistance;
    public float cooldown;
}

public abstract class ActionBehavior : MonoBehaviour
{
    [Header("Common")]
    public bool isRunning = false;
    public bool isOnCoolDown = false;

    public ActionData data;
    public HitEmitter hitEmitter;

    public int id = -1;

    [SerializeField] protected CharacterContext context;
    protected bool isInitialized = false;


    void Start()
    {
        context = GetComponentInParent<CharacterContext>();
        Debug.Assert(id >= 0);
        if(hitEmitter == null)
        {
           Debug.LogWarning($"Hit emitter for {this.name} is not set!");
        }

        isInitialized = true;
        this.enabled = false;
    }
}