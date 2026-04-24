using UnityEngine;

public class CollectCoin : MonoBehaviour
{
    [SerializeField] private AudioSource coinFx;
    [SerializeField] private AudioClip collectClip;
    [SerializeField] [Range(0f, 1f)] private float collectVolume = 1f;
    
    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerCollector(other))
        {
            return;
        }

        AudioClip clipToPlay = coinFx != null && coinFx.clip != null ? coinFx.clip : collectClip;
        float volumeToPlay = coinFx != null && coinFx.clip != null ? coinFx.volume : collectVolume;
        if (clipToPlay != null)
        {
            AudioSource.PlayClipAtPoint(clipToPlay, transform.position, volumeToPlay);
        }

        RunManager.Instance.AddCoin();
        gameObject.SetActive(false);
    }

    private static bool IsPlayerCollector(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.CompareTag("Player"))
        {
            return true;
        }

        if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<PlayerMovement>() != null)
        {
            return true;
        }

        return other.GetComponentInParent<PlayerMovement>() != null;
    }
}
