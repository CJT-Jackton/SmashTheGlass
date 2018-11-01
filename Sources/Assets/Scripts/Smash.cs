﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smash : MonoBehaviour
{
    public float thrust = 100.0f;

    private Rigidbody rigidbody;

    private float normalDistribution(float x)
    {
        float stdDev = 1.0f;
        float mean = 0;
        return Mathf.Exp(-Mathf.Pow((x - mean), 2.0f) / (2.0f * stdDev * stdDev));
    }

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

                            Renderer renderer = child.GetComponent<Renderer>();

                            Vector3 force = transform.forward.normalized * thrust;

                            if (!renderer.bounds.Contains(hit.point))
                            {
                                float dist = Vector3.Distance(child.GetComponent<Renderer>().bounds.center, hit.point);
                                force *= normalDistribution(dist);
                            }

                            rigidbody.AddForceAtPosition(force, hit.point);
                        }
                    }
                }
            }
        }
    }
}
