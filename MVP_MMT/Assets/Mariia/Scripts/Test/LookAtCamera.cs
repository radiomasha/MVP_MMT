using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Vector3 normal = mainCamera.transform.forward.normalized; 
            transform.rotation = quaternion.identity;
        }
        

    }
}
  
