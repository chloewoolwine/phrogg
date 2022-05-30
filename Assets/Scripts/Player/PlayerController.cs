using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D mybody;
    public SpriteRenderer mysprite;
    public Camera mainCamera;
    public SpringJoint2D myspring;
    public Transform firePoint;
    public LineRenderer mytongue;

    //a lot of shit you grabbed from https://github.com/atmosgames/SuperSimple2DKit/blob/master/Assets/Scripts/Core/NewPlayer.cs
    [Header("Properties")]
    public bool alive = true;
    public bool paused = false;

    [ReadOnly] public RaycastHit2D ground; //might be good for playing feet sounds
    /*
    private float fallForgivenessCounter; //Counts how long the player has fallen off a ledge
    [SerializeField] private float fallForgiveness = .2f; //How long the player can fall from a ledge and still jump
    [SerializeField] private Vector2 hurtLaunchPower; //How much force should be applied to the player when getting hurt?
    private float launch; //The float added to x and y moveSpeed. This is set with hurtLaunchPower, and is always brought back to zero
    [SerializeField] private float launchRecovery; //How slow should recovering from the launch be? (Higher the number, the longer the launch will last)
    */
    [Header("Speed")]
    public float maxSpeed = 7; //Max move speed
    public int baseMoveSpeed = 10;
    public float accelerationFactor = .01f;
    public float deccelerationFactor = .01f;
    [ReadOnly] public float currentSpeed;
    [ReadOnly] public float additionalMomentum;

    [Header("Jumps")]
    public float jumpPower = 17;
    public float jumpHeight = 10;
    public int maxJumps = 2;
    [ReadOnly] public bool grounded;
    [ReadOnly] public float jumpCounter;
    public int jumpMultiplier = 1;
    public float gravityScale = 13;

    [Header("Tongue")]
    public float tongueLength;
    public float launchSpeed;
    public float lineDrawSpeed = 1;
    public float slurpspeed = 1;
    [ReadOnly] public Vector2 tongueTarget;
    [ReadOnly] public Vector2 tongueDirection;
    public int swingSpeed = 3;
    public float TangentForce = 1.0f;

    // Singleton instantiation
    private static PlayerController instance;
    public static PlayerController Instance
    {
        get
        {
            if (instance == null) instance = GameObject.FindObjectOfType<PlayerController>();
            return instance;
        }
    }

    private void Start()
    {
        mytongue.enabled = false;
        myspring.enabled = false;
    }

    // ground = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), -Vector2.up); 
    // ^ for potentially different step sounds

    void Update()
    {
        if (alive && !paused)
        {
            if (mybody.velocity.x > 0.01f) mysprite.transform.localScale = new Vector3(.5f, transform.localScale.y, transform.localScale.z);
            else if (mybody.velocity.x < -0.01f) mysprite.transform.localScale = new Vector3(-.5f, transform.localScale.y, transform.localScale.z);
           
            UpdateJumpBehavior();
            if (!myspring.enabled)
                UpdateRunBehavior();
            else
                UpdateHookBehavior();
            UpdateMouseBehavior();
        }

    }

    private void UpdateRunBehavior()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float inputspeed = (horizontal * baseMoveSpeed);
        if (horizontal != 0)
        {
            if (mybody.velocity.x == 0) additionalMomentum = 0;
            if (Math.Abs(additionalMomentum) < maxSpeed)
            {
                if (!grounded)
                    Accelerate();
                Accelerate();
            }
            if (Math.Sign(inputspeed) != Math.Sign(additionalMomentum)) additionalMomentum = 0; //cancel all momentum on turn
        }
        else
        {
            Deccelerate();
        }
        Vector2 move = Vector2.zero;
        move.x = inputspeed + (additionalMomentum / 10);
        currentSpeed = mybody.velocity.x;

        move.y = mybody.velocity.y;
        if (!grounded && (Math.Abs(mybody.velocity.x) > Math.Abs(move.x)))
            move.x = mybody.velocity.x;
        mybody.velocity = move;
    }

    private void UpdateJumpBehavior()
    {
        bool jumpstart = Input.GetButtonDown("Jump");
        bool jumpend = Input.GetButtonUp("Jump");
        bool jumpheld = Input.GetButton("Jump");

        if (jumpheld && !jumpstart && !jumpend)
        {
            if (grounded)
            {
                jumpCounter = 0;
                Jump();
            }
        }
        if (jumpstart)
        {
            if ((grounded) || (!grounded && jumpCounter < maxJumps))
            {
                Jump();
            }
        }
        if (jumpend)
        {
            mybody.gravityScale = gravityScale;
        }
    }

    private void UpdateHookBehavior()
    {
        if (!mytongue.enabled) myspring.enabled = false;
        else
        {
            mytongue.SetPosition(0, firePoint.position);
            float vertical = Input.GetAxis("Vertical");
            if (vertical != 0)
            {
                myspring.autoConfigureDistance = false;
                myspring.distance += (vertical * -.1f / slurpspeed);
            }
            else myspring.autoConfigureDistance = true;
            if (myspring.distance > tongueLength) myspring.distance = tongueLength;
        }
    }

    private void UpdateMouseBehavior()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKeyDown(KeyCode.Mouse1))
        {
            ShootHook();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StopHook();
        }
    }

    private bool isfirstswing;
    //used exclusively for applying force to swinging
    //TODO: add force buildup over time
    private void FixedUpdate()
    {
        if (myspring.enabled)
        {
            float input = Input.GetAxisRaw("Horizontal");
            Vector2 force = new Vector2(swingSpeed, 0) * input;
            if (isfirstswing)
            {
                isfirstswing = false;
                force += (additionalMomentum) * Vector2.right;
            }

            mybody.AddForce(force, ForceMode2D.Impulse);

            var constraint = (tongueTarget - mybody.position).normalized;

            if (input > 0)
            {
                var f = new Vector2(-constraint.y, constraint.x) * TangentForce;
                mybody.AddForce(force, ForceMode2D.Impulse);
            }
            else if (input < 0)
            {
                var f = new Vector2(constraint.y, -constraint.x) * TangentForce;
                mybody.AddForce(force, ForceMode2D.Impulse);
            }
            else
            {
                Deccelerate();
            }
            if (Math.Sign(input) != Math.Sign(additionalMomentum)) additionalMomentum = 0;
        }
    }

    private void ShootHook()
    {
        myspring.enabled = false;
        mytongue.enabled = false;
        Vector2 direction = mainCamera.ScreenToWorldPoint(Input.mousePosition) - firePoint.position;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction.normalized);
        if (hit)
        {
            if (Vector2.Distance(hit.point, firePoint.position) <= tongueLength && hit.collider.tag == "Ground"
                && !hit.collider.isTrigger)
            {
                jumpCounter = maxJumps - 1;
                tongueDirection = direction.normalized;
                tongueTarget = hit.point;
                if (tongueroutine != null) StopCoroutine(tongueroutine);
                tongueroutine = StartCoroutine(StretchTongue(hit.point, firePoint.position));
            }
        }
    }

    private Coroutine tongueroutine;
    //TODO add stretch tongue no hit to make an animation for when you dont hit 
    IEnumerator StretchTongue(Vector2 destination, Vector2 origin)
    {
        float distance = Vector2.Distance(origin, destination);
        mytongue.enabled = true;
        float counter = 0;
        while (counter < 1f)
        {
            counter += .1f / lineDrawSpeed;
            origin = firePoint.position; //reset origin
            mytongue.SetPosition(0, origin);
            mytongue.SetPosition(1, Vector2.Lerp(origin, destination, counter));
            yield return null;
        }
        mytongue.SetPosition(1, destination);
        myspring.connectedAnchor = destination;
        myspring.enabled = true;
        isfirstswing = true;

        if (additionalMomentum > (maxSpeed * .7f))
        {
            myspring.autoConfigureDistance = false;
            myspring.distance = myspring.distance - 1f;
        }
    }

    private void StopHook()
    {
        mytongue.enabled = false;

        if (myspring.enabled)
        {
            myspring.enabled = false;
            additionalMomentum += mybody.velocity.x;
            Vector2 force = new Vector2(swingSpeed, 0) * mybody.velocity.x;
            mybody.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private Coroutine jumproutine;


    private void Jump()
    {
        jumpCounter++;
        mybody.gravityScale = 0;
        mybody.velocity = new Vector2(mybody.velocity.x, jumpPower * jumpMultiplier);
        if (jumproutine != null) StopCoroutine(jumproutine);
        jumproutine = StartCoroutine(DetermineGravity(transform.position.y));
    }

    IEnumerator DetermineGravity(float starty)
    {
        while (mybody.gravityScale == 0)
        {
            if (transform.position.y - starty >= jumpHeight || mybody.velocity.y < 2)
                mybody.gravityScale = gravityScale;
            yield return null;
        }
    }

    private void Accelerate()
    {
        if (mybody.velocity.x > 0) additionalMomentum += accelerationFactor;
        if (mybody.velocity.x < 0) additionalMomentum -= accelerationFactor;
    }

    private void Deccelerate()
    {
        if (additionalMomentum > 0) additionalMomentum = additionalMomentum - deccelerationFactor;
        if (additionalMomentum < 0) additionalMomentum = additionalMomentum + deccelerationFactor;
    }

    // called when the cube hits the floor
    void OnCollisionEnter2D(Collision2D col)
    {
        mybody.gravityScale = gravityScale;
    }

    //Check if Grounded
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Ground")
        {
            grounded = true;
            jumpCounter = 0;
        }
    }
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Ground")
        {
            grounded = false;
        }
    }


    public IEnumerator Die()
    {

        if (!paused)
        {
            StopHook();
            additionalMomentum = 0;
            mybody.velocity = Vector2.zero;
            alive = false;
            //TODO death animation
            mysprite.color = Color.red;
            //TODO- FIX THE GODAMN CAMERA UGH
            
            yield return new WaitForSeconds(.5f);
            //TODO fade to black (maybe not actually? it kinda looks nice where it flips back!
            GameManager.Instance.respawns.RespawnPlayer();
            mysprite.color = Color.white;
        }

        yield return null;
    }

    public void Revive()
    {
        if (!alive)
        {
            alive = true;
            //TODO revival animation and effects
        }
    }

}
