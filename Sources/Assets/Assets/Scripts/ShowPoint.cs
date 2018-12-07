using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPoint : MonoBehaviour {

    public GameObject point;

    List<GameObject> points = new List<GameObject>();

    public void CreatePoints(Vector2[] position)
    {
        Destroy();

        foreach (Vector2 p in position)
        {
            Vector3 pos = new Vector3(p.x, p.y, 0);
            GameObject newPoint = Instantiate(point, pos, new Quaternion(0, 0, 0, 0));

            newPoint.name = "Site " + (points.Count + 1).ToString("D2"); ;

            newPoint.transform.parent = gameObject.transform;
            newPoint.transform.localPosition = pos;

            newPoint.GetComponent<ParticleSystem>().startColor = Color.HSVToRGB(Vector3.Distance(pos, new Vector2(0, 0)), 1, 1);

            points.Add(newPoint);
        }
    }

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
