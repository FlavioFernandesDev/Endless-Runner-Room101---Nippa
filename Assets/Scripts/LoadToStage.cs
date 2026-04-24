using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadToStage : MonoBehaviour
{
    [SerializeField] GameObject fadeOut;

    void Start()
    {
        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel()
    {
        yield return new WaitForSeconds(3);
        if (fadeOut != null)
        {
            fadeOut.SetActive(true);
        }

        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(RunManager.Instance.SelectedGameplaySceneName);
    }
}
