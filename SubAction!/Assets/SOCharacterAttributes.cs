using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CharacterAttributes")]
public class SOCharacterAttributes : ScriptableObject
{
    public Movement movement;
    public Health health; 
    public VisualizerData visualizer;
    public int rank;
}
