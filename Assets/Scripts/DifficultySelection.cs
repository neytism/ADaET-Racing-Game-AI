using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultySelection : MonoBehaviour
{
    private AIHandler[] _cars;

    private void Awake()
    {
        _cars = FindObjectsOfType<AIHandler>();
    }

    private void SetDifficulty(AIHandler.AIDifficulty dif)
    {
        foreach (var item in _cars)
        {
            item.ApplyDifficulty(dif);
        }

        GameManager.instance.LevelStart();
        gameObject.SetActive(false);
    }

    public void SetEasy() { SetDifficulty(AIHandler.AIDifficulty.easy);}
    
    public void SetNormal() { SetDifficulty(AIHandler.AIDifficulty.meduim); }
    
    public void SetHard() { SetDifficulty(AIHandler.AIDifficulty.hard); }
    
    public void SetExtreme() { SetDifficulty(AIHandler.AIDifficulty.extreme); }
    
    
}
