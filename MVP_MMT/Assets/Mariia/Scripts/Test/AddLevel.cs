using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction.Grab;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

public class  AddLevel : MonoBehaviour
{
    [SerializeField] private float levelDistance = 0.3f;
    [SerializeField] private GameObject nodePrefab;
    
    private HandGrabPose pose;
    private HandGrabInteractable interactable;
    private HandGrabInteractor currentInteractor;
    bool isReadyCreate = false;
    
    // Start is called before the first frame update

    
    void Start()
    {
        interactable = GetComponentInChildren<HandGrabInteractable>();
        
    }

    private void Update()
    {
        
        if (interactable.SelectingInteractors.Count > 0)
        {
          currentInteractor = interactable.SelectingInteractors.First();
          {
              if (currentInteractor != null)
              {
           
              }
          }
        }

        
    }
    
    // Update is called once per frame
    private void CreateLevel()
    {
       UIDebugger.Log("Creating level");
        if (IsPalmGrab()==true)
        {
            isReadyCreate = true;
        }
        if (isReadyCreate&&Vector3.Distance(transform.position, currentInteractor.PalmPoint.position) >= levelDistance)
        {
            UIDebugger.Log("call");
            Vector3 scale = transform.GetComponentInChildren<GameObject>().transform.localScale;
            Vector3 pos = interactable.transform.position - Vector3.forward * levelDistance;
            var node = Instantiate(nodePrefab, pos, Quaternion.identity );
            node.transform.localScale = scale;
            NodeLevel level = node.GetComponent<NodeLevel>();
            NodeLevel currentLevelComp = GetComponent<NodeLevel>();
            level.currentLevel = currentLevelComp.currentLevel + 1;
            UIDebugger.Log(level.currentLevel.ToString());
        }
    }

    private bool IsPalmGrab()
    {
        if(currentInteractor.Hand.GetFingerIsPinching(HandFinger.Middle)==true&&
           currentInteractor.Hand.GetFingerIsPinching(HandFinger.Ring)==true&&
           currentInteractor.Hand.GetFingerIsPinching(HandFinger.Thumb)==true)
            return true;
        return false;
    }
}
