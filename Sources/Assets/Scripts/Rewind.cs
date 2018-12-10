using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rewind : MonoBehaviour
{
    public Scrollbar timeline;
    public float recordTime = 3f;

    /// <summary>
    /// A simple class store transformation of a game oject in a time frame.
    /// </summary>
    class TransformInTime
    {
        public TransformInTime(Vector3 pos, Quaternion rotate)
        {
            position = pos;
            rotation = rotate;
        }

        public Vector3 position;
        public Quaternion rotation;
    }

    // whether recording the transform
    bool isRecording = false;

    // the list of transformations over time
    List<TransformInTime> transformInTime;

    // rigidbody of the game object
    Rigidbody rigidbody;
    
    /// <summary>
    /// Initialize the variables.
    /// </summary>
    void Start()
    {
        isRecording = true;
        timeline.enabled = false;
        transformInTime = new List<TransformInTime>();
        rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Recording the transformation in each time frame.
    /// </summary>
    void FixedUpdate()
    {
        if (isRecording)
        {
            if(transformInTime.Count >= recordTime / Time.fixedDeltaTime)
            {
                // stop recording if exceeded record time
                EndRecord();
            }
            else
            {
                float time = transformInTime.Count / (1 + recordTime / Time.fixedDeltaTime);
                timeline.value = time;

                // record the transformation
                transformInTime.Add(new TransformInTime(transform.position, transform.rotation));
            }
        }
    }

    /// <summary>
    /// Start recording.
    /// </summary>
    public void StartRecord()
    {
        isRecording = true;
    }

    /// <summary>
    /// Stop recording.
    /// </summary>
    void EndRecord()
    {
        isRecording = false;
        timeline.value = 1f;
        timeline.enabled = true;
        rigidbody.isKinematic = true;
    }

    /// <summary>
    /// Rewind the frame.
    /// </summary>
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
