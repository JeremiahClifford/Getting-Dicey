using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class D6 : Die
{
    [SerializeField]
    private List<TMP_Text> sideTexts = new List<TMP_Text>();

    public void Start()
    {
        for (int i = 0; i < sides.Count; i++)
        {
            sideTexts[i].text = sides[i].ToString();
        }
    }

    override protected Vector3 HitVector(int side)
    {
        switch (side)
        {
            case 1: return new Vector3(0F, 0F, 1F);
            case 2: return new Vector3(0F, -1F, 0F);
            case 3: return new Vector3(-1F, 0F, 0F);
            case 4: return new Vector3(1F, 0F, 0F);
            case 5: return new Vector3(0F, 1F, 0F);
            case 6: return new Vector3(0F, 0F, -1F);
        }
        return Vector3.zero;
    }
}
