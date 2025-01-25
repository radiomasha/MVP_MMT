using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using HandFinger = Oculus.Interaction.Input.HandFinger;


public class NodeLevelManager : MonoBehaviour
{
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private float levelDistance = 0.35f;
    private HandGrabInteractor currentInteractor;
    private HandGrabInteractable currentInteractable;
    private bool isNodeCreated = false;
    private List<Transform> nodes = new List<Transform>();
    private Transform currentNode = null;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FindCurrentNode();
        if (currentInteractable != null)
        {
           bool checkIfAdded = CheckHasLevelAdded();
           if (checkIfAdded) return;
           AddNodeLevel();
        }
    }

    private bool CheckHasLevelAdded()
    {
        if (currentInteractable.GetComponentInChildren<NodeLevel>()) return true;
        return false;
    }
    private void FindCurrentNode()
    {
        FindInteractor();
        if (currentInteractor != null && currentInteractor.IsGrabbing)
        {
            if (currentInteractor.SelectedInteractable.GetComponentInParent<NodeLevel>())
            {
                currentInteractable = currentInteractor.SelectedInteractable;
                currentNode = currentInteractable.GetComponentInParent<Transform>();
            } 
        }
    }
    private void FindInteractor()
    {
        HandGrabInteractor[] handGrabInteractors = FindObjectsOfType<HandGrabInteractor>();
        foreach (var interactor in handGrabInteractors)
        {
            if (interactor.isActiveAndEnabled&& interactor.IsGrabbing)
            {
                currentInteractor = interactor;
                break;
            }
        }
    }

    private void AddNodeLevel()
    {
        bool isPalmGrab = IsPalmGrab();
        if (isPalmGrab)
        {
            if (Vector3.Distance(currentNode.position, currentInteractor.transform.position) >=
                levelDistance)
            {
                if (isNodeCreated == false)
                {
                    Vector3 pos = currentNode.position - Vector3.forward * levelDistance;
                    var node = Instantiate(nodePrefab, pos, Quaternion.identity);
                    NodeLevel level = node.GetComponent<NodeLevel>();
                    NodeLevel currentLevelComp = currentNode.GetComponent<NodeLevel>();
                    if (level != null && currentLevelComp != null)
                    {
                        level.currentLevel = currentLevelComp.currentLevel + 1;
                        UIDebugger.Log(level.currentLevel.ToString());
                    }

                    isNodeCreated = true;
                    StartCoroutine(Delay());
                }
            }
        }
    }
    
    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(2f);
        isNodeCreated = false;
    }
    private bool IsPalmGrab()
    {
        if (currentInteractor.Hand.GetFingerIsPinching(HandFinger.Middle) &&
            currentInteractor.Hand.GetFingerIsPinching(HandFinger.Ring) &&
            currentInteractor.Hand.GetFingerIsPinching(HandFinger.Thumb) &&
            currentInteractor.Hand.GetFingerIsPinching(HandFinger.Index))
        {
            return true;
        }
        return false;
    }

    private bool Pinching(HandGrabInteractor interactor)
    {
        if (interactor.Hand.GetFingerIsPinching(HandFinger.Thumb) &&
            interactor.Hand.GetFingerIsPinching(HandFinger.Index))
        {
            return true;
        }

        return false;
    }
}
