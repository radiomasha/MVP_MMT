using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIDebugger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _debugText;
    private static UIDebugger instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    // Start is called before the first frame update
    public static void Log(string text)
    {
        if (instance != null && instance._debugText != null)
        {
            instance._debugText.text += text + "\n";
        }
    }
}