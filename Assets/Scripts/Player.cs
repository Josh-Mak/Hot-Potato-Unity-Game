using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Player : MonoBehaviour
{

    public Rigidbody2D Rigidbody;
    public CircleCollider2D Collider;
    public CircleCollider2D ballGrabCollider;
    public GameObject ball;
    public TextMeshProUGUI shotChargeUI;
    public TextMeshProUGUI levelTimerUI;
    public LayerMask Terrain;
    public GameObject StartLine;
    public GameObject ballPointer;

    public Vector2 startingPosition = new Vector2(-15.5f, -8.4f);

    private Vector3 mousePosition;

    #region Horizontal Movement Variables
    public float moveSpeed = 6;
    public float kickStartStrength = 2;
    public float kickStartThreshold = 1.5f;

    public float driftStrength = 3f;

    public Vector2 horizontalDirection { get; private set; }
    private string pressedFirst = "";
    private bool leftKeyPressed = false;
    private bool rightKeyPressed = false;
    #endregion

    #region Jumping Variables
    private bool isGrounded = false;
    private bool isWalled = false;

    public float shortJumpStrength = 4f;
    public float fullJumpStrength = 4f;
    private float fullJumpTimeHold = 4f;

    public float doubleJumpStrength = 5;
    public float doubleJumpHorizontalStrength = 4f;

    public float wallJumpStrengthH = 3f;
    public float wallJumpStrengthV = 6f;

    private bool shortJumping = false;
    private bool currentlyGroundedJumping = false;
    private bool holdingJump = false;
    private bool hasFullJumped = false;
    private int fullJumpTimer = 0;
    private bool doubleJumping = false;
    private bool hasDoubleJumped = false;

    private string lastTouchedWallDirection;
    private Vector2 wallJumpDirection;
    private bool wallJumping = false;
    #endregion

    #region Up/Down Variables
    public float fastFallSpeed = 30f;
    public float floatStrength = 2f;

    private bool fastFalling = false;
    private bool Floating = false;
    #endregion

    #region DASHING Variables
    public float dashSpeed = 3f;
    public float dashCooldown = 60f;
    public float dashLength = 6f;
    public float dashRefractoryPeriod = 15f;
    public float dashShotStrength = 50f;

    private bool groundDashing = false;
    private bool airDashing = false;
    private bool activelyGroundDashing = false;
    private bool activelyAirDashing = false;
    private bool dashOnCooldown = false;
    private bool justWaveDashed = false;


    private int dashLengthTimer = 0;
    private int dashCooldownTimer = 0;

    private Vector2 dashDirectionH = Vector2.zero;
    private Vector2 dashDirectionV = Vector2.zero;
    #endregion

    #region BALL CATCH SYSTEM VARIABLES
    public int shotChargeSpeed = 2;  // higher numbers = slower charge. Adds 1 shotStrength every shotChargeSpeed milliseconds.
    private float ballShotStrengthModifier = 0.25f;  // how much shot speed per charge the shot will come out at.
    private float maxBallShotCharge = 100f;
    private int currentBallShotCharge = 0;
    private int ballChargeCounter = 0;

    private bool mouseHeldDown = false;
    //private bool ballInCatchArea = false;
    private bool ballCurrentlyHeld = false;
    private bool canCatch = false;
    #endregion

    private Vector2 testv2;

    #region Helper Functions
    bool IsTouchingTerrain()
    {
        return Physics2D.OverlapCircle(Rigidbody.position, Collider.radius, Terrain);
    }

    string TouchingTerrain()
    {
        Collider2D hit = Physics2D.OverlapCircle(Rigidbody.position, Collider.radius, Terrain);
        if (hit != null)
        {
            Vector2 pointOfContact = hit.ClosestPoint(Rigidbody.position);

            if (Math.Abs(Rigidbody.position.x - pointOfContact.x) < 0.02f)  // if it's a floor 
            {
                return "Floor";
            }
            else if (Math.Abs(Rigidbody.position.y - pointOfContact.y) < 0.02f)  // if it's a wall.
            {
                return "Wall";
            }
            else
            {
                return "None";
            }
        }
        else { return "None"; }
    }

    string getCurrentHorizontalMovementDirection()
    {
        if (Rigidbody.velocity.x > 0)
        {
            return "Right";
        }
        else if (Rigidbody.velocity.x < 0)
        {
            return "Left";
        }
        else
        {
            return "Stopped";
        }
    }

    void SetHeldBallPosition()
    {
        Vector2 playerPos = new Vector2(this.transform.position.x, this.transform.position.y);
        Vector2 mousePos = new Vector2(mousePosition[0], mousePosition[1]);
        Vector2 direction = (mousePos - playerPos).normalized;
        Vector2 offset = direction * (Collider.radius + 0.4f);
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        ballRb.isKinematic = true;
        Vector2 newBallPos = playerPos + offset;
        //ball.transform.position = newBallPos;

        // if the ball collides with anything, don't change it's position.
        CircleCollider2D ballCollider = ball.GetComponent<CircleCollider2D>();
        Collider2D hitCollider = Physics2D.OverlapCircle(newBallPos, 0.25f, Terrain); 
        if (hitCollider == null)
        {
            ball.transform.position = newBallPos;
        }
        else { }
    }

    void SetBallPointerPosition()
    {
        Vector3 playerPos = this.transform.position;
        Vector3 ballPos = ball.transform.position;
        Vector3 direction = (ballPos - playerPos).normalized;
        ballPointer.transform.position = playerPos + direction;

        float angle = Mathf.Atan2(direction.y, direction.x);  // gets the angle in radians
        angle = angle * Mathf.Rad2Deg;  // converts to degrees
        angle -= 90;  // adjust b/c Z rotattion of 0 points up. 
        ballPointer.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void ReleaseBall()
    {
        Vector2 playerPos = new Vector2(this.transform.position.x, this.transform.position.y);
        Vector2 mousePos = new Vector2(mousePosition[0], mousePosition[1]);
        Vector2 direction = (mousePos - playerPos).normalized;
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        ballRb.isKinematic = false;
        ballRb.velocity = new Vector2(direction.x * (currentBallShotCharge * ballShotStrengthModifier), direction.y * (currentBallShotCharge * ballShotStrengthModifier));

        ballChargeCounter = 0;
        currentBallShotCharge = 0;
        shotChargeUI.text = "";
        ballCurrentlyHeld = false;
        ballPointer.SetActive(true);
    }

    public void ResetPlayer()
    {
        this.transform.position = startingPosition;  // whatever are our starting coordinates for the current level.
        this.Rigidbody.velocity = Vector2.zero;
        canCatch = false;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        ballPointer.SetActive(true);
        canCatch = false;
    }

    // Update is called once per frame
    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (justWaveDashed || (dashCooldownTimer == 0) || (dashCooldownTimer > (dashLength + dashRefractoryPeriod)))  // if we are not currently dashing
        {
            #region LEFT/RIGHT MOVEMENT
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  // left
            {
                leftKeyPressed = true;
                dashDirectionH = Vector2.left;
            }
            else
            {
                leftKeyPressed = false;
                if (pressedFirst == "Left")
                {
                    pressedFirst = "";
                }
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))  // right
            {
                rightKeyPressed = true;
                dashDirectionH = Vector2.right;
            }
            else
            {
                rightKeyPressed = false;
                if (pressedFirst == "Right")
                {
                    pressedFirst = "";
                }
            }

            if (!leftKeyPressed && !rightKeyPressed)
            {
                horizontalDirection = Vector2.zero;
                dashDirectionH = Vector2.zero;
            }
            else if (leftKeyPressed && !rightKeyPressed)
            {
                horizontalDirection = Vector2.left;
                if (pressedFirst == "")
                {
                    pressedFirst = "Left";
                }
            }
            else if (rightKeyPressed && !leftKeyPressed)
            {
                horizontalDirection = Vector2.right;
                if (pressedFirst == "")
                {
                    pressedFirst = "Right";
                }
            }
            else  // both are pressed
            {
                dashDirectionH = Vector2.zero;
                if (pressedFirst == "Left")
                {
                    horizontalDirection = Vector2.right;
                }
                else if (pressedFirst == "Right")
                {
                    horizontalDirection = Vector2.left;
                }
            }
            #endregion

            #region UP/DOWN MOVEMENT
            if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))  // down
            {
                fastFalling = true;
                dashDirectionV = Vector2.down;
            }
            else
            {
                fastFalling = false;
            }

            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && !isGrounded)  // UP
            {
                Floating = true;
            }
            else
            {
                Floating = false;
            }
            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)))
            {
                dashDirectionV = Vector2.up;
            }

            if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && !(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))
            {
                dashDirectionV = Vector2.zero;
            }
            #endregion

            #region JUMPING
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)  // grounded jump
            {
                shortJumping = true;
                currentlyGroundedJumping = true;
                hasFullJumped = false;

                if (hasDoubleJumped)
                {
                    hasDoubleJumped = false;
                }
            }

            if (Input.GetKey(KeyCode.Space))  // holding jump
            {
                holdingJump = true;
            }
            else
            {
                holdingJump = false;
                fullJumpTimer = 0;
                currentlyGroundedJumping = false;
                hasFullJumped = true;
            }

            if (Input.GetKeyDown(KeyCode.Space) && !hasDoubleJumped && !isGrounded && !isWalled && !currentlyGroundedJumping)  // double jump
            {
                doubleJumping = true;
            }

            if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && isWalled)  // wall jump
            {
                if (lastTouchedWallDirection == "Right" && ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))))
                {
                    wallJumping = true;
                    wallJumpDirection = Vector2.left;
                }
                else if (lastTouchedWallDirection == "Left" && ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))))
                {
                    wallJumping = true;
                    wallJumpDirection = Vector2.right;
                }
            }
            #endregion

            #region DASHING
            if (Input.GetKey(KeyCode.LeftShift) && !dashOnCooldown && isGrounded)
            {
                groundDashing = true;
            }
            else { groundDashing = false; }
            if (Input.GetKey(KeyCode.LeftShift) && !dashOnCooldown && !isGrounded)
            {
                airDashing = true;
            }
            else { airDashing = false; }
            #endregion

            #region CATCHING/THROWING
            if (canCatch)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    BallCatchCollider catchScript = ballGrabCollider.GetComponent<BallCatchCollider>();
                    if (catchScript != null && catchScript.ballInCatchArea)
                    {
                        ballCurrentlyHeld = true;
                        ballPointer.SetActive(false);
                    }
                }
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    mouseHeldDown = true;
                }
                else
                {
                    mouseHeldDown = false;
                    if (ballCurrentlyHeld)
                    {
                        ballCurrentlyHeld = false;
                        ReleaseBall();
                    }
                }
            }
            #endregion
        }

        if (!ballCurrentlyHeld)
        {
            SetBallPointerPosition();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log($"IsGrounded: {isGrounded}. IsWalled: {isWalled}");
        }
    }

    // called at a fixed interval (replaces everything being times by time.deltaTime i think?)
    private void FixedUpdate()
    {
        #region LEFT/RIGHT MOVEMENT
        if ((horizontalDirection != Vector2.zero) && !isGrounded)  // aerial drifting
        {
            Rigidbody.AddForce(horizontalDirection * driftStrength);
        }
        else if (horizontalDirection != Vector2.zero)  // grounded
        {
            if (Rigidbody.velocity.magnitude < kickStartThreshold)
            {
                if ((getCurrentHorizontalMovementDirection() == "Left" && horizontalDirection == Vector2.left) || (getCurrentHorizontalMovementDirection() == "Right" && horizontalDirection == Vector2.right) || (getCurrentHorizontalMovementDirection() == "Stopped"))
                {
                    Rigidbody.AddForce(horizontalDirection * kickStartStrength, ForceMode2D.Impulse);
                }
            }
            else
            {
                Rigidbody.AddForce(horizontalDirection * moveSpeed);
            }
        }
        #endregion

        #region UP/DOWN MOVEMENT
        if (fastFalling)
        {
            Rigidbody.AddForce(Vector2.down * fastFallSpeed);
        }

        if (Floating && (Rigidbody.velocity.y < 0))
        {
            Rigidbody.AddForce(Vector2.up * floatStrength);
        }
        #endregion

        #region JUMPING
        if (shortJumping)
        {
            //Debug.Log("short jumped");
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, 0);
            Rigidbody.AddForce(Vector2.up * shortJumpStrength, ForceMode2D.Impulse);
            shortJumping = false;
            //doubleJumpNBufferTimerActive = true;
            //wallJumpNBufferTimerActive = true;
        }
        if ((fullJumpTimer >= fullJumpTimeHold) && !hasFullJumped)
        {
            //Debug.Log("full jumped");
            Rigidbody.AddForce(Vector2.up * fullJumpStrength, ForceMode2D.Impulse);
            hasFullJumped = true;
        }
        if (doubleJumping)
        {
            //Debug.Log("double jumped");
            if (horizontalDirection.x > 0)
            {
                if (Rigidbody.velocity.x > doubleJumpHorizontalStrength)
                {
                    Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, doubleJumpStrength);
                }
                else
                {
                    Rigidbody.velocity = new Vector2(doubleJumpHorizontalStrength, doubleJumpStrength);
                }
            }
            else if (horizontalDirection.x < 0)
            {
                if (Rigidbody.velocity.x < -doubleJumpHorizontalStrength)
                {
                    Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, doubleJumpStrength);
                }
                else
                {
                    Rigidbody.velocity = new Vector2(-doubleJumpHorizontalStrength, doubleJumpStrength);
                }
            }
            else
            {
                Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, doubleJumpStrength);
            }
            hasDoubleJumped = true;
            doubleJumping = false;
        }
        if (wallJumping)
        {
            //Debug.Log("wall jumped");
            Rigidbody.velocity = Vector2.zero;  // stop momentum so that the walljump doesn't change depending on current speed.
            Rigidbody.AddForce(Vector2.up * wallJumpStrengthV, ForceMode2D.Impulse);
            Rigidbody.AddForce(wallJumpDirection * wallJumpStrengthH, ForceMode2D.Impulse);
            wallJumping = false;
            wallJumpDirection = Vector2.zero;
        }
        #endregion

        #region Dashing
        if (groundDashing)
        {
            groundDashing = false;
            activelyGroundDashing = true;
            dashOnCooldown = true;
            Vector2 dashDirection = new Vector2(dashDirectionH[0], dashDirectionV[1]);
            dashDirection.Normalize();
            Rigidbody.velocity = dashDirection * dashSpeed;
        }
        if (airDashing)
        {
            airDashing = false;
            activelyAirDashing = true;
            dashOnCooldown = true;
            Vector2 dashDirection = new Vector2(dashDirectionH[0], dashDirectionV[1]);
            dashDirection.Normalize();
            Rigidbody.velocity = dashDirection * dashSpeed;
        }
        if (dashLengthTimer >= dashLength)
        {
            Rigidbody.velocity = Vector2.zero;
            activelyGroundDashing = false;
            activelyAirDashing = false;
            dashLengthTimer = 0;
        }
        if (dashCooldownTimer >= dashCooldown)
        {
            dashOnCooldown = false;
            dashCooldownTimer = 0;
            justWaveDashed = false;
        }
        #endregion

        #region TIMERS
        if (holdingJump)
        {
            fullJumpTimer += 1;
        }

        if (activelyGroundDashing)
        {
            dashLengthTimer += 1;
        }
        if (activelyAirDashing)
        {
            dashLengthTimer += 1;
        }
        if (dashOnCooldown)
        {
            dashCooldownTimer += 1;
        }
        #endregion

        #region BALL HANDLING
        if (ballCurrentlyHeld)
        {
            SetHeldBallPosition();
            ballChargeCounter += 1;
            if (ballChargeCounter % shotChargeSpeed == 0)
            {
                currentBallShotCharge += 1;
                shotChargeUI.text = currentBallShotCharge.ToString();
                shotChargeUI.color = new Color(1f, (1f - currentBallShotCharge/100f), (1f - currentBallShotCharge/100f), 1);
                {
                    if (currentBallShotCharge > maxBallShotCharge)
                    {
                        ReleaseBall();
                    }
                }
            }
        }
        #endregion
    }

    #region COLLISIONS WITH PLAYER
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & Terrain) != 0)  // a bitwise comparison, complicated >:(
        {
            Vector2 normal = collision.contacts[0].normal;
            float angle = Vector2.Angle(normal, Vector2.up);

            if (angle < 45)
            {
                isGrounded = true;

                hasDoubleJumped = false;
                fullJumpTimer = 0;

                if (activelyAirDashing && (dashDirectionV == Vector2.down))
                {
                    activelyAirDashing = false;
                    dashLengthTimer = 0;
                    justWaveDashed = true;
                }
            }
            else
            {
                isWalled = true;

                hasFullJumped = true;
                if (Rigidbody.position.x - collision.transform.position.x < 0)
                {
                    lastTouchedWallDirection = "Right";
                }
                else if (Rigidbody.position.x - collision.transform.position.x > 0)
                {
                    lastTouchedWallDirection = "Left";
                }
                else
                {
                    Debug.Log("Hit a wall not at left or right?");
                }
            }
        }

        if (collision.gameObject == ball)
        {
            if (activelyAirDashing || activelyGroundDashing)
            {
                Rigidbody2D ballRB = ball.GetComponent<Rigidbody2D>();
                Vector2 dashShotDirection = Rigidbody.velocity.normalized;
                Vector2 dashShotVelocity = dashShotDirection * dashShotStrength;
                ballRB.velocity = dashShotVelocity;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & Terrain) != 0)
        {
            if (isWalled && isGrounded && IsTouchingTerrain())
            {
                string terrainType = TouchingTerrain();
                Debug.Log($"Touching: {terrainType}");
                if (terrainType == "Floor")
                    {
                        isWalled = false;
                        isGrounded = true;
                    }
                else if (terrainType == "Wall")
                    {
                        isWalled = true;
                        isGrounded = false;
                    }
                else
                    {
                        isGrounded = false;
                        isWalled = false;
                    }
            }
            else
            {
                isGrounded = false;
                isWalled = false;
            }

        }
    }
    #endregion

    #region ON TRIGGER ENTER/EXIT
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == StartLine)
        {
            LevelTimer levelTimerScript = levelTimerUI.GetComponent<LevelTimer>();
            levelTimerScript.levelTimerActive = true;
            Ball ballScript = ball.GetComponent<Ball>();
            ballScript.ActivateBall();
            canCatch = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }
    #endregion
}
