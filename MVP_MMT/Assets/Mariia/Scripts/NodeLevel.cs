using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Level
{
    baseLevel =0,
    secondLevel=1,
    thirdLevel=2,
    fourthLevel=3
}
public class NodeLevel : MonoBehaviour
{
   
    public Level currentLevel;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevel(Level level)
    {
        currentLevel = level;
    }
}
