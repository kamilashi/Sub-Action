using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public LayerMask attractorLayers;
    public LayerMask sensorLayers;
    //public LayerMask collectibleLayers;

    public GameObject currencyPrefab;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
