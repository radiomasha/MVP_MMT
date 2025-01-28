using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera camera;
    private RectTransform rectTransform;
    private Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
    }

    private void TurnToCamera()
    {
        Vector3 cameraDir = camera.transform.position - transform.position;
        float dotProduct = Vector3.Dot(transform.forward, cameraDir.normalized);
        if (dotProduct  <0)
        {
            rectTransform.localRotation = Quaternion.Euler(0,0,0);
        }
        else
        {
            rectTransform.localRotation = Quaternion.Euler(0,180,0);
        }
    }
}
// Update is called once per frame}
  
