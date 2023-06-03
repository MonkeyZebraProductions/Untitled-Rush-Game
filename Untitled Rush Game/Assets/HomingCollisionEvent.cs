using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HomingCollisionEvent : MonoBehaviour
{
    public UnityEvent CollisonEvent;
    public Collider2D ColliderToCompare;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision==ColliderToCompare)
        {
            Debug.Log("Done");
            CollisonEvent.Invoke();
        }
    }
}
