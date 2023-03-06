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
        Odds = 3
    }

    private static List<DieDef> dieList = new List<DieDef>();

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
        die.dieName = def.name;
        die.description = def.dieDescription;
        die.price = def.cost;
        die.sides = def.sideNums;
        die.earnings = def.earnings;
        return die;
    }

    public static Die GetRandom()
    {
        DieDef def = dieList[Random.Range(0, dieList.Count - 1)];
        Die die = GameObject.Instantiate<Die>(def.prefab);
        die.dieName = def.name;
        die.description = def.dieDescription;
        die.price = def.cost;
        die.sides = def.sideNums;
        die.earnings = def.earnings;
        return die;
    }
}
