using System.Collections;
using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction.HandGrab;
using Unity.Mathematics;
using UnityEngine;

public class AnchorManager : MonoBehaviour
{
    public static AnchorManager Instance;
    [SerializeField] private GameObject anchorPrefab;
    [SerializeField] private MRUK mruk; 
    [SerializeField] private EffectMesh effectMesh;
    [SerializeField] private HandGrabInteractor rightHand;

    public OVRSpatialAnchor mainAnchor;
    private Guid anchorUuid;
  
    private bool sceneHasBeenLoaded;
    private bool SceneAndRoomInfoAvailable => currentRoom != null && sceneHasBeenLoaded;
    private MRUKRoom currentRoom;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        mruk.RoomCreatedEvent.AddListener(BindRoomInfo);
    }

    private void OnDisable()
    {
        mruk.RoomCreatedEvent.RemoveListener(BindRoomInfo);
    }

    public void EnableMRUKDemo()
    {
        sceneHasBeenLoaded = true;
    }

    private void BindRoomInfo(MRUKRoom room)
    {
        currentRoom = room;
        UIDebugger.Log("Room bound");
    }

    private void Start()
    {
        UIDebugger.Log("AnchorManager started");
    }

    void Update()
    {
        if (SceneAndRoomInfoAvailable )
        {
            if (Physics.Raycast(rightHand.PalmPoint.position, rightHand.PalmPoint.forward,
                    out RaycastHit hitInfo, 0.05f))
            {
               // UIDebugger.Log("Ray hit: " + hitInfo.collider.gameObject.name);
                
                //if (hitInfo.collider.gameObject.name == "WALL_FACE_EffectMesh" ||
                    //hitInfo.collider.gameObject.name == "TABLE_EffectMesh") 
                if (hitInfo.collider.gameObject.name == "WALL_FACE_EffectMesh")
                {
                    if(mainAnchor == null)
                    {
                        effectMesh.HideMesh = true;
                        Quaternion rotation = Quaternion.LookRotation(hitInfo.normal);
                        CreateMainAnchor(hitInfo.point, rotation);
                    }
                    
                }
            }
        }
    }

    private async void CreateMainAnchor(Vector3 position, Quaternion rotation)
    {
       
        var anchorObject = Instantiate(anchorPrefab, position, rotation);
        mainAnchor = anchorObject.AddComponent<OVRSpatialAnchor>();

        
        if (!await mainAnchor.WhenLocalizedAsync())
        {
            Destroy(anchorObject);
            mainAnchor = null;
            return;
        }
        
        var saveResult = await mainAnchor.SaveAnchorAsync();
        if (saveResult.Success)
        {
            anchorUuid = mainAnchor.Uuid;
        }
    }

    public async void LoadMainAnchor()
    {
        if (mainAnchor == null)
        {
            var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
            var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(new[] { anchorUuid }, unboundAnchors);

            if (result.Success && unboundAnchors.Count > 0)
            {
                var unboundAnchor = unboundAnchors[0];
                await unboundAnchor.LocalizeAsync();

                var pose = unboundAnchor.Pose;
                var go = Instantiate(anchorPrefab, pose.position, pose.rotation);;
                mainAnchor = go.AddComponent<OVRSpatialAnchor>();
                unboundAnchor.BindTo(mainAnchor);

            }
            
        }
    }

    public async void EraseMainAnchor()
    {
        if (mainAnchor != null)
        {
            var eraseResult = await OVRSpatialAnchor.EraseAnchorsAsync(anchors: null, uuids: new[] { anchorUuid });
            if (eraseResult.Success)
            {
                Destroy(mainAnchor.gameObject);
                mainAnchor = null;
            }
        }
    }

    public bool AnchorCreated()
    {
        if (mainAnchor != null) return true;
        return false;
    }
}