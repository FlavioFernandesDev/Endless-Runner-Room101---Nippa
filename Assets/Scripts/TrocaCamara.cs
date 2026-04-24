using UnityEngine;

public class TrocaCamara : MonoBehaviour
{
    public GameObject cameraLonge; // AnimCam
    public GameObject cameraPerto; // StaticCam

    void OnEnable()
    {
        ShowStatic();
    }

    public void PlayIntro()
    {
        if (cameraPerto != null) cameraPerto.SetActive(false);
        if (cameraLonge != null)
        {
            cameraLonge.SetActive(true);

            Animator cameraAnimator = cameraLonge.GetComponent<Animator>();
            if (cameraAnimator != null)
            {
                cameraAnimator.Play("AnimMenuCam", 0, 0f);
                cameraAnimator.Update(0f);
            }
        }
    }

    public void ShowStatic()
    {
        if (cameraLonge != null) cameraLonge.SetActive(false);
        if (cameraPerto != null) cameraPerto.SetActive(true);
    }
}
