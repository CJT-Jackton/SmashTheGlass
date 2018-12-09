using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    private GameObject effect;

    void Start()
    {
        effect = transform.GetChild(0).gameObject;
        effect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null)
            {
                transform.position = hit.point;
                //transform.Translate(new Vector3(.0f, .0f, -0.1f));
                effect.SetActive(true);

                return;
            }
        }

            effect.SetActive(false);
    }
}
