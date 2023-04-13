using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class AIHandler : MonoBehaviour
{
    public enum AIMode
    {
        followPlayer,
        followWaypoints
    }
    
    public enum AIDifficulty
    {
        easy,
        meduim,
        hard,
        extreme
    }

    public AIMode aiMode;
    public AIDifficulty aiDifficulty;
    public bool isAvoidingCars = true;
    public float driveToTargetInfluenceFactor = 6f;
    public float minDistanceDetectCar = 6f;
    public float maxSpeed = 8;
    [Range(0.0f, 1.0f)] public float skillLevel = 1.0f;
    [Header("higher = more jitter & more responsive")]public float reactionSpeed = 4f;
    [Header("Enable if there is obstacles in track")]public bool isAStarEnabled = false;

    private Transform raycastHitCar;
    
    private Vector2 _avoidanceVectorLerped = Vector2.zero;

    private Vector3 _targetPosition = Vector3.zero;
    private Transform _targetTransform = null;
    private float _aiSpeedFactor = 1.05f;
    private float _originalMaxSpeed = 0f;
    private float _tempAccelerator;

    private float _maxSkillLevel;
    private float _minSkillLevel;
    
    //Stuck handling
    bool isRunningStuckCheck = false;
    bool isFirstTemporaryWaypoint = false;
    int stuckCheckCounter = 0;
    List<Vector2> temporaryWaypoints = new List<Vector2>();
    float angleToTarget = 0;

    private WaypointNode _currentWaypoint = null;
    private WaypointNode _previousWaypoint = null;
    private WaypointNode _temporaryWaypoint = null;
    private WaypointNode[] _allWayPoints;
    
    private CarController _carController;
    private Collider2D _collider2D;
    AStarLite aStarLite;

    
    
    //visualization
    

    private void Awake()
    {
        _carController = GetComponent<CarController>();
        _allWayPoints = FindObjectsOfType<WaypointNode>();
        _collider2D = GetComponent<Collider2D>();
        _originalMaxSpeed = maxSpeed;
        _tempAccelerator = _carController.acceleratorFactor;
        
        aStarLite = GetComponent<AStarLite>();

        //get the starting point
        foreach (var wp in _allWayPoints)
        {
            if (wp.isStartingPoint)
            {
                _currentWaypoint = wp;
            }
        }

        _previousWaypoint = _currentWaypoint;
        SetMaxSpeedBasedOnSkillLevel(maxSpeed);
        
    }

    private void Start()
    {
        if (gameObject.CompareTag("AI"))
        {
            switch (aiDifficulty)
            {
                case AIDifficulty.easy:
                    _minSkillLevel = 0.85f;
                    _maxSkillLevel = 0.89f;
                    break;
                case AIDifficulty.meduim:
                    _minSkillLevel = 0.90f;
                    _maxSkillLevel = 0.94f;
                    break;
                case AIDifficulty.hard:
                    _minSkillLevel = 0.95f;
                    _maxSkillLevel = 0.99f;
                    break;
                case AIDifficulty.extreme:
                    _minSkillLevel = 1f;
                    _maxSkillLevel = 1f;
                    break;
            }
            
            StartCoroutine(RandomSkill());
        }
    }


    void FixedUpdate()
    {

        if (GameManager.instance.GetGameStates() == GameStates.countDown)
            return;

        Vector2 inputVector = Vector2.zero;

        switch (aiMode)
        {
            case AIMode.followPlayer:
                FollowPlayer();
                break;
            case AIMode.followWaypoints:
                if (temporaryWaypoints.Count == 0 && !isAStarEnabled)
                    FollowWaypoints();
                else FollowTemporaryWayPoints();
                
                break;
        }

        inputVector.x = TurnTowardTarget();
        inputVector.y = ApplyAISpeed(inputVector.x);
        
        //If the AI is applying throttle but not manging to get any speed then lets run our stuck check.
        if (_carController.GetVelocityMagnitude() < 0.5f && Mathf.Abs(inputVector.y) > 0.01f && !isRunningStuckCheck)
            StartCoroutine(StuckCheckCO());

        //Handle special case where the car has reversed for a while then it should check if it is still stuck. If it is not then it will drive forward again.
        if (stuckCheckCounter >= 4 && !isRunningStuckCheck)
            StartCoroutine(StuckCheckCO());

        
        _carController.SetInputVector(inputVector);
    }
    
    private void FollowPlayer()
    {
        if (_targetTransform == null)
        {
            //will always find Player if null
            _targetTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
        else
        {
            _targetPosition = _targetTransform.position;
        }
        
    }
    
    private void FollowWaypoints()
    {
        if (_currentWaypoint == null)
        {
            _currentWaypoint = FindClosestWaypoint();
            _previousWaypoint = _currentWaypoint;
        }
        else
        {
            //following waypoint
            _targetPosition = _currentWaypoint.transform.position;
            
            //visualization
            if (_carController.CompareTag("Player"))
            {
                _temporaryWaypoint = _currentWaypoint;
                _temporaryWaypoint.isActiveNode = true;
            }

            //changing waypoint
            float distanceToWaypoint = (_targetPosition - transform.position).magnitude;

            if (distanceToWaypoint > 8)
            {
                Vector3 nearestPointOnTheWaypointLine = FindNearestPointOnLine(_previousWaypoint.transform.position, _currentWaypoint.transform.position, transform.position);

                float segments = distanceToWaypoint/ 8f;

                _targetPosition = (_targetPosition + nearestPointOnTheWaypointLine * segments)/ (segments+1);

                Debug.DrawLine(transform.position, _targetPosition, Color.cyan);
            }


            if (distanceToWaypoint <= _currentWaypoint.minDistanceToReachWaypoint)
            {
                if (_currentWaypoint.maxSpeed > 0)
                {
                    SetMaxSpeedBasedOnSkillLevel(_currentWaypoint.maxSpeed);
                }
                else
                {
                    SetMaxSpeedBasedOnSkillLevel(1000);
                }

                _previousWaypoint= _currentWaypoint;

                //will pick randomly if splitNode
                _currentWaypoint = _currentWaypoint.nextWaypoint[Random.Range(0, _currentWaypoint.nextWaypoint.Length)];
                
                //visualization
                if (_carController.CompareTag("Player"))
                {
                    _temporaryWaypoint.isActiveNode = false;
                }
                
            }

        }
    }
    
    //AI follows waypoints
    void FollowTemporaryWayPoints()
    {
        //Set the target position of for the AI. 
        _targetPosition = temporaryWaypoints[0];

        //Store how close we are to the target
        float distanceToWayPoint = (_targetPosition - transform.position).magnitude;

        //Drive a bit slower than usual
        //SetMaxSpeedBasedOnSkillLevel(_originalMaxSpeed/3);

        //Check if we are close enough to consider that we have reached the waypoint
        float minDistanceToReachWaypoint = .75f;

        if (!isFirstTemporaryWaypoint)
            minDistanceToReachWaypoint = 1.5f;

        if (distanceToWayPoint <= minDistanceToReachWaypoint)
        {
            temporaryWaypoints.RemoveAt(0);
            isFirstTemporaryWaypoint = false;
        }
    }
    
    private WaypointNode FindClosestWaypoint()
    {
        //finding closest waypoint using Linq
        return _allWayPoints.OrderBy(t => Vector3.Distance(transform.position, t.transform.position)).FirstOrDefault();
    }
    
    private float TurnTowardTarget()
    {
        Vector2 vectorToTarget = _targetPosition - transform.position;
        vectorToTarget.Normalize();

        if (isAvoidingCars)
        {
            AvoidCars(vectorToTarget, out vectorToTarget);
        }

        angleToTarget = Vector2.SignedAngle(transform.up, vectorToTarget);
        angleToTarget *= -1;

        float steerAmount = angleToTarget / 45.0f;

        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);

        return steerAmount;
        
        //this will return the valur of steering based on the position of target
    }

    private float ApplyAISpeed(float inputX)
    {
        if (_carController.GetVelocityMagnitude() > maxSpeed )
        {
            return 0;
        }

        float reduceSpeedDueToCornering = Mathf.Abs(inputX) / 1f;
        float throttle = _aiSpeedFactor - reduceSpeedDueToCornering * skillLevel;
        
        //Handle throttle differently when we are following temp waypoints
        if (temporaryWaypoints.Count() != 0)
        {
            //If the angle is larger to reach the target the it is better to reverse. 
            if (angleToTarget > 70)
                throttle *= -1;
            else if (angleToTarget < -70)
                throttle *= -1;
            //If we are still stuck after a number of attempts then just reverse. 
            else if (stuckCheckCounter > 3)
                throttle *= -1;
        }

        return throttle;

    }

    void SetMaxSpeedBasedOnSkillLevel(float newSpeed)
    {
        maxSpeed = Mathf.Clamp(newSpeed, 0, _originalMaxSpeed);

        float skillBasedMaxSpeed = Mathf.Clamp(skillLevel, 0.3f, 1f);
        maxSpeed *= skillBasedMaxSpeed;
    }

    private Vector2 FindNearestPointOnLine(Vector2 lineStartPos, Vector2 lineEndPos, Vector2 point)
    {
        Vector2 lineHeading = lineEndPos - lineStartPos;

        float maxDistance = lineHeading.magnitude;
        lineHeading.Normalize();

        Vector2 lineVectorStartToPoint = point - lineStartPos;
        float dotProduct = Vector2.Dot(lineVectorStartToPoint, lineHeading);

        dotProduct = Mathf.Clamp(dotProduct, 0, maxDistance);

        return lineStartPos + lineHeading* dotProduct;
    }

    //Checks for cars ahead of the car.
    bool IsCarsInFrontOfAICar(out Vector3 position, out Vector3 otherCarRightVector)
    {
        //Disable the cars own collider to avoid having the AI car detect itself. 
        _collider2D.enabled = false;

        //Perform the circle cast in front of the car with a slight offset forward and only in the Car layer
        RaycastHit2D raycastHit2d = Physics2D.CircleCast(transform.position + transform.up * 0.5f, .25f, transform.up, minDistanceDetectCar, 1 << LayerMask.NameToLayer("Cars"));

        raycastHitCar = raycastHit2d.transform;
        
        //Enable the colliders again so the car can collide and other cars can detect it.  
        _collider2D.enabled = true;

        if (raycastHit2d.collider != null)
        {
            //Draw a red line showing how long the detection is, make it red since we have detected another car
            Debug.DrawRay(transform.position, transform.up * minDistanceDetectCar, Color.red);

            position = raycastHit2d.collider.transform.position;
            otherCarRightVector = raycastHit2d.collider.transform.right;
            return true;
        }
        else
        {
            //We didn't detect any other car so draw black line with the distance that we use to check for other cars. 
            Debug.DrawRay(transform.position, transform.up * minDistanceDetectCar, Color.black);
        }

        //No car was detected but we still need assign out values so lets just return zero. 
        position = Vector3.zero;
        otherCarRightVector = Vector3.zero;

        return false;
    }

    void AvoidCars(Vector2 vectorToTarget, out Vector2 newVectorToTarget)
    {
        if (IsCarsInFrontOfAICar(out Vector3 otherCarPosition, out Vector3 otherCarRightVector))
        {
            Vector2 avoidanceVector = Vector2.zero;

            //Calculate the reflecing vector if we would hit the other car. 
            avoidanceVector = Vector2.Reflect((otherCarPosition - transform.position).normalized, otherCarRightVector);

            float distanceToTarget = (_targetPosition - transform.position).magnitude;

            //We want to be able to control how much desire the AI has to drive towards the waypoint vs avoiding the other cars. 
            //As we get closer to the waypoint the desire to reach the waypoint increases.

            float driveToTargetInfluence;

            float distanceToCar = Vector2.Distance(transform.position, raycastHitCar.position) - .5f;

            if (distanceToCar < 0.15f)
            {
                driveToTargetInfluence = 0.1f;
            }
            else
            {
                //Ensure that we limit the value to between x% and 100% as we always want the AI to desire to reach the waypoint.  
                driveToTargetInfluence = driveToTargetInfluenceFactor / distanceToTarget;
                driveToTargetInfluence = Mathf.Clamp(driveToTargetInfluence, 0.75f, 1f);
            }

            //The desire to avoid the car is simply the inverse to reach the waypoint
            float avoidanceInfluence = 1.0f - driveToTargetInfluence;

            //Reduce jittering a little bit by using a lerp
            _avoidanceVectorLerped = Vector2.Lerp(_avoidanceVectorLerped, avoidanceVector, Time.fixedDeltaTime * reactionSpeed);

            //Calculate a new vector to the target based on the avoidance vector and the desire to reach the waypoint
            newVectorToTarget = (vectorToTarget * driveToTargetInfluence + avoidanceVector * avoidanceInfluence);
            newVectorToTarget.Normalize();

            if (gameObject.CompareTag("Player"))
            {
                //Draw the vector which indicates the avoidance vector in green
                Debug.DrawRay(transform.position, avoidanceVector * 10, Color.green);

                //Draw the vector that the car will actually take in yellow. 
                Debug.DrawRay(transform.position, newVectorToTarget * 10, Color.yellow);
            }
            
            
            _carController.acceleratorFactor = _tempAccelerator/1.5f;

            //we are done so we can return now. 
            return;
        }

        _carController.acceleratorFactor = _tempAccelerator;

        //We need assign a default value if we didn't hit any cars before we exit the function. 
        newVectorToTarget = vectorToTarget;
    }

    
    IEnumerator StuckCheckCO()
    {
        Vector3 initialStuckPosition = transform.position;

        isRunningStuckCheck = true;

        yield return new WaitForSeconds(0.5f);

        //if we have not moved for a second then we are stuck
        if ((transform.position - initialStuckPosition).sqrMagnitude < 1.5f)
        {
            //Get a path to the desired position
            temporaryWaypoints = aStarLite.FindPath(_currentWaypoint.transform.position);

            //If there was no path found then it will be null so if that happens just make a new empty list.
            if (temporaryWaypoints == null)
                temporaryWaypoints = new List<Vector2>();

            stuckCheckCounter++;

            isFirstTemporaryWaypoint = true;
        }
        else stuckCheckCounter = 0;

        isRunningStuckCheck = false;
    }
    
    
    IEnumerator RandomSkill()
    {
        yield return new WaitForSeconds(5);
        skillLevel = Random.Range(_minSkillLevel,_maxSkillLevel);
        StartCoroutine(RandomSkill());
    }
    
    
}
