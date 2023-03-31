using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputHandler : MonoBehaviour
{
    private CarController _carController;

    private void Awake()
    {
        _carController = GetComponent<CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = Vector2.zero;

        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        
        _carController.SetInputVector(input);
    }
}
