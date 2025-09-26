using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SpawnData
{
    public GameObject prefab;
    public int count;
}

public class Spawner : MonoBehaviour
{
    public float minDistance;
    public float maxDistance;
    public bool spawnerIsParent;
    public List<SpawnData> prefabsToSpawn;

    void Awake()
    {

        for (int i = 0; i < prefabsToSpawn.Count; i++)
        {
            for (int j = 0; j < prefabsToSpawn[i].count; j++)
            {
                float distance = UnityEngine.Random.Range(minDistance, maxDistance);
                Vector2 dir2D = UnityEngine.Random.insideUnitCircle.normalized;
                Vector3 flatDir = new Vector3(dir2D.x, dir2D.y, 0f);

                Vector3 position = transform.position + flatDir * distance;

                GameObject spawn = Instantiate(prefabsToSpawn[i].prefab, position, Quaternion.identity);

                if (spawnerIsParent)
                {
                    spawn.transform.parent = this.transform;
                }
                else
                {
                    spawn.transform.parent = null;
                }
            }
        }
    }
}
