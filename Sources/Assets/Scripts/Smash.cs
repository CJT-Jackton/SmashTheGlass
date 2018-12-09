using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Smash : MonoBehaviour
{
    public Slider slider;
    public Text text;

    Rigidbody rigidbody;
    float MAX_THRUST = 1000.0f;

    private float normalDistribution(float x)
    {
        float stdDev = 0.25f;
        float mean = 0;
        return Mathf.Exp(-Mathf.Pow((x - mean), 2.0f) / (2.0f * stdDev * stdDev));
    }

    void Start()
    {
        UpdateText();
        slider.onValueChanged.AddListener(delegate { UpdateText(); });
    }

    void UpdateText()
    {
        text.text = "Force: " + MAX_THRUST * slider.value;
    }

    public void SmashIt(RaycastHit hit)
    {
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Piece"))
            {
                rigidbody = child.GetComponent<Rigidbody>();

                if (rigidbody != null && rigidbody.constraints != RigidbodyConstraints.None)
                {
                    rigidbody.constraints = RigidbodyConstraints.None;
                    rigidbody.useGravity = true;

                    Renderer renderer = child.GetComponent<Renderer>();

                    Vector3 force = transform.forward.normalized * slider.value * MAX_THRUST;

                    if (!renderer.bounds.Contains(hit.point))
                    {
                        float dist = Vector3.Distance(child.GetComponent<Renderer>().bounds.center, hit.point);
                        force *= normalDistribution(dist);
                    }

                    if (force.z > 25)
                    {
                        rigidbody.AddForceAtPosition(force, hit.point);
                    }
                    else
                    {
                        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                        rigidbody.useGravity = false;
                    }
                }
            }
        }
    }
}
