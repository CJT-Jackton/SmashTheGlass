using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Smash : MonoBehaviour
{
    /// <summary>
    /// The slider controls thrust.
    /// </summary>
    public Slider ThrustSlider;
    public Text ThrustText;

    /// <summary>
    /// The slider controls standard deviation.
    /// </summary>
    public Slider StdDevSlider;
    public Text StdDevText;

    /// <summary>
    /// The slider controls glass strength.
    /// </summary>
    public Slider GlassStrengthSlider;
    public Text GlassStrengthText;

    /// <summary>
    /// Setup the event listener for UI elements.
    /// </summary>
    void Start()
    {
        UpdateThrustText();
        ThrustSlider.onValueChanged.AddListener(delegate { UpdateThrustText(); });

        UpdateStdDevText();
        StdDevSlider.onValueChanged.AddListener(delegate { UpdateStdDevText(); });

        UpdateGlassStrengthText();
        GlassStrengthSlider.onValueChanged.AddListener(delegate { UpdateGlassStrengthText(); });
    }

    #region Event Listener
    /// <summary>
    /// Update the text of thrust.
    /// </summary>
    void UpdateThrustText()
    {
        ThrustText.text = "Force: " + ThrustSlider.value;
    }

    /// <summary>
    /// Update the text of standard deviation.
    /// </summary>
    void UpdateStdDevText()
    {
        StdDevText.text = "Standard Dev: " + StdDevSlider.value;
    }

    /// <summary>
    /// Update the text of glass strength.
    /// </summary>
    void UpdateGlassStrengthText()
    {
        GlassStrengthText.text = "Glass strength: " + GlassStrengthSlider.value;
    }
    #endregion

    /// <summary>
    /// Calculate the value sampling on a normal distribution function. 
    /// </summary>
    /// <param name="x">The x-coordinate value.</param>
    /// <returns>The normal distribution value.</returns>
    private float normalDistribution(float x)
    {
        // standard deviation
        float stdDev = StdDevSlider.value;
        // mean is always 0
        float mean = 0;
        return Mathf.Exp(-Mathf.Pow((x - mean), 2.0f) / (2.0f * stdDev * stdDev));
    }

    /// <summary>
    /// Smash the glass at hit point.
    /// </summary>
    /// <param name="hit">The hit point.</param>
    public void SmashIt(RaycastHit hit)
    {
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Piece"))
            {
                // rigidbody of the glass piece
                Rigidbody rigidbody = child.GetComponent<Rigidbody>();

                if (rigidbody != null && rigidbody.constraints != RigidbodyConstraints.None)
                {
                    Renderer renderer = child.GetComponent<Renderer>();

                    Vector3 force = transform.forward.normalized * ThrustSlider.value;

                    // the hit point in outside the piece
                    if (!renderer.bounds.Contains(hit.point))
                    {
                        float dist = Vector3.Distance(child.GetComponent<Renderer>().bounds.center, hit.point);
                        // calculate the indirect force value by normal distribution
                        force *= normalDistribution(dist);
                    }

                    // the force is strong enough to push the glass
                    if (force.z >= GlassStrengthSlider.value)
                    {
                        // unfreeze the piece
                        rigidbody.constraints = RigidbodyConstraints.None;
                        rigidbody.useGravity = true;
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
