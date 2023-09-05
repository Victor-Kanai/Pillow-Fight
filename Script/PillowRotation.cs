using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillowRotation : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up, 300 * Time.deltaTime);
    }
}
