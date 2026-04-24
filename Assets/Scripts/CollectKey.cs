using UnityEngine;

public class CollectKey : MonoBehaviour
{
    [SerializeField] AudioSource coinFx;
    
    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerCollector(other))
        {
            return;
        }

        if (coinFx != null && coinFx.clip != null)
        {
            AudioSource.PlayClipAtPoint(coinFx.clip, transform.position, coinFx.volume);
        }

        RunManager.Instance.AddKey();
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
