using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownUIHandler : MonoBehaviour
{

    public TextMeshProUGUI CountDownText;

    void Awake()
    {
        CountDownText.text = "";
    }
    
    private void OnEnable()
    {
        GameManager.RaceReady += StartCountdown;
    }

    private void OnDisable()
    {
        GameManager.RaceReady -= StartCountdown;
    }

    void StartCountdown()
    {
        CountDownText.gameObject.SetActive(true);
        StartCoroutine(CountDownCO());
    }

  

    IEnumerator CountDownCO()
    {
        yield return new WaitForSeconds(0.3f);

        CountDownText.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.red);
        
        int counter = 3;

        while (true)
        {
            if (counter != 0)
            {
                if (counter == 1)
                {
                    CountDownText.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.yellow);
                }
                
                CountDownText.text = counter.ToString();
            }
            else
            {
                CountDownText.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.green);
                CountDownText.text = "GO";

                GameManager.instance.OnRaceStart();

                break;
            }

            counter--;
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
    }

}
