using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HitEmitter : MonoBehaviour
{
    public int entityId;

    public int damage;
    public bool isDamageOverTime;
    public bool attachOnContact;

    public static float timeIntervalInSeconds = 1.0f;
}
