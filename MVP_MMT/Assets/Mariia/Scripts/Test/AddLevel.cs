using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction.Grab;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Management;

public class AddLevel : MonoBehaviour
{
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private float levelDistance = 0.3f;
    [SerializeField] private float handDistance = 0.15f;
    [SerializeField] private Material planeMaterial;
    private XRHandSubsystem subsystem;
    private XRHand righthand;
    private Vector3 previousHandPosition;
    private Transform existingNode;
    private bool canAddLevel;
    private bool canCloseLevel;
    private bool nodeCreated;
    private List<GameObject> nodes = new List<GameObject>();
    private List<GameObject> planes = new List<GameObject>();
    //private List<int> nodeIDs = new List<int>();



    private void Start()
    {
        subsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
        if (subsystem == null)
        {
            UIDebugger.Log("XR Hand Subsystem Not Found");
        }

        previousHandPosition = righthand.rootPose.position;
    }

    public void SetAddlevel(bool add)
    {
        canAddLevel = add;
    }

    public void SetCloseLevel(bool close)
    {
        canCloseLevel = close;
    }

    private void Update()
    {
        if (subsystem == null) return;
        righthand = subsystem.rightHand;

        if (righthand.isTracked)
        {
            if (canAddLevel)
            {
                GetPosition(righthand, out Vector3 position);
                ActivateNode(position);
            }

            if (canCloseLevel)
            {
                UIDebugger.Log("Close Level");
                DeactivateLevel();
            }

        }
    }

    private void GetPosition(XRHand hand, out Vector3 position)
    {
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        Vector3 indexPos = Vector3.zero;
        Vector3 thumbPos = Vector3.zero;
        if (indexTip.TryGetPose(out Pose indexTipPose))
        {
            indexPos = indexTipPose.position;
        }

        if (thumbTip.TryGetPose(out Pose thumbTipPose))
        {
            thumbPos = thumbTipPose.position;
        }

        position = (indexPos + thumbPos) / 2;
    }

    // Update is called once per frame
    private void CreateLevel()
    {
        Vector3 pos = existingNode.transform.position - Vector3.forward.normalized * levelDistance;
        var node = Instantiate(nodePrefab, pos, existingNode.rotation);
        node.GetComponent<NodeLevel>().currentLevel = existingNode.GetComponent<NodeLevel>().currentLevel + 1;
        node.GetComponent<NodeLevel>().nodeIndex =existingNode.GetComponent<NodeLevel>().nodeIndex;
        //UIDebugger.Log(node.GetComponent<NodeLevel>().currentLevel.ToString());
        var plane = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Quad), node.transform.position,
            Quaternion.identity);
        plane.GetComponent<Renderer>().material = planeMaterial;
        plane.transform.SetParent(node.transform);
        plane.SetActive(false);
        nodes.Add(node);
        planes.Add(plane);
    }

    private void ActivateNode(Vector3 position)
    {
        Vector3 currentHandPosition = righthand.rootPose.position;
        if (Physics.Raycast(position, Vector3.forward, out RaycastHit hit, handDistance))
        {
            if (hit.collider.CompareTag("Node") && Vector3.Distance(currentHandPosition, previousHandPosition) >= 0.05f)
            {
                existingNode = hit.transform;
                //UIDebugger.Log("Node Found");
                if (!existingNode.GetComponent<HasNode>() &&
                    existingNode.GetComponent<NodeLevel>().currentLevel <= Level.thirdLevel)
                {
                    existingNode.AddComponent<HasNode>();
                    CreateLevel();
                }
                else
                {
                    var levelToActivate = existingNode.GetComponent<NodeLevel>().currentLevel + 1;
                    var nodeToact = nodes[existingNode.GetComponent<NodeLevel>().nodeIndex];
                    nodeToact.GetComponent<NodeLevel>().currentLevel = levelToActivate;
                    nodeToact.SetActive(true);
                    //UIDebugger.Log(nodeToact.GetComponent<NodeLevel>().currentLevel.ToString());
                }
            }
        }
    }

    private void DeactivateLevel()
    {
        var middle = righthand.GetJoint(XRHandJointID.MiddleIntermediate);
        if (middle.TryGetPose(out Pose middlePose))
        {
            var position = middlePose.position;
            if (Physics.Raycast(position, Vector3.forward, out RaycastHit hit, handDistance))
            {
                if (hit.collider.CompareTag("Node") &&
                    Vector3.Distance(position, hit.collider.transform.position) <= 0.03f)
                {
                    hit.collider.gameObject.SetActive(false);
                    UIDebugger.Log("Deactivated Node");
                }
            }
        }
        //Vector3 currentHandPosition = righthand.rootPose.position;
        
    }
}
