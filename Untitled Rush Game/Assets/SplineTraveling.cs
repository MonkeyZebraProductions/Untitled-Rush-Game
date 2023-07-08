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
    private PlayerMovement pM;
    private bool splineEnded;


    private void Start()
    {
        tS = FindObjectOfType<TrickSystem>();
        pM = FindObjectOfType<PlayerMovement>();
    }

    private void Update()
    {
        if (playerSpline.elapsedTime > Duration && playerSpline != null)
        {
            StopSpline();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag=="Player")
        {
            splineEnded = false;
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
        if(!splineEnded)
        {
            playerSpline.Pause();
            tS.TrickState = false;
            pM.ResetMomentum();
            splineEnded = true;
        }
        
    }
}
