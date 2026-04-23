using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionDetect : MonoBehaviour
{
    [SerializeField] GameObject thePlayer;
    [SerializeField] GameObject playerAnim;
    [SerializeField] AudioSource collisionFx;
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject fadeOut;
    [SerializeField] string triggeringTag = "Player";
    private bool _hasTriggered;


    void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered || !other.CompareTag(triggeringTag))
        {
            return;
        }

        if (SceneManager.GetActiveScene().name != RunManager.GameplaySceneName)
        {
            return;
        }

        PlayerMovement movement = null;
        if (thePlayer != null)
        {
            movement = thePlayer.GetComponent<PlayerMovement>();
        }

        if (movement == null)
        {
            movement = other.GetComponentInParent<PlayerMovement>();
        }

        if (movement != null && movement.TryHandleProtectedDoorHit(transform))
        {
            return;
        }

        _hasTriggered = true;
        RunManager.Instance.EndRun();
        StartCoroutine(CollisionEnd(movement));
    }

    IEnumerator CollisionEnd(PlayerMovement movement)
    {
        if (collisionFx != null)
        {
            collisionFx.Play();
        }

        if (movement == null && thePlayer != null)
        {
            movement = thePlayer.GetComponent<PlayerMovement>();
        }

        if (movement != null)
        {
            movement.HandleFatalCollision(false);
        }

        if (playerAnim != null)
        {
            Animator playerAnimator = playerAnim.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.Play("Stumble Backwards");
            }
        }

        if (mainCam != null)
        {
            Animator cameraAnimator = mainCam.GetComponent<Animator>();
            if (cameraAnimator != null)
            {
                cameraAnimator.Play("CollisionCam");
            }
        }

        yield return new WaitForSeconds(2);
        if (fadeOut != null)
        {
            fadeOut.SetActive(true);
        }

        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(RunManager.StageSelectSceneName);
    }

}
