using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class CarLapCounter : MonoBehaviour
{
    //public TextMeshProUGUI carPositionText;
    
    [Header("Car name")] 
    public string carName = "";
    public TextMeshProUGUI positionText;

    int passedCheckPointNumber = 0;
    float timeAtLastPassedCheckPoint = 0;

    private string timeText;

    int numberOfPassedCheckpoints = 0;

    int lapsCompleted = 0;
    const int lapsToComplete = 10;

    bool isRaceCompleted = false;

    int carPosition = 0;
    int numberOfCars;

    bool isHideRoutineRunning = false;
    float hideUIDelayTime;

    private CheckPoint tempCheckPoint;


    //Events
    public event Action<CarLapCounter> OnPassCheckpoint;

    public void SetCarPosition(int position)
    {
        carPosition = position;
    }

    public void SetNumberOfCars(int value)
    {
        numberOfCars = value;
    }

    public int GetNumberOfCheckpointsPassed()
    {
        return numberOfPassedCheckpoints;
    }
    public float GetTimeAtLastCheckPoint()
    {
        return timeAtLastPassedCheckPoint;
    }

    IEnumerator ShowPositionCO(float delayUntilHidePosition)
    {
        hideUIDelayTime += delayUntilHidePosition;

        //carPositionText.text = carPosition.ToString();

        //carPositionText.gameObject.SetActive(true);

        if (!isHideRoutineRunning)
        {
            isHideRoutineRunning = true;

            yield return new WaitForSeconds(hideUIDelayTime);
            //carPositionText.gameObject.SetActive(false);

            isHideRoutineRunning = false;
        }

    }


    void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.CompareTag("CheckPoint"))
        {
            //Once a car has completed the race we don't need to check any checkpoints or laps. 
            if (isRaceCompleted)
                return;
            
            

            CheckPoint checkPoint = collider2D.GetComponent<CheckPoint>();
            
            if (gameObject.CompareTag("Player") && passedCheckPointNumber + 1 == checkPoint.checkPointNumber)
            {
                if (tempCheckPoint != null && tempCheckPoint != checkPoint)
                {
                    tempCheckPoint.isActiveCheckpoint = false;
                }
            }
            
            if (gameObject.CompareTag("Player"))
            {
                tempCheckPoint = checkPoint;
            }

            //Make sure that the car is passing the checkpoints in the correct order. The correct checkpoint must have exactly 1 higher value than the passed checkpoint
            if (passedCheckPointNumber + 1 == checkPoint.checkPointNumber)
            {
                passedCheckPointNumber = checkPoint.checkPointNumber;
                

                numberOfPassedCheckpoints++;
                

                //Store the time at the checkpoint
                timeAtLastPassedCheckPoint = Time.time;

                string minutes = Math.Floor(timeAtLastPassedCheckPoint / 60).ToString("00");
                string seconds = (timeAtLastPassedCheckPoint % 60).ToString("00");
                string milliseconds = Mathf.Floor((timeAtLastPassedCheckPoint*1000) % 1000).ToString("0000");

                timeText = $"{minutes}:{seconds}:{milliseconds}";

                if (gameObject.CompareTag("Player"))
                {
                    positionText.text = $"{carPosition} / {numberOfCars}";
                    checkPoint.isActiveCheckpoint = true;
                }
                
                if (checkPoint.isFinishLine)
                {
                    
                    passedCheckPointNumber = 0;
                    lapsCompleted++;
                    
                    Debug.Log($"{String.Format("{0, -12}", timeText)} | {String.Format("{0, -10}", carName)} | Laps Completed: {lapsCompleted} ");
                    
                    if (lapsCompleted >= lapsToComplete)
                        isRaceCompleted = true;
                }


                //Invoke the passed checkpoint event
                OnPassCheckpoint?.Invoke(this);



                //Now show the cars position as it has been calculated but only do it when a car passes through the finish line
                if (isRaceCompleted && gameObject.CompareTag("Player"))
                {
                    //Jai- To freeze the game, tas ipasok UI dto
                    Time.timeScale = 0;
                    StartCoroutine(ShowPositionCO(100));
                }
                //Jai - prang uneccesary neto?
                else if (checkPoint.isFinishLine)
                    StartCoroutine(ShowPositionCO(1.5f));
            }
            
        }
    }
}
