using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    Sensor sensor;
    public float maxSpeed = 10.0f;
    public float acceleration = 3.0f;

    public float spawnSpeed = 5.0f;      
    public float arcHeight = 2.0f;
    public float spawnDistance = 2.0f;

    private void Awake()
    {
        sensor = GetComponent<Sensor>();
        sensor.onSensorEnter.AddListener(GetCollected);
    }

    private void GetCollected(Collider2D receiver, Sensor receiverSensor)
    {
        StartCoroutine(CollectedCoroutine(receiver.transform));
    }

    [ContextMenu("Test")]
    void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        StartCoroutine(SpawnCoroutine());
    }

    IEnumerator CollectedCoroutine(Transform receiver)
    {
        const float stopDistance = 0.5f;  
        float speed = 0f;

        while (receiver != null)
        {
            Vector3 toTarget = receiver.position - transform.position;
            float dist = toTarget.magnitude;
            if (dist <= stopDistance) break;

            Vector3 dir = toTarget / dist; 

            speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * Time.deltaTime);

            float step = speed * Time.deltaTime;

            float move = Mathf.Min(step, Mathf.Max(0f, dist - stopDistance));

            transform.Translate(dir * move, Space.World);

            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator SpawnCoroutine()
    {
        Vector2 dir2D = Random.insideUnitCircle.normalized;
        Vector3 flatDir = new Vector3(dir2D.x, dir2D.y, 0f);

        Vector3 start = transform.position;
        float traveled = 0f;

        while (traveled < spawnDistance)
        {
            float step = spawnSpeed * Time.deltaTime;
            traveled += step;
            float t = Mathf.Clamp01(traveled / spawnDistance);
            float height = Mathf.Sin(Mathf.PI * t) * arcHeight;

            Vector3 delta = flatDir * step;
            delta += Vector3.up * (height - Mathf.Sin(Mathf.PI * (t - step / spawnDistance)) * arcHeight);

            transform.Translate(delta, Space.World);

            yield return null;
        }
    }
}
