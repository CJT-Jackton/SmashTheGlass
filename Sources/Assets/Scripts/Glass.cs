using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glass : MonoBehaviour
{
    public Material glassMaterial;

    private GameObject glass;
    private List<GameObject> pieces;
    private Rigidbody rigidbody;
    private GeneratePieces gp;

    private bool _broken;

    // Use this for initialization
    void Start()
    {
        pieces = new List<GameObject>();

        gp = GetComponent<GeneratePieces>();

        _broken = true;

        ResetGlass();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (!_broken)
            {
                _broken = true;
                Destroy(glass);

                Vector3[] v = new Vector3[4];
                v[0] = new Vector3(0, 0, 0);
                v[1] = new Vector3(1, 0, 0);
                v[2] = new Vector3(1, 0, 1);
                v[3] = new Vector3(0, 0, 1);

                AddPiece(v);

                v[0] = new Vector3(0, 0, 0);
                v[1] = new Vector3(0, 0, -1);
                v[2] = new Vector3(1, 0, -1);
                v[3] = new Vector3(1, 0, 0);

                AddPiece(v);

                v[0] = new Vector3(0, 0, 0);
                v[1] = new Vector3(0, 0, 1);
                v[2] = new Vector3(-1, 0, 1);
                v[3] = new Vector3(-1, 0, 0);

                AddPiece(v);

                v[0] = new Vector3(0, 0, 0);
                v[1] = new Vector3(-1, 0, 0);
                v[2] = new Vector3(-1, 0, -1);
                v[3] = new Vector3(0, 0, -1);

                AddPiece(v);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (_broken)
            {
                ResetGlass();
            }
        }
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(glass);
    }

    void AddPiece(Vector3[] vertices)
    {
        // Create the game object
        GameObject p = gp.CreatePiece(vertices);
        p.name = "Piece " + (pieces.Count + 1).ToString("D2");

        // Set up the transformation
        p.transform.parent = gameObject.transform;
        p.transform.localPosition = new Vector3(0, 0, 0);
        p.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        // Assign material
        p.GetComponent<Renderer>().material = glassMaterial;

        pieces.Add(p);
    }

    void ResetGlass()
    {
        if (_broken)
        {
            _broken = false;

            foreach (GameObject p in pieces)
            {
                Destroy(p);
            }

            pieces.Clear();

            glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "Glass";
            glass.transform.localScale = new Vector3(2, 0.05f, 2);
            glass.transform.localRotation = Quaternion.Euler(-90, 0, 0);

            glass.transform.parent = gameObject.transform;
            glass.transform.localPosition = new Vector3(0, 0, 0);

            Renderer renderer = glass.GetComponent<Renderer>();
            renderer.material = glassMaterial;

            // GameObject reflectionProbe = CreateReflectionProbe();
            // reflectionProbe.transform.parent = glass.transform;
            // reflectionProbe.transform.localPosition = new Vector3(0, 0, 0);
            // reflectionProbe.transform.localScale = new Vector3(1, 1, 1);

            rigidbody = glass.AddComponent(typeof(Rigidbody)) as Rigidbody;
            rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;
        }
    }

    GameObject CreateReflectionProbe()
    {
        GameObject rp = new GameObject();
        rp.name = "Reflection Probe";

        ReflectionProbe reflectionProbe = rp.AddComponent<ReflectionProbe>() as ReflectionProbe;
        reflectionProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
        reflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
        reflectionProbe.boxProjection = true;
        reflectionProbe.resolution = 256;
        reflectionProbe.size = new Vector3(10, 10, 10);

        return rp;
    }

    bool IsBroken()
    {
        return _broken;
    }
}
