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
    private InputAction slideAction;
    private InputAction boostAction;
    private InputAction breakAction;

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
    public float BreakPower;

    private float currentSpeed;
    private float preBoostSpeed;
    private float currentAcceleration;
    public bool IsFacingLeft;
    private bool _isBreaking;

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
    //private bool _jumpPressed;
    private float fallSpeed;
    private float jumpMultiplyer;
    private float currentJumpMultiplierRate;
    private float currentAirFriction;
    private float? lastGroundTime;
    private float? jumpButtonPressed;
    private int jumps;

    [Header("HomingAttack")]
    public Vector2 HomingDashDirection;
    public float HomingAttackSpeed;
    public bool CanHomingAttack;

    private bool _isHoming,_homingEnded;
    private Vector2 currentHomingDirection;
    private HomingAttack hA;

    //Player Slide Variables
    [Header("Sliding")]
    public float SlideSpeed;
    public float SlideMomentumMultiplier;
    public float StompSpeed;

    private bool _isSliding;
    private bool _isStomping;
    private float currentMomentumMultiplier=1;

    //Player Boost Variables
    [Header("Boosting")]
    public float BoostSpeed;
    public float Mach2BoostSpeed;
    public float Mach3BoostSpeed;
    public float AirBoostForce;
    public Vector2 AirBoostDirection;
    public int AirBoostMax;

    private int airBoostNumber;
    private int boostDir;
    private bool _isBoosting;

    //Debug Variables
    public bool DebugUI;
    public TextMeshProUGUI CurrentSpeedText;

    private void Awake()
    {
        //Asigns variables at start of Runtime
        playerInput = GetComponent<PlayerInput>();
        jumpAction = playerInput.actions["Jump"];
        slideAction = playerInput.actions["slide"];
        boostAction = playerInput.actions["Boost"];
        breakAction = playerInput.actions["Break"];
        rb2D = GetComponent<Rigidbody2D>();
        capCollider2D = GetComponent<CapsuleCollider2D>();
        hA = FindObjectOfType<HomingAttack>();
    }
    // Start is called before the first frame update
    void Start()
    {
        currentSpeed = 0;
        GroundCheck = new RaycastHit2D[4];
        sideOffset = (capCollider2D.size.y - capCollider2D.size.x)/2;
        jumps = MaxJumps;
        airBoostNumber = AirBoostMax;
        boostDir = 1;
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
        if (DebugUI)
        {
            CurrentSpeedText.text = "Current Speed: " + rb2D.velocity;
        }


        slopeRotationAngle = transform.rotation.eulerAngles.z;
        if (slopeRotationAngle > 180)
        {
            slopeRotationAngle -= 360;
        }


        //Gets the movement action value
        moveAction = playerInput.actions["Move"];
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        if (!_isBreaking)
        {
            //Checks movement direction
            if (moveInput.x > 0.01)
            {
                boostDir = 1;
                ////Sets player speed to slide speed if sliding and moving slowly
                //if (_isSliding && currentSpeed < SlideSpeed)
                //{
                //    currentSpeed = SlideSpeed;
                //}

                //Accelerates player if slower than base speed
                if (currentSpeed < BaseMovementSpeed)
                {
                    currentSpeed += currentAcceleration;
                }
            }
            else if (moveInput.x < -0.01)
            {
                boostDir = -1;
                ////Sets player speed to slide speed if sliding
                //if (_isSliding && currentSpeed > -SlideSpeed)
                //{
                //    currentSpeed = -SlideSpeed;
                //}
                //Accelerates player if slower than base speed in the inverse direction
                if (currentSpeed > -BaseMovementSpeed)
                {
                    currentSpeed -= currentAcceleration;
                }
            }
            else
            {
                //Slowly moves player speed to 0 if movement input is 0 at a slower rate

                if (currentSpeed > 0 || currentSpeed < 0)
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, 0.0f, currentAcceleration / 2);
                }
            }
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0.0f, BreakPower);
        }

        if(currentSpeed>0)
        {
            IsFacingLeft = false;
        }
        else if (currentSpeed<0)
        {
            IsFacingLeft = true;
        }
        //Sets velocity
        if(_isBoosting)
        {
            if(!IsFacingLeft)
            {
                // Speed of the boost depends of the Player speed before the boost button is pressed
                if (preBoostSpeed > Mach2BoostSpeed * 0.9f)
                {
                    currentSpeed = Mach3BoostSpeed;
                }
                else if (preBoostSpeed > BoostSpeed * 0.9f)
                {
                    currentSpeed = Mach2BoostSpeed;
                }
                else
                {
                    currentSpeed = BoostSpeed;
                }
            }
            else
            {
                if (preBoostSpeed < -Mach2BoostSpeed * 0.9f)
                {
                    currentSpeed = -Mach3BoostSpeed;
                }
                else if (preBoostSpeed < -BoostSpeed * 0.9f)
                {
                    currentSpeed = -Mach2BoostSpeed;
                }
                else
                {
                    currentSpeed = -BoostSpeed;
                }
            }
            
        }
        else //Use Slope Physics if not Boosting
        {
            currentSpeed -= Mathf.Sin(groundAngle) * SlopeMultiplier * currentMomentumMultiplier;
            preBoostSpeed = currentSpeed;
        }
            
        
        
        //rb2D.velocity = (currentSpeed * transform.right) + (-Vector3.up*2f);
        //Checks if Player is grounded
        if (rotationhit.collider != null)
        {
            if(_isStomping)
            {
                StompLand();
            }
            currentAcceleration = Acceleration;
            //Rotates Player if the player is grounded
            RotatePlayer();
            _isGrounded = true;
            _isFalling = false;
            lastGroundTime = Time.time;
            //jumpMultiplyer = 1f;
            //Adds force to let player stick on walls
            jumps = MaxJumps;
            groundVar = 1;
            fallSpeed = 0;
            _isHoming = false;
            airBoostNumber = AirBoostMax;
            if (slideAction.IsPressed())
            {
                _isSliding = true;
                currentMomentumMultiplier = SlideMomentumMultiplier;
                Debug.Log("Sliding");
            }
            else
            {
                _isSliding = false;
                currentMomentumMultiplier = 1;
            }
            if(boostAction.IsPressed())
            {
                _isBoosting = true;
            }
            if(breakAction.IsPressed())
            {
                _isBreaking = true;
            }
            else
            {
                _isBreaking = false;
            }
            rb2D.velocity = (currentSpeed * transform.right) + (-Vector3.up * 2f) + (transform.up * -Mathf.Abs(currentSpeed) * SlopeStickiness * groundVar);
        }
        else
        {
            if (!_isStomping)
            {
                currentAcceleration = AirAcceleration;
            }

            if(boostAction.IsPressed() && airBoostNumber==AirBoostMax && _isBoosting==false)
            {
                Debug.Log("Boost");
                AirBoost();
            }
            if (slideAction.IsPressed())
            {
                _isStomping = true;
                ResetMomentum();
            }
            _isGrounded = false;
            groundVar = 0;
            //StartCoroutine(CorrectRotation());
            groundAngle = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), 0.1f);
            if(jumpAction.IsPressed() && CanHomingAttack)
            { 
                hA.CheckHoming();
            }
            rb2D.velocity = ((currentSpeed * currentAirFriction) * transform.right) + (-Vector3.up * 2f);
        }

        if (Time.time - lastGroundTime >= JumpGracePeriod)
        {
            jumps = 0;
            _isFalling = true;
        }

        if (Time.time - jumpButtonPressed <= JumpGracePeriod && jumps == MaxJumps)
        {
            Debug.Log("Arrg");
            _isFalling = false;
            _isJumping = true;
            jumps = 0;
            jumpMultiplyer = 1;
            currentJumpMultiplierRate = JumpMultiplierRate;
            jumpButtonPressed = null;
            lastGroundTime = null;
        }

        if(_isHoming)
        {
            rb2D.AddForce(currentHomingDirection.normalized * (HomingAttackSpeed*(1+Mathf.Abs(rb2D.velocity.x/8)))*hA.CurrentHomingMultiplier * Time.deltaTime);
        }
        //Jump Code
        if (_isJumping)
        {
            Vector2 tUp = transform.up;
            rb2D.velocity += tUp * JumpSpeed * jumpMultiplyer;

            jumpMultiplyer *= currentJumpMultiplierRate;

            if (jumpMultiplyer <= 0.01f)
            {
                _isJumping = false;
                _isFalling = true;
            }
        }
        //increases gravity when falling
        if (_isFalling)
        {
            fallSpeed += FallAcceleration;
            rb2D.velocity -= Vector2.up * Gravity * fallSpeed * Time.deltaTime;
        }

        //Stomp Code

        if(_isStomping)
        {
            Stomp();
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

    public void ResetMomentum()
    {
        rb2D.velocity = Vector2.zero;
        currentSpeed = 0;
        groundAngle = 0;
        transform.rotation = Quaternion.identity;
    }

    //Sets mode to Jumping when Jump button is pressed
    private void JumpVoid()
    {
       
        jumpButtonPressed = Time.time;
        
        
        currentAirFriction = AirFriction;
        if (jumps<MaxJumps)
        {
            CanHomingAttack = true;
        }
        else
        {
            CanHomingAttack = false;
        }
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
        CanHomingAttack=true;
      
        currentJumpMultiplierRate = JumpCancelRate;
        currentAirFriction = 1;

        
    }

    public IEnumerator HomingDash()
    {
        ResetMomentum();
        currentHomingDirection = new Vector2 (HomingDashDirection.x*boostDir,HomingDashDirection.y);
        jumpButtonPressed = null;
        _isJumping = false;
        _isFalling = true;
        _isHoming = true;
        currentSpeed = BaseMovementSpeed*boostDir;
        yield return new WaitForSeconds(2);
        _isHoming = false;
    }

    public void HomingAttack()
    {
        ResetMomentum();
        currentHomingDirection = hA.CurrentTarget;
        jumpButtonPressed = null;
        _isJumping = false;
        _isFalling = true;
        _isHoming = true;
        currentSpeed = BaseMovementSpeed * hA.Facing;
    }

    private void Slide()
    {
        //code to change hitbox on this line, normalHitbox.enabled=!_isSliding
        //code to change hitbox on this line, slidingHitbox.enabled=_isSliding
        //code to change hurtbox on this line, slidingHurtbox.enabled= _isSliding
    }

    //Moves Player down at a rapid pace with a hitbox
    private void Stomp()
    {
        //code to change hurtbox on this line, stompingHitbox.enabled= true
        currentAcceleration = 0;
        _isJumping = false;
        _isFalling = false;
        rb2D.velocity -= Vector2.up * StompSpeed * Time.deltaTime;
    }

    private void StompLand()
    {
        //code to change hurtbox on this line, stompingHitbox.enabled= falce
        //code to change hurtbox on this line, LandingHitbox.enabled= true
        //Wait for frames
        //code to change hurtbox on this line, LandingHitbox.enabled= falce
        _isStomping = false;
    }

    private void BoostCancel()
    {
        if(_isBoosting)
        {
            if(currentSpeed>BaseMovementSpeed && !IsFacingLeft)
            {
                currentSpeed = BaseMovementSpeed;
            }
            else if(currentSpeed < -BaseMovementSpeed && IsFacingLeft)
            {
                currentSpeed = -BaseMovementSpeed;
            }
            
            _isBoosting = false;
        }
        
    }
    private void AirBoost()
    {
        ResetMomentum();
        rb2D.AddForce(new Vector2(AirBoostDirection.x*boostDir,AirBoostDirection.y).normalized * AirBoostForce);
        currentSpeed = BoostSpeed*boostDir;
        airBoostNumber -= 1;
    }

    private void OnEnable()
    {
        jumpAction.started += context => JumpVoid();
        jumpAction.canceled += context => JumpCancel();
        boostAction.canceled += context => BoostCancel();
    }

    private void OnDisable()
    {
        jumpAction.started -= context => JumpVoid();
        jumpAction.canceled -= context => JumpCancel();
        boostAction.canceled -= context => BoostCancel();
    }
}
