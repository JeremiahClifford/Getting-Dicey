using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dice", menuName = "Getting Dicey/Dice Preset")]
public class Dice : ScriptableObject
{
    [SerializeField]
    public List<int> values = new List<int>();
}