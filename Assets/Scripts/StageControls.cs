using UnityEngine;
using UnityEngine.SceneManagement;

public class StageControls : MonoBehaviour
{
    public void PressPlay()
    {
        SceneManager.LoadScene(RunManager.InformationSceneName);
    }
}
