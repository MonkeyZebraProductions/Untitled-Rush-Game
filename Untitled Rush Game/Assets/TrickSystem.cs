using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrickSystem : MonoBehaviour
{
    public int TrickAmount;
    public float TrickDeadzone,BreakBetweenTricks;

    public bool TrickState;

    private PlayerInput playerInput;
    private InputAction buttonTrickAction;
    private InputAction stickTrickAction;
    private BoostBar bB;
    private bool _trickTick;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = FindObjectOfType<PlayerMovement>().GetComponent<PlayerInput>();
        buttonTrickAction = playerInput.actions["ButtonTrick"];
        bB = FindObjectOfType<BoostBar>();
        _trickTick = true;
    }

    // Update is called once per frame
    void Update()
    { 
        stickTrickAction = playerInput.actions["StickTrick"];
        Vector2 stickInput= stickTrickAction.ReadValue<Vector2>();

        //adds boost meter to the if the player is tricking
        if (TrickState)
        {
            if (buttonTrickAction.triggered || (Mathf.Abs(stickInput.normalized.magnitude) >= 1 && _trickTick))
            {
                bB.IncreaseBoost(TrickAmount);
                _trickTick = false;
            }

            if (Mathf.Abs(stickInput.normalized.magnitude) < TrickDeadzone)
            {
                _trickTick = true;
            }
        }
    }
}
