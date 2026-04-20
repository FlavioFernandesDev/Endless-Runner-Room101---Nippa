using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class MainMenuControl : MonoBehaviour
{
    [SerializeField] GameObject fadeOut;
    [SerializeField] GameObject bounceText;
    [SerializeField] GameObject bigButton;
    [SerializeField] GameObject animCam;
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject menuControls;
    [SerializeField] AudioSource buttonSelect;
    public static bool hasClicked;
    [SerializeField] GameObject staticCam;
    [SerializeField] GameObject fadeIn;

    [SerializeField] int loadedCoins;
    [SerializeField] int loadedKeys;
    [SerializeField] int loadedDistance;
    [SerializeField] GameObject coinDisplay;
    [SerializeField] GameObject keyDisplay;
    [SerializeField] GameObject distanceDisplay;
    

    void Start()
    {
        StartCoroutine(FadeInTurnOff());
        if(hasClicked == true)
        {
            staticCam.SetActive(true);
            animCam.SetActive(false);
            menuControls.SetActive(true);
            bounceText.SetActive(false);
            bigButton.SetActive(false);
        }
    }

    public void MenuBeginButton()
    {
        StartCoroutine(AnimCam());
    }

    public void StartGame()
    {
        StartCoroutine(StartButton());
    }

    IEnumerator StartButton()
    {
        buttonSelect.Play();
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(2);
    }

    IEnumerator AnimCam()
    {
        animCam.GetComponent<Animator>().Play("AnimMenuCam");
        bounceText.SetActive(false);
        bigButton.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        fadeIn.SetActive(false);
        mainCam.SetActive(true);
        animCam.SetActive(false);
        menuControls.SetActive(true);
        hasClicked = true;
    }

    IEnumerator FadeInTurnOff()
    {
        yield return new WaitForSeconds(0.05f);
        loadedCoins = PlayerPrefs.GetInt("COINSAVE");
        loadedKeys = PlayerPrefs.GetInt("KEYSAVE");
        loadedDistance = PlayerPrefs.GetInt("DISTANCESAVE");
        coinDisplay.GetComponent<TMPro.TMP_Text>().text = "" + MasterInfo.coinCount;
        keyDisplay.GetComponent<TMPro.TMP_Text>().text = "" + MasterInfo.keyCount;
        distanceDisplay.GetComponent<TMPro.TMP_Text>().text = "" + MasterInfo.distanceRun;
        yield return new WaitForSeconds(1);
        fadeIn.SetActive(false);
    }

    
    public void SairParaSelectStage()
    {
        // Garante que o tempo está normal 
        Time.timeScale = 1f;
        // Carrega a cena de seleção de níveis
        SceneManager.LoadScene("StageSelect");
    }
}