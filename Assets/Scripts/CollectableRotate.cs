using System.Collections.Generic;
using UnityEngine;

public class CollectableRotate : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1;

    private void OnEnable()
    {
        CollectableRotationDriver.Register(this);
    }

    private void OnDisable()
    {
        CollectableRotationDriver.Unregister(this);
    }

    internal void ApplyRotation(float deltaTime)
    {
        transform.Rotate(0f, rotationSpeed * deltaTime * 60f, 0f, Space.World);
    }
}

internal sealed class CollectableRotationDriver : MonoBehaviour
{
    private static CollectableRotationDriver _instance;

    private readonly List<CollectableRotate> _rotators = new List<CollectableRotate>();

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = _rotators.Count - 1; i >= 0; i--)
        {
            CollectableRotate rotator = _rotators[i];
            if (rotator == null || !rotator.isActiveAndEnabled)
            {
                _rotators.RemoveAt(i);
                continue;
            }

            rotator.ApplyRotation(deltaTime);
        }
    }

    internal static void Register(CollectableRotate rotator)
    {
        if (rotator == null)
        {
            return;
        }

        EnsureInstance();
        if (!_instance._rotators.Contains(rotator))
        {
            _instance._rotators.Add(rotator);
        }
    }

    internal static void Unregister(CollectableRotate rotator)
    {
        if (_instance == null || rotator == null)
        {
            return;
        }

        _instance._rotators.Remove(rotator);
    }

    private static void EnsureInstance()
    {
        if (_instance != null)
        {
            return;
        }

        GameObject runnerObject = new GameObject("CollectableRotationDriver");
        runnerObject.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(runnerObject);
        _instance = runnerObject.AddComponent<CollectableRotationDriver>();
    }
}
