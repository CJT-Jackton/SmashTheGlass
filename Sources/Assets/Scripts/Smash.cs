using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smash : MonoBehaviour
{
    public float thrust = 100.0f;

    private Rigidbody rigidbody;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    foreach (Transform child in transform)
                    {
                        rigidbody = child.GetComponent<Rigidbody>();

                        if (rigidbody.constraints != RigidbodyConstraints.None)
                        {
                            rigidbody.constraints = RigidbodyConstraints.None;
                            rigidbody.useGravity = true;
                            rigidbody.AddForceAtPosition(transform.forward.normalized * thrust, hit.point);
                        }
                    }
                }
            }
        }
    }
}
