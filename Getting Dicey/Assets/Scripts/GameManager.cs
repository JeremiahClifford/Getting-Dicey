using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro moneyLabel;

    private float money;
    public float Money
    {
        get { return money; }
        set { money = value; }
    }

    public void Roll()
    {
    }
}
