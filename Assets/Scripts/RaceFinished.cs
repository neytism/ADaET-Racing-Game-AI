using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceFinished : MonoBehaviour
{

    public GameObject raceFinishedPanel;

    public TextMeshProUGUI congratsText;

    public TextMeshProUGUI positionText;

    public GameObject restartButton;
    public GameObject quitButton;
    
    public GameObject player;
    private void OnEnable()
    {
        player.GetComponent<CarLapCounter>().PlayerFinishedRace += ShowFinishedPanel;
    }

    void ShowFinishedPanel()
    {
        player.GetComponent<CarInputHandler>().enabled = false;
        player.GetComponent<AIHandler>().enabled = true;
        raceFinishedPanel.SetActive(true);

        int pos = player.GetComponent<CarLapCounter>().carPosition;
        
        congratsText.text = pos < 3 ? "Congratulations!" : "You Lost!";
        
        positionText.text = $"You finished {AddOrdinal(pos)} place!";
        
        
        StartCoroutine(WaitForRestartButton());
    }
    
    private string AddOrdinal(int num)
    {
        if( num <= 0 ) return num.ToString();

        switch(num % 100)
        {
            case 11:
            case 12:
            case 13:
                return num + "th";
        }
    
        switch(num % 10)
        {
            case 1:
                return num + "st";
            case 2:
                return num + "nd";
            case 3:
                return num + "rd";
            default:
                return num + "th";
        }
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator WaitForRestartButton()
    {
        yield return new WaitForSeconds(3f);
        restartButton.SetActive(true);
        quitButton.SetActive(true);
    }
}
