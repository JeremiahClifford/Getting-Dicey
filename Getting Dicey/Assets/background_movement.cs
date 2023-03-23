using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class background_movement : MonoBehaviour
{

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    // Use this for initialization
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        //float translateX = Mathf.SmoothStep(-min, min, Mathf.PingPong(Time.time * (float)(10), 1));

        //transform.Translate(new Vector3(0, 0, translateX));

        transform.position = new Vector3(Mathf.Lerp(minX, maxX, Mathf.PingPong(Time.time/10, 1)), Mathf.Lerp(minY, maxY, Mathf.PingPong(Time.time * 5 , 1)), 100);

        //transform.position = new Vector3(Mathf.PingPong( Time.deltaTime, max - min) + min, transform.position.y, transform.position.z);

    }
}
