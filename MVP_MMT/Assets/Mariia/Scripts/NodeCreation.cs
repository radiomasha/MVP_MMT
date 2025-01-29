using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class NodeCreation : MonoBehaviour
{
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private MRUK mruk; 
    [SerializeField] private EffectMesh effectMesh;
    [SerializeField] private HandGrabInteractor rightHand;
    
    private List <GameObject> nodeObjects = new List<GameObject>();
    private List<int> nodeIds = new List<int>();
    private bool isNodeCreated = false;

    private bool canCreateNodes;
    // Start is called before the first frame update
    public void SetCreateNodes(bool draw)
    {
        canCreateNodes = draw;
    }

    // Update is called once per frame
    void Update()
    {
        if (AnchorManager.Instance.AnchorCreated())
        {
            if (canCreateNodes)
            {
                if (Physics.Raycast(rightHand.PalmPoint.position, rightHand.PalmPoint.forward,
                        out RaycastHit hitInfo, 0.03f))
                {
                    // UIDebugger.Log("Ray hit: " + hitInfo.collider.gameObject.name);

                    if (hitInfo.collider.gameObject.name == "WALL_FACE_EffectMesh")
                    {
                        if (isNodeCreated == false)
                        {
                            Vector3 position = hitInfo.point - hitInfo.normal * 0.035f;
                            Quaternion rotation = Quaternion.LookRotation(hitInfo.normal);
                            var nodeObject = Instantiate(nodePrefab, hitInfo.point, rotation);
                            nodeObject.transform.SetParent(AnchorManager.Instance.mainAnchor.transform);
                            isNodeCreated = true;
                            StartCoroutine(ResetNodeCreation());
                            nodeObject.GetComponent<NodeLevel>().currentLevel = Level.baseLevel;
                            //UIDebugger.Log(nodeObject.GetComponent<NodeLevel>().currentLevel.ToString());
                            nodeObjects.Add(nodeObject);
                            nodeObject.GetComponent<NodeLevel>().nodeIndex = nodeObjects.Count - 1;
                        }
                    }
                    else if(hitInfo.collider.CompareTag("Plane"))
                    {
                        Vector3 position = hitInfo.point;
                        Quaternion rotation = Quaternion.LookRotation(-hitInfo.normal);
                    }
                }
            }
        }
    }

    private IEnumerator ResetNodeCreation()
    {
        yield return new WaitForSeconds(1f);
        isNodeCreated = false;
    }
}
