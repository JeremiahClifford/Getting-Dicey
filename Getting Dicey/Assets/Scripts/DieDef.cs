using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DieDef", menuName = "GettingDicey/DieDef")]
public class DieDef : ScriptableObject
{
    [SerializeField]
    public string dieName, dieDescription;

    [SerializeField]
    public DiceManager.DieIndex index;

    [SerializeField]
    public float cost, payout;

    [SerializeField]
    public Die prefab;

    [SerializeField]
    public List<int> sideNums = new List<int> { 0, 1, 2, 4, 5, 6 };
}
