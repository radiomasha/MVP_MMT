using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class ConnectNodes : MonoBehaviour
{
    [SerializeField] private Material lineMaterial;
    private HandGrabPose handGrabPose;
    private Vector3 startPosition;
    private LineRenderer currentLineRenderer;
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void CreateNewLine(Vector3 startPosition)
    {
        GameObject newLine = new GameObject("LineRenderer");
        currentLineRenderer= newLine.AddComponent<LineRenderer>();
        newLine.transform.SetParent(transform);
        currentLineRenderer.positionCount = 2;
        currentLineRenderer.startWidth = 0.01f;
        currentLineRenderer.endWidth = 0.01f;
        currentLineRenderer.material=lineMaterial?? new Material(Shader.Find("Unlit/Color"));
        currentLineRenderer.material.color = Color.green;
        currentLineRenderer.SetPosition(0, startPosition);
        currentLineRenderer.SetPosition(1, startPosition);
        lineRenderers.Add(currentLineRenderer);
    }

    private void UpdateLine(Vector3 currentPosition)
    {
        if (currentLineRenderer != null)
        {
            currentLineRenderer.SetPosition(1, currentPosition);
            //_prefabTransform.position = _currentLineRenderer.GetPosition(1);
           
        }
    }
}
