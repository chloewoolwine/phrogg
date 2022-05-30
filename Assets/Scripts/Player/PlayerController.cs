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

    private void Update()
    {
        if(alive && !paused)
        {
            UpdateJumpBehavior();
            UpdateMouseBehavior();
            if(myspring.enabled) UpdateHookBehavior();
        }
    }

    private void FixedUpdate()
    {
        if (alive && !paused)
        {
            if (mybody.velocity.x > 0.01f) mysprite.transform.localScale = new Vector3(.5f, transform.localScale.y, transform.localScale.z);
            else if (mybody.velocity.x < -0.01f) mysprite.transform.localScale = new Vector3(-.5f, transform.localScale.y, transform.localScale.z);

            if (!myspring.enabled)
                FixedUpdateRunBehavior();
            else
                FixedUpdateHookBehavior();
        }
    }

    private void FixedUpdateRunBehavior()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float inputspeed = (horizontal * baseMoveSpeed);
        ground = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), -Vector2.up);
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

    public void UpdateHookBehavior()
    {
        if (!mytongue.enabled) myspring.enabled = false;
        else
        {
            mytongue.SetPosition(0, firePoint.position);
            if (myspring.connectedBody != null)
            {
                //   myspring.connectedAnchor = myspring.connectedBody.transform.position;
                Vector2 point = myspring.connectedBody.GetComponentInParent<Transform>().position;
                mytongue.SetPosition(1, myspring.connectedAnchor + point);
            }
        }
    }

    private bool isfirstswing;
    private void FixedUpdateHookBehavior()
    {
        if (!mytongue.enabled) myspring.enabled = false;
        else
        {
            float vertical = Input.GetAxis("Vertical");
            if (vertical != 0)
            {
                myspring.distance += (vertical * -.1f / slurpspeed);
            }
            if (myspring.distance > tongueLength) myspring.distance = tongueLength;

            float input = Input.GetAxisRaw("Horizontal");
            Vector2 force = new Vector2(swingSpeed, 0) * input;
            if (isfirstswing)
            {
                isfirstswing = false;
                force += (additionalMomentum / 700) * Vector2.right;
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

    private void ShootHook()
    {
        myspring.enabled = false;
        mytongue.enabled = false;
        Vector2 direction = mainCamera.ScreenToWorldPoint(Input.mousePosition) - firePoint.position;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction.normalized);
        if (hit)
        {
            if (Vector2.Distance(hit.point, firePoint.position) <= tongueLength)
            {
                if(hit.collider.tag == "Ground" && !hit.collider.isTrigger)
                {
                    jumpCounter = maxJumps - 1;
                    tongueDirection = direction.normalized;
                    tongueTarget = hit.point;
                    hit.collider.TryGetComponent<MovingPlatform>(out MovingPlatform platform);
                    if (tongueroutine != null) StopCoroutine(tongueroutine);
                    tongueroutine = StartCoroutine(StretchTongue(hit.point, firePoint.position, platform));
                } else if( hit.collider.tag == "Collectable")
                {
                    if (tongueroutine != null)
                    {
                        StopCoroutine(tongueroutine);
                        if(currentCollectable != null)currentCollectable.Release();
                    }
                    tongueroutine = StartCoroutine(StretchTongueCollectable(hit.point, hit.transform.gameObject.GetComponent<Collectable>()));
                }
            }
        }
    }

    private Coroutine tongueroutine;
    private Collectable currentCollectable;
    //TODO currently scales speed based on distance, maybe change this later 
    IEnumerator StretchTongue(Vector2 destination, Vector2 origin, MovingPlatform platform)
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
            yield return new WaitForFixedUpdate();
        }
        mytongue.SetPosition(1, destination);
        myspring.connectedAnchor = destination;
        if (platform != null)
        {
            myspring.connectedAnchor = platform.transform.InverseTransformPoint(destination);
            myspring.connectedBody = platform.GetComponent<Rigidbody2D>();
        }
        myspring.distance = distance;
        myspring.enabled = true;
        isfirstswing = true;

        if (additionalMomentum > (maxSpeed * .7f))
        {
            myspring.autoConfigureDistance = false;
            myspring.distance = myspring.distance - 1f;
        }
    }

    public IEnumerator StretchTongueCollectable(Vector2 destination, Collectable collectable)
    {
        currentCollectable = collectable;
        Vector2 origin = firePoint.position; //reset origin
        float distance = Vector2.Distance(origin, destination);
        mytongue.enabled = true;
        float counter = 0;
        while (counter < 1f)
        {
            counter += .1f / lineDrawSpeed;
            origin = firePoint.position; //reset origin
            mytongue.SetPosition(0, origin);
            mytongue.SetPosition(1, Vector2.Lerp(origin, destination, counter));
            yield return new WaitForFixedUpdate();
        }
        collectable.AttatchToTongue();
        mytongue.SetPosition(1, destination);
        while (counter > 0f)
        {
            counter -= .1f / lineDrawSpeed;
            origin = firePoint.position; //reset origin
            mytongue.SetPosition(0, origin);
            Vector2 pos = Vector2.Lerp(origin, destination, counter);
            mytongue.SetPosition(1, pos);
            if(collectable != null)
                collectable.transform.position = pos;
            yield return new WaitForFixedUpdate();
        }
        mytongue.enabled = false;
    }

    private void StopHook()
    {
        mytongue.enabled = false;

        if (myspring.enabled)
        {
            myspring.connectedBody = null;
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
            yield return new WaitForFixedUpdate();
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
