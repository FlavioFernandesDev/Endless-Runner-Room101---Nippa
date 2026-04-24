using System.Collections.Generic;
using UnityEngine;

public sealed class RuntimePrefabPool : MonoBehaviour
{
    private readonly Dictionary<GameObject, Queue<GameObject>> _poolByPrefab = new Dictionary<GameObject, Queue<GameObject>>();

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null)
        {
            return null;
        }

        if (_poolByPrefab.TryGetValue(prefab, out Queue<GameObject> queue) && queue.Count > 0)
        {
            GameObject instance = queue.Dequeue();
            instance.transform.SetParent(parent, false);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        GameObject created = Instantiate(prefab, position, rotation, parent);
        created.AddComponent<RuntimePooledInstance>().Initialize(prefab);
        return created;
    }

    public void ReleaseChildren(Transform container)
    {
        if (container == null)
        {
            return;
        }

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject child = container.GetChild(i).gameObject;
            Release(child);
        }
    }

    public void Release(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        RuntimePooledInstance pooledInstance = instance.GetComponent<RuntimePooledInstance>();
        if (pooledInstance == null || pooledInstance.SourcePrefab == null)
        {
            Destroy(instance);
            return;
        }

        if (!_poolByPrefab.TryGetValue(pooledInstance.SourcePrefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _poolByPrefab.Add(pooledInstance.SourcePrefab, queue);
        }

        instance.SetActive(false);
        instance.transform.SetParent(transform, false);
        queue.Enqueue(instance);
    }

    public static RuntimePrefabPool GetOrCreate(GameObject owner)
    {
        if (owner == null)
        {
            return null;
        }

        RuntimePrefabPool pool = owner.GetComponent<RuntimePrefabPool>();
        return pool != null ? pool : owner.AddComponent<RuntimePrefabPool>();
    }
}
