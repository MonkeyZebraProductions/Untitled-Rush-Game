using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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
    private RaycastHit2D hitdown, hitright, hitleft, hitup,rotationhit;
    private RaycastHit2D[] GroundCheck;
    private int index;
    private int groundVar;
    private float slopeRotationAngle;
    private float slopeValue;
    private float sideOffset;
    private bool _isGrounded;
    private float groundAngle;

    public float SlopeLimit;
    public float SlopeMultiplier;
    public float SlopeStickiness;
    public float GroundCheckDistance = 0.7f;
    public LayerMask GroundLayer;
    

    //Player Movement Variables
    [Header("Moving")]
    public float BaseMovementSpeed;
    public float CappedMovementSpeed;
    public float Acceleration;
    private float currentSpeed;
    private float currentAcceleration;

    //Player Jump Variables
    [Header("Jumping")]
    public float JumpSpeed=100;
    public float Gravity;
    public float FallAcceleration;
    public float AirAcceleration;
    public int MaxJumps;
    public float JumpGracePeriod;
    [Range(0.0f, 0.99f)]
    public float JumpMultiplierRate, JumpCancelRate,AirFriction;
    private bool _isJumping,_isFalling;
    private float fallSpeed;
    private float jumpMultiplyer;
    private float currentJumpMultiplierRate;
    private float currentAirFriction;
    private float? lastGroundTime;
    private float? jumpButtonPressed;

    private int jumps;

    //Debug Variables
    public bool DebugUI;
    public TextMeshProUGUI CurrentSpeedText;

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
        currentSpeed = 0;
        GroundCheck = new RaycastHit2D[4];
        sideOffset = (capCollider2D.size.y - capCollider2D.size.x)/2;
        jumps = MaxJumps;
    }

    // Update is called once per frame
    void Update()
    {
        //rb2D.velocity = _currentSpeed * transform.right;
        //Applies a ground check on each of the cardinal directions of the player
        hitdown = Physics2D.Raycast(transform.position, -Vector2.up, GroundCheckDistance, GroundLayer);
        Debug.DrawRay(transform.position, -Vector2.up * GroundCheckDistance, Color.red);

        hitleft = Physics2D.Raycast(transform.position, -Vector2.right, GroundCheckDistance, GroundLayer);
        Debug.DrawRay(transform.position, Vector2.left * (GroundCheckDistance), Color.red);

        hitright = Physics2D.Raycast(transform.position, Vector2.right, GroundCheckDistance, GroundLayer);
        Debug.DrawRay(transform.position, Vector2.right * (GroundCheckDistance), Color.red);

        hitup = Physics2D.Raycast(transform.position, Vector2.up, GroundCheckDistance, GroundLayer);
        Debug.DrawRay(transform.position, Vector2.up * GroundCheckDistance, Color.red);

        rotationhit = Physics2D.Raycast(transform.position, -transform.up, GroundCheckDistance, GroundLayer);
        Debug.DrawRay(transform.position, -Vector2.up * GroundCheckDistance, Color.blue);

        GroundCheck[0] = hitdown;
        GroundCheck[1] = hitright;
        GroundCheck[2] = hitup;
        GroundCheck[3] = hitleft;
        SlopeCheck();
    }

   
    private void FixedUpdate()
    {
        if(DebugUI)
        {
            CurrentSpeedText.text = "Current Speed: " + rb2D.velocity;
        }
        

        slopeRotationAngle = transform.rotation.eulerAngles.z;
        if(slopeRotationAngle>180)
        {
            slopeRotationAngle -= 360;
        }
        

        //Gets the movement action value
        moveAction = playerInput.actions["Move"];
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        //Checks movement direction
        if (moveInput.x > 0.01)
        {
            //Accelerates player if slower than base speed
            if (currentSpeed < BaseMovementSpeed)
            {
                currentSpeed += currentAcceleration;
            }
        }
        else if (moveInput.x < -0.01)
        {
            //Accelerates player if slower than base speed in the inverse direction
            if (currentSpeed > -BaseMovementSpeed)
            {
                currentSpeed -= currentAcceleration;
            }
        }
        else
        {
            //Slowly moves player speed to 0 if movement input is 0

            if (currentSpeed > 0 || currentSpeed < 0)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0.0f, currentAcceleration/2);
            }
        }


        //Sets velocity
        currentSpeed -= Mathf.Sin(groundAngle) * SlopeMultiplier;
        //rb2D.velocity = (currentSpeed * transform.right) + (-Vector3.up*2f);
        //Rotates Player if the player is grounded
        if (rotationhit.collider != null)
        {
            currentAcceleration = Acceleration;
            RotatePlayer();
            _isGrounded = true;
            _isFalling = false;
            lastGroundTime = Time.time;
            //jumpMultiplyer = 1f;
            //Adds force to let player stick on walls
            jumps = MaxJumps;
            groundVar = 1;
            fallSpeed = 0;
            rb2D.velocity = (currentSpeed * transform.right) + (-Vector3.up * 2f)+ (transform.up * -Mathf.Abs(currentSpeed) * SlopeStickiness * groundVar);
        }
        else
        {
            currentAcceleration = AirAcceleration;
            _isGrounded = false;
            groundVar = 0;
            //StartCoroutine(CorrectRotation());
            groundAngle = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), 0.1f);
            rb2D.velocity = ((currentSpeed * currentAirFriction) * transform.right) + (-Vector3.up * 2f);
        }

        if(Time.time - lastGroundTime >= JumpGracePeriod)
        {
            jumps = 0;
            _isFalling = true;
        }

        if (Time.time - jumpButtonPressed <= JumpGracePeriod && jumps == MaxJumps)
        {
            _isFalling = false;
            _isJumping = true;
            jumps = 0;
            jumpMultiplyer = 1;
            currentJumpMultiplierRate = JumpMultiplierRate;
            jumpButtonPressed = null;
            lastGroundTime = null;
        }


        if (_isJumping)
        {
            Vector2 tUp = transform.up;
            rb2D.velocity += tUp* JumpSpeed * jumpMultiplyer;

            jumpMultiplyer *= currentJumpMultiplierRate;

            if (jumpMultiplyer <= 0.01f)
            {
                _isJumping = false;
                _isFalling = true;
            }
        }

        //increases gravity when falling
        if(_isFalling)
        {
            fallSpeed += FallAcceleration;
            rb2D.velocity -= Vector2.up * Gravity * fallSpeed * Time.deltaTime;
        }
    }

    //rotates player based on slope
    private void RotatePlayer()
    {
        

        //Debug.Log("Hit collider " + GroundCheck[index].collider + ", at " + GroundCheck[index].point + ", normal " + GroundCheck[index].normal);
        Debug.DrawRay(GroundCheck[index].point, GroundCheck[index].normal * 2f, Color.green);
        Debug.DrawRay(rotationhit.point, rotationhit.normal * 2f, Color.yellow);
        groundAngle = Mathf.Atan2(-rotationhit.normal.x, rotationhit.normal.y);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, Mathf.Rad2Deg * groundAngle), 1f);
        //transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(-GroundCheck[index].normal.x,GroundCheck[index].normal.y));
        //storedRotation = new Vector2(GroundCheck[index].normal.x * 1.5f, GroundCheck[index].normal.y / 1.5f).normalized;
        //_isFalling = false;
    }


    //Checks the height of slopes to see if they can be ran up
    private void SlopeCheck()
    {
        
        if (hitdown && index != 0)
        {
            index = 0;
            groundAngle = transform.rotation.z;
        }

        if (hitright && index != 1)
        {
            if (Mathf.Abs(Mathf.Rad2Deg * Mathf.Atan2(-hitright.normal.x, hitright.normal.y) - slopeRotationAngle) < SlopeLimit)
            {
                index = 1;
                groundAngle = transform.rotation.z;
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
                index = 2;
                groundAngle = transform.rotation.z;
            }

        }

        if (hitleft && index != 3)
        {

            if (Mathf.Abs(Mathf.Rad2Deg * Mathf.Atan2(-hitleft.normal.x, hitleft.normal.y) - slopeRotationAngle) < SlopeLimit)
            {
                index = 3;
                groundAngle = transform.rotation.z;
            }

        }
    }

    //Sets mode to Jumping when Jump button is pressed
    private void JumpVoid()
    {
        jumpButtonPressed = Time.time;
        Debug.Log("Pong");
        currentAirFriction = AirFriction;
        //if(jumps==MaxJumps)
        //{
        //    Debug.Log("Ping");
        //    _isJumping = true;
        //    jumpMultiplyer = 1;
        //    currentJumpMultiplierRate = JumpMultiplierRate;
        //}

    }

    //Reduces Jump Height when Jump Button is let go
    private void JumpCancel()
    {
        currentJumpMultiplierRate = JumpCancelRate;
        currentAirFriction = 1;
    }

    private void OnEnable()
    {
        jumpAction.started += context => JumpVoid();
        jumpAction.canceled += context => JumpCancel();
    }

    private void OnDisable()
    {
        jumpAction.started -= context => JumpVoid();
        jumpAction.canceled -= context => JumpCancel();
    }
}
