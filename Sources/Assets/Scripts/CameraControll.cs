using UnityEngine;
using UnityEngine.UI;

public class CameraControll : MonoBehaviour {
    public Camera[] cameras;
    public Button[] buttons;

    /// <summary>
    /// Setup buttons event handler.
    /// </summary>
    void Start ()
    {
        for (int i = 0; i < buttons.Length; ++i)
        {
            int k = i;
            buttons[i].onClick.AddListener(delegate { SwitchToCamera(k); });

            // only enable the first camera
            if (i != 0)
            {
                cameras[i].enabled = false;
            }
        }
    }

    /// <summary>
    /// Switch to camera #n.
    /// </summary>
    /// <param name="n">The camera index.</param>
    void SwitchToCamera(int n)
    {
        if (n >= 0 && n < cameras.Length)
        {
            // enable the camera
            for (int i = 0; i < cameras.Length; ++i)
            {
                if (i == n)
                {
                    cameras[i].enabled = true;
                }
            }

            // disable rest of the cameras
            for (int i = 0; i < cameras.Length; ++i)
            {
                if (i != n)
                {
                    cameras[i].enabled = false;
                }
            }
        }
    }
}
