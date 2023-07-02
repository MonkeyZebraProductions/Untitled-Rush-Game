using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoostBar : MonoBehaviour
{

    public float MaxBoost;
    public float DecreaseRate=1;
    public Slider Bar;
    public bool StartDecreace,CanBoost;

    public float currentBoost;
    // Start is called before the first frame update
    void Start()
    {
        Bar.maxValue = MaxBoost;
        currentBoost = MaxBoost;
        StartDecreace = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Bar.value = currentBoost;

        if(currentBoost > MaxBoost)
        {
            currentBoost = MaxBoost;
        }

        if(currentBoost<=0)
        {
            StartDecreace = false;
            CanBoost = false;
            currentBoost = 0;
        }
        else
        {
            CanBoost = true;
        }

        if(StartDecreace)
        {
            currentBoost -= DecreaseRate * Time.deltaTime;
        }
    }

    public void IncreaseBoost(int IncreaseValue)
    {
        currentBoost += IncreaseValue;
    }
}
