using UnityEngine;

public sealed class RuntimePooledInstance : MonoBehaviour
{
    public GameObject SourcePrefab { get; private set; }

    public void Initialize(GameObject sourcePrefab)
    {
        SourcePrefab = sourcePrefab;
    }
}
