using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class slot_dance : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 1;
    public float RotAngleZ = 30;
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        float rZ = Mathf.SmoothStep(-RotAngleZ, RotAngleZ , Mathf.PingPong(Time.time * speed, 1));
        transform.rotation = Quaternion.Euler(0, 180, rZ);
    }
}
