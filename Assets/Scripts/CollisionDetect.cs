using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionDetect : MonoBehaviour
{
    [SerializeField] GameObject thePlayer;
    [SerializeField] GameObject playerAnim;
    [SerializeField] AudioSource collisionFx;
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject fadeOut;


    void OnTriggerEnter(Collider other)
    {
        SaveLoad.saveData = true;
        StartCoroutine(CollisionEnd());
    }

    IEnumerator CollisionEnd()
    {
        collisionFx.Play();
        thePlayer.GetComponent<PlayerMovement>().enabled = false;
        playerAnim.GetComponent<Animator>().Play("Stumble Backwards");
        mainCam.GetComponent<Animator>().Play("CollisionCam");
        yield return new WaitForSeconds(2);
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }


}
