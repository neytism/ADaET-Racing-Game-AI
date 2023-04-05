using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public bool isFinishLine = false;
    public int checkPointNumber = 1;

    
    [HideInInspector] public bool isActiveCheckpoint = false;
    private SpriteRenderer _sr;

    private void Awake()
    {
        checkPointNumber = transform.GetSiblingIndex() + 1 ;
        _sr = GetComponent<SpriteRenderer>();
        
    }
    
    private void Update()
    {
        if (isActiveCheckpoint)
        {
            _sr.color = Color.green;
        }
        else
        {
            _sr.color = Color.white;
        }
    }

}
