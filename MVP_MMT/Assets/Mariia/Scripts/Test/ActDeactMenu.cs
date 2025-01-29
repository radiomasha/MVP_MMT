using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class ActDeactMenu : MonoBehaviour
{
    [SerializeField] private GameObject menu;

    [SerializeField] private GameObject titleAudio;

    [SerializeField] private GameObject files;

    private bool isActivated = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ActivateMenu();
    }

    public void ChangeBool()
    {
        isActivated = !isActivated;
        UIDebugger.Log(isActivated.ToString());
    }
    private void ActivateMenu()
    {
        if (isActivated)
        {
            menu.SetActive(true);
            titleAudio.SetActive(true);
            files.SetActive(true);
        }
        else
        {
            menu.SetActive(false);
            titleAudio.SetActive(false);
            files.SetActive(false);
        }
        
    }
}
