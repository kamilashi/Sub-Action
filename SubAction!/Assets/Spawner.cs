using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo: unify with collectibles spawn logic
public class Spawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public int amount;
    public float minDistance;
    public float maxDistance;
    public bool spawnerIsParent;

    void Awake()
    {
        for (int i = 0; i < amount; i++)
        {
            float distance = Random.Range(minDistance, maxDistance);
            Vector2 dir2D = Random.insideUnitCircle.normalized;
            Vector3 flatDir = new Vector3(dir2D.x, dir2D.y, 0f);

            Vector3 position = transform.position + flatDir * distance;

            GameObject spawn = Instantiate(prefabToSpawn, position, Quaternion.identity);

            if(spawnerIsParent)
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
