using UnityEngine;

public class TrocaCamara : MonoBehaviour
{
    public GameObject cameraLonge; // AnimCam
    public GameObject cameraPerto; // StaticCam

    void OnEnable()
    {
        if (cameraLonge != null) cameraLonge.SetActive(false);
        if (cameraPerto != null) cameraPerto.SetActive(true);
    }
}