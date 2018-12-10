using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Smash : MonoBehaviour
{
    public Slider ThrustSlider;
    public Text ThrustText;

    public Slider StdDevSlider;
    public Text StdDevText;

    public Slider GlassStrengthSlider;
    public Text GlassStrengthText;

    void Start()
    {
        UpdateThrustText();
        ThrustSlider.onValueChanged.AddListener(delegate { UpdateThrustText(); });

        UpdateStdDevText();
        StdDevSlider.onValueChanged.AddListener(delegate { UpdateStdDevText(); });

        UpdateGlassStrengthText();
        GlassStrengthSlider.onValueChanged.AddListener(delegate { UpdateGlassStrengthText(); });
    }

    void UpdateThrustText()
    {
        ThrustText.text = "Force: " + ThrustSlider.value;
    }

    void UpdateStdDevText()
    {
        StdDevText.text = "Standard Dev: " + StdDevSlider.value;
    }

    void UpdateGlassStrengthText()
    {
        GlassStrengthText.text = "Glass strength: " + GlassStrengthSlider.value;
    }

    private float normalDistribution(float x)
    {
        float stdDev = StdDevSlider.value;
        float mean = 0;
        return Mathf.Exp(-Mathf.Pow((x - mean), 2.0f) / (2.0f * stdDev * stdDev));
    }

    public void SmashIt(RaycastHit hit)
    {
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Piece"))
            {
                Rigidbody rigidbody = child.GetComponent<Rigidbody>();

                if (rigidbody != null && rigidbody.constraints != RigidbodyConstraints.None)
                {
                    rigidbody.constraints = RigidbodyConstraints.None;
                    rigidbody.useGravity = true;

                    Renderer renderer = child.GetComponent<Renderer>();

                    Vector3 force = transform.forward.normalized * ThrustSlider.value;

                    if (!renderer.bounds.Contains(hit.point))
                    {
                        float dist = Vector3.Distance(child.GetComponent<Renderer>().bounds.center, hit.point);
                        force *= normalDistribution(dist);
                    }

                    if (force.z >= GlassStrengthSlider.value)
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
