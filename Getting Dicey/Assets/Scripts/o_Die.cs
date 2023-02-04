using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class o_Die
{
    public int[] sides;

    public o_Die(int c_sides) {
        sides = new int[c_sides];
        for (int i = 0; i < sides.Length; i++) {
            sides[i] = i + 1;
        }
    }

    public int Roll() {
        return sides[Random.Range(0, sides.Length)];
    }
}