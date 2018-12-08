using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rewind : MonoBehaviour
{

    public class TransformInTime
    {
        public TransformInTime(Vector3 pos, Quaternion rotate)
        {
            position = pos;
            rotation = rotate;
        }

        public Vector3 position;
        public Quaternion rotation;
    }

    bool isRecording = false;
    float recordTime = 3f;

    List<TransformInTime> transformInTime;
    Rigidbody rigidbody;

    public Scrollbar timeline;

    // Use this for initialization
    void Start()
    {
        isRecording = true;
        timeline.enabled = false;
        transformInTime = new List<TransformInTime>();
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isRecording)
        {
            if(transformInTime.Count >= recordTime / Time.fixedDeltaTime)
            {
                StopRecord();
            }
            else
            {
                float time = transformInTime.Count / (1 + recordTime / Time.fixedDeltaTime);
                timeline.value = time;

                transformInTime.Add(new TransformInTime(transform.position, transform.rotation));
            }
        }
    }

    public void StartRecord()
    {
        isRecording = true;
    }

    void StopRecord()
    {
        isRecording = false;
        timeline.value = 1f;
        timeline.enabled = true;
        rigidbody.isKinematic = true;

        Debug.Log("Stop record " + name);
    }

    void Update()
    {
        if (!isRecording)
        {
            float time = timeline.value;
            int frame = (int)(time * (transformInTime.Count - 1));
            transform.position = transformInTime[frame].position;
            transform.rotation = transformInTime[frame].rotation;
        }
    }
}
