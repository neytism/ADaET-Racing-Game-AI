using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStates { countDown, running, raceOver, difficultySelection};

public class GameManager : MonoBehaviour
{
    public static event Action RaceReady;
    
    //Static instance of GameManager to give access
    public static GameManager instance = null;

    GameStates gameState = GameStates.countDown;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameStates.difficultySelection;
    }

    public void LevelStart()
    {
        gameState = GameStates.countDown;

        RaceReady?.Invoke();
        Debug.Log("Level Started");
    }

    public GameStates GetGameStates()
    {
        return gameState;
    }

    public void OnRaceStart()
    {
        Debug.Log("OnRaceStart");
        gameState = GameStates.running;

    }

    public void ResetRace()
    {
        gameState = GameStates.difficultySelection;
    }

    // private void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }
    //
    // void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     LevelStart();
    // }

    

}
