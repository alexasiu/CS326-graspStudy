using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    public bool collided = false;
    public Collider objCollider;

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(this.name + " trigger: " + other.name);
        collided = true;
        objCollider = other;
    }

    void OnTriggerExit(Collider other)
    {
        collided = false;
        objCollider = other;
    }


}
