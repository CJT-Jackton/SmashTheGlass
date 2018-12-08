using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Glass : MonoBehaviour
{
    public Material glassMaterial;
    public Scrollbar timeline;

    private GameObject glass;
    private List<GameObject> pieces;
    private Rigidbody rigidbody;
    private GeneratePieces gp;
    private RandomPoint randomPoint;
    private VoronoiCell voronoi;
    private Smash smash;

    private bool _broken;

    // Use this for initialization
    void Start()
    {
        pieces = new List<GameObject>();

        gp = GetComponent<GeneratePieces>();
        randomPoint = GetComponent<RandomPoint>();
        voronoi = GetComponent<VoronoiCell>();
        smash = GetComponent<Smash>();

        _broken = true;

        ResetGlass();
    }

    // Update is called once per frame
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
                    if (!_broken)
                    {
                        _broken = true;
                        Destroy(glass);

                        Vector3 center = gameObject.transform.InverseTransformPoint(hit.point);
                        Vector2[] random = randomPoint.getRandomPoint(new Vector2(center.x, center.y));
                        Vector2[] test = new Vector2[30];

                        //test[0] = new Vector2(-0.538852060144569f, -0.393761943857741f);
                        //test[1] = new Vector2(-0.298433613285351f, 0.596948397781687f);
                        //test[2] = new Vector2(0.609526939477559f, 0.271821487752154f);
                        //test[3] = new Vector2(0.0472157853175322f, -0.665718169174856f);
                        //test[4] = new Vector2(-0.636301019554994f, -0.201323182137448f);
                        //test[5] = new Vector2(-0.0262855956431134f, 0.666872610484462f);
                        //test[6] = new Vector2(0.660706567821757f, -0.0942169962897742f);
                        //test[7] = new Vector2(-0.229822899995986f, -0.626571181742433f);
                        //test[8] = new Vector2(-0.604343329501412f, 0.283159233011782f);
                        //test[9] = new Vector2(0.23099975814383f, 0.626138261800192f);
                        //test[10] = new Vector2(-0.311882751510742f, -0.232998107871212f);
                        //test[11] = new Vector2(-0.178400117941457f, 0.346023650752301f);
                        //test[12] = new Vector2(0.361812246014974f, 0.143704097349643f);
                        //test[13] = new Vector2(0.0306095207505624f, -0.388100536202795f);
                        //test[14] = new Vector2(-0.370368991788815f, -0.119940730708181f);
                        //test[15] = new Vector2(-0.0235630831404277f, 0.388592009792267f);
                        //test[16] = new Vector2(0.387591409965953f, -0.0364947651325542f);
                        //test[17] = new Vector2(-0.127728549443157f, -0.36775587910817f);
                        //test[18] = new Vector2(-0.355483291734862f, 0.158715463200101f);
                        //test[19] = new Vector2(0.144114763887596f, 0.361648868090443f);
                        //test[20] = new Vector2(-0.131302106562233f, -0.102407306534168f);
                        //test[21] = new Vector2(-0.0770595755647677f, 0.147612063982051f);
                        //test[22] = new Vector2(0.15576812075878f, 0.0588539903026245f);
                        //test[23] = new Vector2(0.0149611508624001f, -0.165842285271687f);
                        //test[24] = new Vector2(-0.158514693653205f, -0.0509959950907295f);
                        //test[25] = new Vector2(-0.00953393248128895f, 0.166242605100815f);
                        //test[26] = new Vector2(0.165901894756666f, -0.0142849898638846f);
                        //test[27] = new Vector2(-0.0536191761237597f, -0.157646704916635f);
                        //test[28] = new Vector2(-0.151401277127068f, 0.0693192102057999f);
                        //test[29] = new Vector2(0.0614254439884864f, 0.154772137189097f);

                        for (int i = 0; i < 30; i++)
                        {
                            test[i] = random[i + 10];
                        }
                        //test[16].x -= 0.1f;

                        Vector2[][] pieces = voronoi.GenerateVoronoi(random);

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

                        smash.SmashIt(hit);
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

        // Set up the transformation
        p.transform.parent = gameObject.transform;
        p.transform.localPosition = new Vector3(0, 0, 0);
        p.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        // Assign material
        p.GetComponent<Renderer>().material = glassMaterial;

        // Recording transform
        p.AddComponent<Rewind>();
        p.GetComponent<Rewind>().timeline = timeline;
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
