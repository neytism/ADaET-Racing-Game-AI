using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car settings")] 
    public string carName = "";
    public float acceleratorFactor = 30.0f;
    public float turnFactor = 3.5f;
    public float driftFactor = 0.95f;
    public float dragFactor = 3;
    public float maximumSpeed = 20;

    private float accelerationInput = 0;
    private float steeringInput = 0;
    private float rotationAngle = 0;
    private float velocityVsUp = 0;

    private Rigidbody2D carRigidBody2D;

    private int _lapCount = 0;

    private void Awake()
    {
        carRigidBody2D = GetComponent<Rigidbody2D>();
        
    }
    
    private void FixedUpdate()
    {
        ApplyEngineForce();
        KillOrthoganoalVelocity();
        ApplySteering();
        
        
    }
    
    public void SetInputVector(Vector2 input)
    {
        steeringInput = input.x;
        accelerationInput = input.y;
    }


    private void ApplyEngineForce()
    {
        //gets the speed
        velocityVsUp = Vector2.Dot(transform.up, carRigidBody2D.velocity);

        //limits forward
        if ((velocityVsUp > maximumSpeed) && (accelerationInput > 0))
        {
            return;
        }
        
        //limits reverse
        if ((velocityVsUp < -maximumSpeed * .5f) && (accelerationInput < 0))
        {
            return;
        }
        
        //limit any direction
        if ((carRigidBody2D.velocity.sqrMagnitude > maximumSpeed * maximumSpeed) && (accelerationInput > 0))
        {
            return;
        }
        
        //apply drag
        if (accelerationInput == 0)
        {
            carRigidBody2D.drag = Mathf.Lerp(carRigidBody2D.drag, dragFactor, Time.fixedDeltaTime * dragFactor);
        }
        else
        {
            carRigidBody2D.drag = 0;
        }
        
        Vector2 engineForceVector = transform.up * accelerationInput * acceleratorFactor;
        carRigidBody2D.AddForce(engineForceVector,ForceMode2D.Force);
    }
    
    private void ApplySteering()
    {
        float minimumSpeedBeforeTurning = carRigidBody2D.velocity.magnitude / 8;
        minimumSpeedBeforeTurning = Mathf.Clamp01(minimumSpeedBeforeTurning);
            
        rotationAngle -= steeringInput * turnFactor * minimumSpeedBeforeTurning;
        carRigidBody2D.MoveRotation(rotationAngle);
    }

    
    private void KillOrthoganoalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidBody2D.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidBody2D.velocity, transform.right);

        carRigidBody2D.velocity = forwardVelocity + rightVelocity * driftFactor;
    }

    private float GetLateralVelocity()
    {
        //speed of car if moving sideways
        return Vector2.Dot(transform.right, carRigidBody2D.velocity);
    }

    public bool IsTireScreeching(out float lateralVelocity, out bool isBraking)
    {
        lateralVelocity = GetLateralVelocity();
        isBraking = false;

        if (accelerationInput < 0 && velocityVsUp > 0)
        {
            isBraking = true;
            return true;
        }

        if (Mathf.Abs(GetLateralVelocity()) > 1.5f)
        {
            return true;
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("FinishLine"))
        {
            _lapCount++;
            Debug.Log($"{carName}'s lap count: {_lapCount}");
            
        }
    }

    public float GetVelocityMagnitude()
    {
        return carRigidBody2D.velocity.magnitude;
    }
}
