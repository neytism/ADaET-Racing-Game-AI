using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTrailHandler : MonoBehaviour
{
    private CarController _carController;
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        _carController = GetComponentInParent<CarController>();
        _trailRenderer = GetComponent<TrailRenderer>();

        _trailRenderer.emitting = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_carController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            _trailRenderer.emitting = true;
        }
        else
        {
            _trailRenderer.emitting = false;
        }
    }
}
