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
        Debug.Assert(hitEmitter != null);
        isInitialized = true;
        this.enabled = false;
    }

    /*

    protected virtual void OnEnable()
    {
        if (!isInitialized)
        {
            return;
        }
    }*/
}