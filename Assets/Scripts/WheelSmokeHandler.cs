using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSmokeHandler : MonoBehaviour
{
    private float particeEmissionRate = 0;

    private CarController _carController;

    private ParticleSystem _particleSystemSmoke;
    private ParticleSystem.EmissionModule _particleSystemModule;

    private void Awake()
    {
        _carController = GetComponentInParent<CarController>();
        _particleSystemSmoke = GetComponent<ParticleSystem>();
        _particleSystemModule = _particleSystemSmoke.emission;
        _particleSystemModule.rateOverTime = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        particeEmissionRate = Mathf.Lerp(particeEmissionRate, 0, Time.deltaTime * 5);
        _particleSystemModule.rateOverTime = particeEmissionRate;

        if (_carController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (isBraking)
            {
                particeEmissionRate = 30;
            }
            else
            {
                particeEmissionRate = Mathf.Abs(lateralVelocity) * 20;
            }
        }
    }
}
