using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingAttack : MonoBehaviour
{
    public LayerMask HomingLayer;
    public List<Transform> targets;
    public Vector2 CurrentTarget;
    public bool hasTarget;
    public int Facing;
    public int CurrentHomingMultiplier, HomingMultiplier;

    private PlayerMovement pM;

    private void Awake()
    {
        pM = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 7)
        {
            targets.Add(collision.gameObject.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            targets.Clear();
        }
    }

    //Checks if targets are withing Homing Vacinity
    public void CheckHoming()
    {
        pM.CanHomingAttack = false;
        float smallestDistance=1000;
        Debug.Log(smallestDistance);
        //Player does a small dash if no targets are near
        if (targets.Count==0)
        {
            Debug.Log("HomingDash");
            CurrentHomingMultiplier = 1;
            StartCoroutine(pM.HomingDash());
        }
        else //Player quickly homes in on target
        {
            //Checks closest Target
            foreach(Transform target in targets) 
            {
                if(Vector2.Distance(transform.position,target.position) < smallestDistance)
                {
                    Debug.Log("HomingAttack");
                    CurrentHomingMultiplier = HomingMultiplier;
                    smallestDistance = Vector2.Distance(transform.position, target.position);
                    CurrentTarget = target.position - transform.position;
                    pM.HomingAttack();
                }
            }
        }
    }
}
