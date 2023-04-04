using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public bool isFinishLine = false;
    public int checkPointNumber = 1;

    private void Awake()
    {
        checkPointNumber = transform.GetSiblingIndex() + 1 ;
    }

}
