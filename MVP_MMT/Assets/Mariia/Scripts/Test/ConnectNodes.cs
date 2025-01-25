using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using Color = UnityEngine.Color;

public class ConnectNodes : MonoBehaviour
{
    [SerializeField] private Material lineMaterial;
    private XRHandSubsystem _subsystem;
    private bool _isPointing = false;
    private bool canDraw;
    private Vector3 _startPosition;
    private LineRenderer currentLineRenderer;
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private bool isPaused = false;
    private bool _firstNode = false;

    private List<Transform> startNodeTransforms = new List<Transform>();
    private List<Transform> endNodeTransforms= new List<Transform>();

    void Start()
    {
        _subsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
        if (_subsystem == null)
        {
            UIDebugger.Log("XR Hand Subsystem Not Found");
        }
    }

    public void SetCanDraw(bool draw)
    {
       canDraw = draw;
    }

    void Update()
    {
        if (_subsystem == null) return;
        XRHand righthand = _subsystem.rightHand;
        if (righthand.isTracked && canDraw)
        {
            bool isPointing = IsPointing(righthand, out Vector3 pointPosition, out Quaternion indexRotation);
            
            if (isPointing && !isPaused)
            {
                if (!_isPointing)
                {
                    // First node selection
                    if (IsPointingNode(pointPosition, indexRotation, out Vector3 nodePosition, out Transform startNode)&&!_firstNode)
                    {
                        CreateNewLine(nodePosition, startNode);
                        UIDebugger.Log("start");
                    }
                }
                else
                {
                    if (IsPointingNode(pointPosition, indexRotation, out Vector3 finalNodePosition, out Transform endNode) && 
                        Vector3.Distance(finalNodePosition, _startPosition)>=0.2f&&_firstNode)
                    {
                        FinalizeLine(finalNodePosition, endNode);
                        _firstNode = false;
                        UIDebugger.Log("fin");
                    }
                    else if(_firstNode)
                    {
                        UpdateLine(pointPosition);
                    }
                }
            }
        }

        UpdateRenderers();
    }

    private void CreateNewLine(Vector3 startPosition, Transform startNode)
    {
        GameObject newLine = new GameObject("LineRenderer");
        LineRenderer lineRenderer = newLine.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = lineMaterial ?? new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.green;

        _startPosition = startPosition;
        startNodeTransforms.Add(startNode);
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition);
        
        currentLineRenderer = lineRenderer;
        lineRenderers.Add(currentLineRenderer);
        _isPointing = true;
        _firstNode = true;
    }

    private void UpdateLine(Vector3 currentPosition)
    {
        if (currentLineRenderer != null)
        {
            currentLineRenderer.SetPosition(1, currentPosition);
        }
    }

    private void FinalizeLine(Vector3 endPosition, Transform endNode)
    {
        if (currentLineRenderer != null)
        {
            currentLineRenderer.SetPosition(1, endPosition);
            endNodeTransforms.Add(endNode);
            currentLineRenderer = null;
            _isPointing = false;
            isPaused = true;
            
            StartCoroutine(PauseBeforeNewLine());
        }
    }

    private bool IsPointing(XRHand hand, out Vector3 pointPosition, out Quaternion indexRotation)
    {
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        
        if (indexTip.TryGetPose(out Pose indexTipPose))
        {
            pointPosition = indexTipPose.position;
            indexRotation = indexTipPose.rotation;
            return true;
        }
        
        pointPosition = Vector3.zero;
        indexRotation = Quaternion.identity;
        return false; 
    }
    
    private bool IsPointingNode(Vector3 pointPosition, Quaternion indexRotation, out Vector3 nodePosition, out Transform node)
    {
        Ray ray = new Ray(pointPosition, indexRotation * Vector3.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.05f))
        {
            if (hit.collider.CompareTag("Node"))
            {
                
                node = hit.transform;
                nodePosition = hit.collider.transform.position;
                return true; 
            }
        }
        node = null;
        nodePosition = Vector3.zero;
        return false;
    }

    private IEnumerator PauseBeforeNewLine()
    {
        yield return new WaitForSeconds(2f);  
        isPaused = false;  
    }

    private void UpdateRenderers()
    {
        for(int i = 0; i < lineRenderers.Count; i++)
        {
            if (lineRenderers[i] != null && lineRenderers[i].positionCount > 1)
            {
                if (startNodeTransforms[i] != null)
                {
                    lineRenderers[i].SetPosition(0, startNodeTransforms[i].position);
                }
                if (endNodeTransforms != null)
                {
                    lineRenderers[i].SetPosition(1, endNodeTransforms[i].position);
                }
            }
        }
    } 
}