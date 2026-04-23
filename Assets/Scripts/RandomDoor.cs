using System.Collections;
using UnityEngine;

public class RandomDoor : MonoBehaviour
{
    [Header("Configurações de Probabilidade")]
    [Range(0, 100)]
    public float chanceToOpen = 40f;
    
    [Header("Configurações de Animação")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float distanceToTrigger = 14f;
    public float minimumForwardOffset = -2f;

    [Header("Referências (Opcional)")]
    public AudioSource audioSource;
    public AudioSource protectedHitAudioSource;
    public Transform player;

    private bool jaTentouAbrir = false;
    private Quaternion closedRot;
    private Quaternion openRot;
    private Coroutine _animationCoroutine;
    private bool _isOpen;
    private bool _isAnimating;

    public bool IsOpen => _isOpen;
    public bool IsAnimating => _isAnimating;

    void Start()
    {
        closedRot = transform.localRotation;
        openRot = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y + openAngle, transform.localEulerAngles.z);

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (player == null || RunManager.Instance.IsGameOver)
        {
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (!jaTentouAbrir && dist < distanceToTrigger)
        {
            jaTentouAbrir = true;
            TentarAbrirPorta();
        }
    }

    void TentarAbrirPorta()
    {
        float sorteio = Random.Range(0f, 100f);

        if (sorteio < chanceToOpen)
        {
            StartDoorAnimation(openRot, true);

            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }

    public bool TryConsumeDoorHit()
    {
        if (!_isOpen)
        {
            return false;
        }

        StartDoorAnimation(closedRot, false);

        AudioSource feedbackSource = protectedHitAudioSource != null ? protectedHitAudioSource : audioSource;
        if (feedbackSource != null)
        {
            feedbackSource.Play();
        }

        return true;
    }

    private void StartDoorAnimation(Quaternion targetRotation, bool opening)
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }

        if (opening)
        {
            _isOpen = true;
        }

        _animationCoroutine = StartCoroutine(AnimateDoor(targetRotation, opening));
    }

    private IEnumerator AnimateDoor(Quaternion targetRotation, bool opening)
    {
        _isAnimating = true;
        if (!opening)
        {
            _isOpen = false;
        }

        float t = 0;
        Quaternion startRotation = transform.localRotation;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.localRotation = targetRotation;
        _isAnimating = false;
        _isOpen = opening;
        _animationCoroutine = null;
    }
}
