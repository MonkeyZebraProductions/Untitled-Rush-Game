using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineTraveling : MonoBehaviour
{

    public float Duration;
    public bool CanTrick;

    private SplineAnimate playerSpline;
    private TrickSystem tS;


    private void Start()
    {
        tS = FindObjectOfType<TrickSystem>();
    }

    private void Update()
    {
        if (playerSpline.elapsedTime > Duration && playerSpline != null)
        {
            tS.TrickState = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag=="Player")
        {
            
            tS.TrickState = CanTrick;
            playerSpline = collision.gameObject.GetComponent<SplineAnimate>();
            playerSpline.splineContainer = gameObject.GetComponent<SplineContainer>();
            playerSpline.duration = Duration;
            if(playerSpline.elapsedTime>Duration)
            {
                playerSpline.Restart(true);
            }
            playerSpline.Play();
        }
    }

    public void StartSpline()
    {
        
    }

    public void StopSpline()
    {
        playerSpline.Pause();
        tS.TrickState = false;
    }
}
