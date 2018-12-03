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
    private RandomPoint randomPoint;
    private VoronoiCell voronoi;

    private bool _broken;

    // Use this for initialization
    void Start()
    {
        pieces = new List<GameObject>();

        gp = GetComponent<GeneratePieces>();
        randomPoint = GetComponent<RandomPoint>();
        voronoi = GetComponent<VoronoiCell>();

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

                Vector2[][] pieces = voronoi.GenerateVoronoi(randomPoint.getRandomPoint());

                foreach (Vector2[] piece in pieces)
                {
                    Vector3[] vertices = new Vector3[piece.Length];

                    for (uint i = 0; i < piece.Length; ++i)
                    {
                        vertices[i] = new Vector3(piece[i].x, 0, piece[i].y);
                    }

                    if(vertices.Length < 3)
                    {
                        foreach(Vector3 v in vertices)
                        {
                            Debug.Log(v);
                        }
                    }
                    else
                    {
                        AddPiece(vertices);
                    }
                }
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

        if(pieces.Count == 29)
        {
            foreach(Vector3 v in vertices)
            {
                Debug.Log(v);
            }
        }

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
