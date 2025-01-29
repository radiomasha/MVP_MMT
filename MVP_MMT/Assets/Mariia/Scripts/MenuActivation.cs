using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class MenuActivation : MonoBehaviour
{
    [SerializeField] private GameObject menu;

    [SerializeField] private GameObject titleMic;
    [SerializeField] private GameObject files;
    private bool activated = false;
    
    // Start is called before the first frame update
    void Start()
    {
        var interactable = GetComponentInChildren<HandGrabInteractable>();
        interactable.WhenStateChanged += OnTouched;
    }

    private void Update()
    {
        ActDeact(activated);
       
    }

    private void ActDeact(bool state)
    {
        if (state)
        {
            menu.SetActive(true);
            titleMic.SetActive(true);
            files.SetActive(true);
        }
        else
        {
            menu.SetActive(false);
            titleMic.SetActive(false);
            files.SetActive(false);
        }
    }
    private void OnTouched(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select)
        {
            activated=!activated;
        }
        UIDebugger.Log(activated.ToString());
    }
}
