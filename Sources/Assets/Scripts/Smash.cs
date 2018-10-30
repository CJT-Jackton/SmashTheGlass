using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smash : MonoBehaviour
{
    public float thrust = 100.0f;
    public new Rigidbody rigidbody;

    // Use this for initialization
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void OnMouseDown()
    {
        rigidbody.AddForceAtPosition(transform.up * -thrust, new Vector3(0, 1.63f, 0));
        rigidbody.useGravity = true;
        rigidbody.constraints = RigidbodyConstraints.None;
    }
}
