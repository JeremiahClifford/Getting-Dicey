using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Currently unused
/// </summary>
public class DieBehavior : MonoBehaviour
{
    private List<int> values = new List<int>();
    public List<int> Values
    {
        set
        {
            values = value;
        }
    }

    public int GetRoll() 
    {
        int index = Random.Range(0, values.Count);
        return values[index];
    }
}
