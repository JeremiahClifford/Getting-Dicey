using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private enum CamState
    {
        Animating,
        Still
    }
    private CamState state = CamState.Still;
    private Vector3 destination;
    private Vector3 desiredRotation;
    private Vector3 originalDestination;
    private Vector3 originalRotation;
    private float timer = 0;
    private float animationTime = 0.5f;

    void Update()
    {
        if (state == CamState.Animating)
        {
            timer += Time.deltaTime;
            if (timer <= animationTime && destination != originalDestination && originalRotation != desiredRotation)
            {
                Vector3 newPos = new Vector3();
                newPos.x = Mathf.SmoothStep(originalDestination.x, destination.x, timer / animationTime);
                newPos.y = Mathf.SmoothStep(originalDestination.y, destination.y, timer / animationTime);
                newPos.z = Mathf.SmoothStep(originalDestination.z, destination.z, timer / animationTime);

                Vector3 newRot = new Vector3();
                newRot.x = Mathf.SmoothStep(originalRotation.x, desiredRotation.x, timer / animationTime);
                newRot.y = Mathf.SmoothStep(originalRotation.y, desiredRotation.y, timer / animationTime);
                newRot.z = Mathf.SmoothStep(originalRotation.z, desiredRotation.z, timer / animationTime);

                this.gameObject.transform.position = newPos;

                Quaternion newQuat = new Quaternion();
                newQuat.eulerAngles = newRot;
                this.gameObject.transform.rotation = newQuat;
            }
            else
            {
                this.gameObject.transform.position = destination;

                Quaternion newQuat = new Quaternion();
                newQuat.eulerAngles = desiredRotation;
                this.gameObject.transform.rotation = newQuat;

                state = CamState.Still;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    public void MoveTo(Vector3 pos, Vector3 rot, float time)
    {
        destination = pos;
        desiredRotation = rot;
        originalDestination = this.gameObject.transform.position;
        originalRotation = this.gameObject.transform.rotation.eulerAngles;
        animationTime = time;
        timer = 0;
        state = CamState.Animating;
    }

    public bool isAnimating()
    {
        if (state == CamState.Animating) 
        {
            return true;
        }
        return false;
    }
}
