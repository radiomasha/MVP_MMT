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
    
    private bool isNodeCreated = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (AnchorManager.Instance.AnchorCreated())
        {
            if (Physics.Raycast(rightHand.PalmPoint.position, rightHand.PalmPoint.forward,
                    out RaycastHit hitInfo, 0.01f))
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
                        NodeLevel level = nodeObject.GetComponent<NodeLevel>();
                        level.SetLevel(Level.baseLevel);
                        UIDebugger.Log(level.currentLevel.ToString());
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
