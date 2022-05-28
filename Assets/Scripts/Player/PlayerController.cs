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


    void Update()
    {
        Vector2 move = Vector2.zero;
        ground = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), -Vector2.up);
        if (alive && !paused)
        {
            if (mybody.velocity.x > 0.01f)
            {
                mysprite.transform.localScale = new Vector3(.5f, transform.localScale.y, transform.localScale.z);
            }
            else if (mybody.velocity.x < -0.01f)
            {
                mysprite.transform.localScale = new Vector3(-.5f, transform.localScale.y, transform.localScale.z);
            }

            if (Input.GetButtonDown("Jump"))
            {
                if ((grounded) || (!grounded && jumpCounter < maxJumps))
                {
                    mybody.gravityScale = 0;
                    jumpCounter++;
                    Jump();
                }
            }
            if (Input.GetButtonUp("Jump"))
            {
                mybody.gravityScale = gravityScale;
            }

            if (grounded)
            {
                if (additionalMomentum > 0) additionalMomentum = additionalMomentum - .1f;
                if (additionalMomentum < 0) additionalMomentum = additionalMomentum + .1f;
            }

            /*
            if (Input.GetButtonDown("Cancel"))
            {
                pauseMenu.SetActive(true);
                //store momentum and crap
            }
             */
            if (myspring.enabled == false)
            {
                float inputspeed = (Input.GetAxis("Horizontal") * baseMoveSpeed);
                if (Math.Sign(inputspeed) != Math.Sign(additionalMomentum) && inputspeed != 0) additionalMomentum = 0; //cancel all momentum on turn
                move.x = inputspeed + (additionalMomentum / 10);
                currentSpeed = mybody.velocity.x;

                move.y = mybody.velocity.y;
                if (!grounded && (Math.Abs(mybody.velocity.x) > Math.Abs(move.x)))
                    move.x = mybody.velocity.x;
                mybody.velocity = move;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKeyDown(KeyCode.Mouse1))
            {
                ShootHook();
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                StopHook();
            }

            if (myspring.enabled)
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
                        if (myspring.distance > tongueLength) myspring.distance = tongueLength;
                    }
                    else myspring.autoConfigureDistance = true;

                }
            }
        }

    }
    private bool isfirstswing;

    //used exclusively for applying force to swinging
    //TODO: add force buildup over time
    private void FixedUpdate()
    {
        if (myspring.enabled)
        {

            Vector2 force = new Vector2(swingSpeed, 0) * Input.GetAxisRaw("Horizontal");
            if (isfirstswing)
            {
                isfirstswing = false;
                force += (additionalMomentum) * Input.GetAxisRaw("Horizontal") * Vector2.right;
            }

            mybody.AddForce(force, ForceMode2D.Impulse);


            var clockwise = Input.GetKeyDown(KeyCode.D);
            var anticlockwise = Input.GetKeyDown(KeyCode.A);
            var constraint = (tongueTarget - mybody.position).normalized;

            if (clockwise)
            {
                var f = new Vector2(-constraint.y, constraint.x) * TangentForce;
                mybody.AddForce(force, ForceMode2D.Impulse);
            }

            if (anticlockwise)
            {
                var f = new Vector2(constraint.y, -constraint.x) * TangentForce;
                mybody.AddForce(force, ForceMode2D.Impulse);
            }
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
                jumpCounter = maxJumps-1;
                tongueDirection = direction.normalized;
                tongueTarget = hit.point;
                if (tongueroutine != null) StopCoroutine(tongueroutine);
                tongueroutine = StartCoroutine(StretchTongue(hit.point, firePoint.position));
                //start courtine for animation?
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

            //         float lengthpercent = Mathf.Lerp(0, distance, counter);
            //         Vector2 point = lengthpercent * tongueDirection + origin;
            mytongue.SetPosition(1, Vector2.Lerp(origin, destination, counter));
            yield return null;
        }
        mytongue.SetPosition(1, destination);
        myspring.connectedAnchor = destination;
        myspring.enabled = true;
        isfirstswing = true;
    }

    private void StopHook()
    {
        //todo add animation for slurping tongue back up
        Debug.Log("Yeet");
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
        mybody.velocity = new Vector2(mybody.velocity.x, jumpPower * jumpMultiplier);
        if (jumproutine != null) StopCoroutine(jumproutine);
        jumproutine = StartCoroutine(DetermineGravity(transform.position.y));
    }

    IEnumerator DetermineGravity(float starty)
    {
        while (mybody.gravityScale == 0)
        {
            if (mybody.velocity.x > 0) additionalMomentum += .1f;
            if (mybody.velocity.x < 0) additionalMomentum -= .1f;
            if (transform.position.y - starty >= jumpHeight || mybody.velocity.y < 2)
                mybody.gravityScale = gravityScale;
            yield return null;
        }
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

            //    float x = maincamera.transform.position.x;
            //   float y = maincamera.transform.position.y;
            //    maincamera.transform.SetParent(null);
            yield return new WaitForSeconds(.5f);
            //TODO fade to black
            GameManager.Instance.respawns.RespawnPlayer();
            mysprite.color = Color.white;
            //    maincamera.transform.SetParent(transform);
            //    maincamera.transform.position.Set(x, y, -10);
            //    Debug.Log("x: " + x + " y: " + y);
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
