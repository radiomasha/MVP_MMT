using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;

public class CreateSubLevel : MonoBehaviour
{
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private Material planeMaterial;
    [SerializeField] private float levelDistance = 0.3f;

    private HandGrabInteractable node;
    private List<HandGrabInteractor> grabInteractors = new List<HandGrabInteractor>();
    // Start is called before the first frame update
    void Start()
    {
        node = GetComponent<HandGrabInteractable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (node.SelectingInteractors.Count > 1)
        {
            grabInteractors.AddRange(node.SelectingInteractors);
            Vector3 posOne = grabInteractors[0].transform.position;
            Vector3 posTwo = grabInteractors[1].transform.position;
            float initialDistance = Vector3.Distance(posOne, posTwo);
            if (Vector3.Distance(posOne, posTwo) > initialDistance&& node.GetComponent<NodeLevel>().currentLevel<= Level.secondLevel
                && !node.GetComponent<HasNode>())
            {
                AddLevel(node.transform.position, node.GetComponent<NodeLevel>().currentLevel,
                    node.GetComponent<NodeLevel>().nodeIndex);
                node.AddComponent<HasNode>();
            }
        }
    }

    private void AddLevel(Vector3 pos, Level currentLevel, int index)
    {
        Vector3 nodePos = pos - Vector3.forward * levelDistance;
        var node = Instantiate(nodePrefab, nodePos, Quaternion.identity);
        node.GetComponent<NodeLevel>().currentLevel = currentLevel + 1;
        node.GetComponent<NodeLevel>().nodeIndex = index;
        UIDebugger.Log("node created");
        var plane = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Quad), node.transform.position,
            Quaternion.identity);
        plane.GetComponent<Renderer>().material = planeMaterial;
        plane.transform.SetParent(node.transform);
    }
}
