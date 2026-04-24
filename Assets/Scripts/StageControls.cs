using UnityEngine;
using UnityEngine.SceneManagement;

public class StageControls : MonoBehaviour
{
    public void PressPlay()
    {
        PlayRoomRun();
    }

    public void PlayRoomRun()
    {
        SelectAndPlay(RunManager.GameplaySceneName);
    }

    public void PlayHauntedHotel()
    {
        SelectAndPlay(RunManager.HauntedGameplaySceneName);
    }

    public void SelectAndPlay(string gameplaySceneName)
    {
        RunManager.Instance.SelectGameplayScene(gameplaySceneName);
        SceneManager.LoadScene(RunManager.InformationSceneName);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(RunManager.MainMenuSceneName);
    }
}
