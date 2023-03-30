using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager
{
    public enum DieIndex
    {
        D6 = 0,
        CoinFlip = 1,
        Evens = 2,
        Odds = 3,
        UpperD6 = 4,
        UpperEvens = 5,
        LowerEvens = 6,
        UpperOdds = 7,
        LowerOdds = 8,
        D10 = 9,
        Ones = 10,
        Twos = 11,
        Threes = 12,
        HighRoller = 13,
        LowRoller = 14,
        Thirteen = 15,
        Pun = 16,
        HighestEvens = 17
    }

    private static List<DieDef> dieList = new List<DieDef>();

    public static int DieCount
    {
        get { return dieList.Count; }
    }

    public static void Init()
    {
        foreach (DieDef def in Resources.LoadAll<DieDef>(""))
        {
            dieList.Add(def);
        }
    }

    public static Die GetDie(DieIndex index)
    {
        DieDef def = null;
        foreach (DieDef d in dieList)
        {
            if (d.index == index)
            {
                def = d;
            }
        }
        Die die = GameObject.Instantiate<Die>(def.prefab);
        die.dieName = def.dieName;
        die.description = def.dieDescription;
        die.cost = def.cost;
        die.sides = def.sideNums;
        die.payout = def.payout;
        return die;
    }

    public static DieDef GetRandomDef()
    {
        int random = Random.Range(0, dieList.Count - 1);
        DieDef def = dieList[random];
        return def;
    }
}
