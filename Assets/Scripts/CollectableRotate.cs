using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class CollectableRotate : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1;

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime * 60f, 0f, Space.World);
    }
}
