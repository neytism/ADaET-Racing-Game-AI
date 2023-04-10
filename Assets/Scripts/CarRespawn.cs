using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarRespawn : MonoBehaviour
{

    public float wrongWayTimeBeforeRespawning = 3f;
    public float outOfTrackTimeBeforeRespawning = 6f;
    
    private float _wrongWayTimer;
    private float _outOfTrackTimer;
    
    private CarLapCounter _carLapCounter;
    private SpriteRenderer _sr;
    private void OnEnable()
    {
        _carLapCounter.OnPassCheckpoint += ResetTimer;
    }

    private void Awake()
    {
        _carLapCounter = GetComponent<CarLapCounter>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        _outOfTrackTimer -= Time.deltaTime;

        if (_outOfTrackTimer <= 0 && _carLapCounter.passedCheckPointNumber != 0)
        {
            Respawn();
            ResetTimer();
        }
        
        if (_carLapCounter._isWrongWay)
        {
            _wrongWayTimer -= Time.deltaTime;

            if (_wrongWayTimer <= 0  && _carLapCounter.passedCheckPointNumber != 0)
            {
                Respawn();
                ResetTimer();
            }
        }
        else
        {
            _wrongWayTimer = wrongWayTimeBeforeRespawning;
        }
    }

    void Respawn()
    {
        transform.position = _carLapCounter.tempCheckPoint.transform.position;
        StartCoroutine(RespawnVisualization());

    }

    void ResetTimer()
    {
        _wrongWayTimer = wrongWayTimeBeforeRespawning;
        _outOfTrackTimer = outOfTrackTimeBeforeRespawning;
    }

    IEnumerator RespawnVisualization()
    {
        
        _sr.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(0.25f);
        _sr.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(0.25f);
        
        _sr.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(0.25f);
        _sr.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(0.25f);
        
        _sr.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(0.25f);
        _sr.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(0.25f);
        
        _sr.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(0.25f);
        _sr.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(0.25f);
        
        _sr.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(0.25f);
        _sr.color = new Color(1f, 1f, 1f, 1f);
    }
}
