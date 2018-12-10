using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPoint : MonoBehaviour {

    public GameObject point;
    public Toggle toggle;

    // list of sites
    List<GameObject> points = new List<GameObject>();

    /// <summary>
    /// Setup the UI event.
    /// </summary>
    void Start()
    {
        toggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(toggle);
        });
    }

    /// <summary>
    /// Create sites from given positions.
    /// </summary>
    /// <param name="position">Rekative position of the sites.</param>
    /// <param name="center">The center of the sites.</param>
    public void CreatePoints(Vector2[] position, Vector2 center)
    {
        Destroy();

        foreach (Vector2 p in position)
        {
            Vector3 pos = new Vector3(p.x, p.y, 0);
            GameObject newPoint = Instantiate(point, pos, new Quaternion(0, 0, 0, 0));

            newPoint.name = "Site " + (points.Count + 1).ToString("D2"); ;

            newPoint.transform.parent = gameObject.transform;
            newPoint.transform.localPosition = pos;

            newPoint.GetComponent<ParticleSystem>().startColor = Color.HSVToRGB(Vector3.Distance(pos, center), 1, 1);

            points.Add(newPoint);
        }

        ToggleValueChanged(toggle);
    }

    /// <summary>
    /// Show or hide the points when toggle value changed.
    /// </summary>
    /// <param name="toggle">The UI toggle.</param>
    void ToggleValueChanged(Toggle toggle)
    {
        if (toggle.isOn)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    /// <summary>
    /// Show the points.
    /// </summary>
    public void Show()
    {
        if (points.Count > 0)
        {
            foreach (GameObject p in points)
            {
                p.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Hide the points.
    /// </summary>
    public void Hide()
    {
        if (points.Count > 0)
        {
            foreach (GameObject p in points)
            {
                p.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Clear all points in the list.
    /// </summary>
    public void Destroy()
    {
        if (points.Count > 0)
        {
            foreach (GameObject p in points)
            {
                Destroy(p);
            }

            points.Clear();
        }
    }
}
