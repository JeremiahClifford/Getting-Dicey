using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberEffect : MonoBehaviour
{
    Camera cam;
    [SerializeField]
    private TMP_Text text;
    private float timer = 0f;
    private float timeAlive = 1.5f;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        text.gameObject.transform.localPosition = new Vector3(text.gameObject.transform.localPosition.x, text.gameObject.transform.localPosition.y + 5 * Time.deltaTime, -4);
        timer += Time.deltaTime;
        text.color = new Color(text.color.r, text.color.g, text.color.b, (timeAlive - timer) / timeAlive);
        if (timer >= timeAlive)
        {
            Destroy(this.gameObject);
        }
    }

    void LateUpdate()
    {
        transform.LookAt(cam.transform);
        transform.Rotate(0, 180, 0);
        Quaternion newQuat = new Quaternion();
        Vector3 angles = transform.rotation.eulerAngles;
        angles.y = 0;
        newQuat.eulerAngles = angles;
        transform.rotation = newQuat;
    }

    public void SetAmount(float num)
    {
        text.text = string.Format("{0:0}", num);
    }
}
