using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Glass : MonoBehaviour
{
    public Material glassMaterial;
    public Scrollbar timeline;
    public Text timer;
    public Button resetButton;
    public GameObject prompt;
    public float recordTime;

    private GameObject glass;
    private List<GameObject> pieces;
    private GeneratePieces gp;
    private RandomPoint randomPoint;
    private ShowPoint showPoint;
    private VoronoiCell voronoi;
    private Smash smash;

    private bool _broken;

    // Use this for initialization
    void Start()
    {
        pieces = new List<GameObject>();

        gp = GetComponent<GeneratePieces>();
        showPoint = GetComponent<ShowPoint>();
        smash = GetComponent<Smash>();

        randomPoint = new RandomPoint();
        voronoi = new VoronoiCell();

        resetButton.onClick.AddListener(ResetGame);

        prompt.active = true;

        _broken = true;

        ResetGlass();
    }

    // Update is called once per frame
    void Update()
    {
        timer.text = (recordTime * timeline.value).ToString("F4") + "s";

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    if (!_broken)
                    {
                        _broken = true;
                        Destroy(glass);

                        Vector3 center = gameObject.transform.InverseTransformPoint(hit.point);
                        Vector2[] random = randomPoint.getRandomPoint(new Vector2(center.x, center.y));
                        Vector2[] test = new Vector2[random.Length];

                        for (int i = 0; i < random.Length - 10; i++)
                        {
                            test[i] = random[i + 10];
                        }
                        //test[16].x -= 0.1f;

                        showPoint.CreatePoints(random, new Vector2(center.x, center.y));
                        Vector2[][] pieces = voronoi.GenerateVoronoi(random, hit.point);

                        foreach (Vector2[] piece in pieces)
                        {
                            Vector3[] vertices = new Vector3[piece.Length];

                            for (uint i = 0; i < piece.Length; ++i)
                            {
                                vertices[i] = new Vector3(piece[i].x, 0, piece[i].y);
                            }

                            if (vertices.Length < 3)
                            {
                                foreach (Vector3 v in vertices)
                                {
                                    Debug.Log(v);
                                }
                            }
                            else
                            {
                                AddPiece(vertices);
                            }
                        }

                        prompt.active = false;
                        smash.SmashIt(hit);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }

    void ResetGame()
    {
        if (_broken)
        {
            ResetGlass();
            Destroy(voronoi);
            showPoint.Destroy();

            randomPoint = new RandomPoint();
            voronoi = new VoronoiCell();

            timeline.value = 0f;
            prompt.active = true;
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

        // Recording transform
        p.AddComponent<Rewind>();
        p.GetComponent<Rewind>().timeline = timeline;
        p.GetComponent<Rewind>().recordTime = recordTime;
        p.GetComponent<Rewind>().StartRecord();

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

            Rigidbody rigidbody = glass.AddComponent(typeof(Rigidbody)) as Rigidbody;
            rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;

            // remove the box collider of glass
            Destroy(glass.GetComponent<BoxCollider>());
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
