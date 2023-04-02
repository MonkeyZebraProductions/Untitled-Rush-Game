using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    //Control Variables
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    //Component Variables
    private Rigidbody2D rb2D;
    private CapsuleCollider2D capCollider2D;

    //Ground and Slope Checks
    [Header("Ground and Slope Checks")]
    private RaycastHit2D hitdown, hitright, hitleft, hitup;
    private RaycastHit2D[] GroundCheck;
    private int index;
    private float slopeRotationAngle;
    private float slopeValue;
    private float sideOffset;
    private bool _isGrounded;
    private float groundAngle;

    public float SlopeLimit;
    public float SlopeMultiplier;
    public float GroundCheckDistance = 0.7f;
    public LayerMask GroundLayer;
    

    //Player Movement Variables
    [Header("Moving")]
    public float BaseMovementSpeed;
    public float CappedMovementSpeed;
    public float Acceleration;
    private float _currentSpeed;

    //Player Jump Variables
    [Header("Jumping")]
    public float Gravity;


    private void Awake()
    {
        //Asigns variables at start of Runtime
        playerInput = GetComponent<PlayerInput>();
        jumpAction = playerInput.actions["Jump"];

        rb2D = GetComponent<Rigidbody2D>();
        capCollider2D = GetComponent<CapsuleCollider2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        _currentSpeed = 0;
        GroundCheck = new RaycastHit2D[4];
        sideOffset = (capCollider2D.size.y - capCollider2D.size.x)/2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {

        //Gets the movement action value
        moveAction = playerInput.actions["Move"];
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        //Checks movement direction
        if (moveInput.x > 0.01)
        {
            //Accelerates player if slower than base speed
            if (_currentSpeed < BaseMovementSpeed)
            {
                _currentSpeed += Acceleration;
            }
        }
        else if (moveInput.x < -0.01)
        {
            //Accelerates player if slower than base speed in the inverse direction
            if (_currentSpeed > -BaseMovementSpeed)
            {
                _currentSpeed -= Acceleration;
            }
        }
        else
        {
            //Slowly moves player speed to 0 if movement input is 0

            if(_currentSpeed>0 || _currentSpeed<0)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0.0f, Acceleration);
            }
        }

        Debug.Log(-Mathf.Sin(Mathf.Abs(groundAngle)));
        //Sets velocity
        rb2D.velocity = (_currentSpeed * transform.right*Mathf.Cos(groundAngle))  
            + (transform.up * -Mathf.Sin(Mathf.Abs(groundAngle)));

        //Applies a ground check on each of the cardinal directions of the player
        hitdown = Physics2D.Raycast(transform.position, -Vector2.up, GroundCheckDistance, GroundLayer);
        Debug.DrawRay(transform.position, -Vector2.up * GroundCheckDistance, Color.red);

        hitleft = Physics2D.Raycast(transform.position, -Vector2.right, GroundCheckDistance, GroundLayer);
        Debug.DrawRay(transform.position, Vector2.left * (GroundCheckDistance), Color.red);

        hitright = Physics2D.Raycast(transform.position , Vector2.right, GroundCheckDistance, GroundLayer);
        Debug.DrawRay(transform.position, Vector2.right * (GroundCheckDistance), Color.red);

        hitup = Physics2D.Raycast(transform.position, Vector2.up, GroundCheckDistance, GroundLayer);
        Debug.DrawRay(transform.position, Vector2.up * GroundCheckDistance, Color.red);

        GroundCheck[0] = hitdown;
        GroundCheck[1] = hitright;
        GroundCheck[2] = hitup;
        GroundCheck[3] = hitleft;

        //Rotates Player if the player is grounded
        if (GroundCheck[index].collider != null)
        {
            RotatePlayer();
            //Adds force to let player stick on walls
            //rb2D.AddForce(-transform.up * Gravity * Time.deltaTime);
        }
        else
        {
            _isGrounded = false;
            //StartCoroutine(CorrectRotation());
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), 0.01f);
            
        }
        SlopeCheck();

        slopeRotationAngle = transform.rotation.eulerAngles.z;
        if(slopeRotationAngle>180)
        {
            slopeRotationAngle -= 360;
        }
        //rb2D.AddForce(-Vector2.up * Gravity * Time.deltaTime);
    }

    //rotates player based on slope
    private void RotatePlayer()
    {
        _isGrounded = true;
        //_jumpMultiplyer = _airMultiplier = 1f;

        //Debug.Log("Hit collider " + GroundCheck[index].collider + ", at " + GroundCheck[index].point + ", normal " + GroundCheck[index].normal);
        Debug.DrawRay(GroundCheck[index].point, GroundCheck[index].normal * 2f, Color.green);
        groundAngle = Mathf.Atan2(-GroundCheck[index].normal.x, GroundCheck[index].normal.y);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, Mathf.Rad2Deg * groundAngle), 0.2f);
        //transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(-GroundCheck[index].normal.x,GroundCheck[index].normal.y));
        //storedRotation = new Vector2(GroundCheck[index].normal.x * 1.5f, GroundCheck[index].normal.y / 1.5f).normalized;
        //_isFalling = false;
    }


    //Checks the height of slopes to see if they can be ran up
    private void SlopeCheck()
    {
        //switch (index)
        //{
        //    case 0:
        //        if (Mathf.Abs(Mathf.Rad2Deg * Mathf.Atan2(-hitright.normal.x, hitright.normal.y) - transform.rotation.eulerAngles.z) < SlopeLimit)
        //        {
        //            Debug.Log("Hi");
        //            index = 1;
        //        }
        //        else if (hitleft)
        //        {
        //            Debug.Log("Hi");
        //            index = 3;
        //        }
        //        break;
        //    case 1:
        //        if (hitdown)
        //        {
        //            index = 0;
        //        }
        //        break;
        //    case 3:
        //        if (hitdown)
        //        {
        //            index = 0;
        //        }
        //        break;
        //}
        if (hitdown && index != 0)
        {
            index = 0;
        }

        if (hitright && index != 1)
        {
            if (Mathf.Abs(Mathf.Rad2Deg * Mathf.Atan2(-hitright.normal.x, hitright.normal.y) - slopeRotationAngle) < SlopeLimit)
            {
                Debug.Log("Hi");
                index = 1;
            }
            else
            {
                //if (!_StopOnWall)
                //{
                //    _currentspeed = 0;
                //    _StopOnWall = true;
                //}

            }
        }

        if (hitup && index != 2)
        {

            if (Mathf.Abs(Mathf.Rad2Deg * Mathf.Atan2(-hitup.normal.x, hitup.normal.y) - slopeRotationAngle) < SlopeLimit)
            {
                Debug.Log("Hi");
                index = 2;
            }

        }

        if (hitleft && index != 3)
        {

            if (Mathf.Abs(Mathf.Rad2Deg * Mathf.Atan2(-hitleft.normal.x, hitleft.normal.y) - slopeRotationAngle) < SlopeLimit)
            {
                Debug.Log("Hi");
                index = 3;
            }

        }
    }
}
