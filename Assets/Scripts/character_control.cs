using UnityEngine;
using UnityEngine.InputSystem;
// using System;
// using Unity.VisualScripting;
// using UnityEditor.Rendering;


public class character_movement : MonoBehaviour
{
    // headers
    [Header("Player Controller")]
    private Rigidbody2D body;
    private Vector2 moveInput;
    private PlayerInputActions inputActions;
    SpriteRenderer sr;

    [Header("Physics")]
    private const float ACCEL = 1f;        // how fast to speed up (directly adds to velocity so is smaller)
    private const float DAMPING = 10f;     // how fast to slow down (uses lerp so is way bigger)
    private const float runSpeed = 5.5f;        // base movement speed
    private ParticleSystem moveParticles;

    [Header("Squash & Stretch")]
    private Vector3 targetScale;                    // target for squash/stretch scaling (depends on speed)
    private float squashStretchAmount = 0.4f;         // squash/stretch magnitude
    private float squashStretchSpeed = 100f;         // squash/stretch speed
    private Transform sprite;                        // player sprite transform

    [Header("Light Attack")]
    public GameObject lightAttack_prefab;
    private int counter = 0;                    // counter for debugging, global cuz ion want it to be reset locally
    private Vector2 mouseScreenPos;             // mouse screen position (body.linearVelocityX), global so i can use everywhere
    private float cooldown_lightAttack = 0.3f;               // cooldown between light attacks
    private float lasttime_lightAttack = -Mathf.Infinity;    // last time light attacked
    private float bufferTimer_lightAttack = 0f;              // timer for light attack's input buffering
    private float buffer_lightAttack = 0.15f;   // input buffer duration for light attack

    [Header("Heavy Attack")]
    public GameObject heavyAttack_prefab;
    private ParticleSystem bursts;
    private float cooldown_heavyAttack = 1f;                // cooldown between heavy attacks
    private float lasttime_heavyAttack = -Mathf.Infinity;   // last time player heavy attacked
    private float bufferTimer_heavyAttack = 0f;             // timer for heavy attack's input buffering
    private float buffer_heavyAttack = 0.15f;
    private float recoil = 15f;                             // amount of velocity added when heavy attacking
    private bool heavyAttackAvailable = true;

    [Header("Dash")]
    public GameObject dashParticlePrefab;           // particle effect after dashing
    private bool isDashing = false;                 // dash state
    private float dashTimer = 0f;                   // time until next dash is availabe
    private float lastDashTime = -Mathf.Infinity;   // time of the last dash
    private Vector2 dashDirection;                  // dash direction
    private Vector2 lastFacingDirection;            // direction the player last faced
    private float dashSpeed = 3f * runSpeed;        // dash speed
    private float dashDuration = 0.2f;              // dash duration
    private float dashCooldown = 0.5f;              // dash cooldown (wow these comments are so helpful)
    private float lasttime_dash = -Mathf.Infinity;  // last time player dashed
    private float bufferTimer_dash = 0f;            // timer for dash input buffering
    private float buffer_dash = 0.2f;               // input buffer duration for dash



    // Unity functions

    // Runs once when script is loaded (use for internal init)
    void Awake()
    {
        inputActions = new PlayerInputActions();    // init player input listener
        inputActions.Player.Enable();               // enable inputs for player
    }

    // Start is called once before the first execution of Update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();             // physics body
        sprite = GetComponentInChildren<Transform>();   // child sprite transform
        sr = GetComponentInChildren<SpriteRenderer>();  // child sprite renderer

        // particles
        moveParticles = transform.Find("particles").GetComponent<ParticleSystem>(); // movement particle system
        bursts = transform.Find("bursts").GetComponent<ParticleSystem>();

        // misc variable init
        targetScale = Vector3.one;
    }

    // Event-driven stuff (i.e. left click for attack)
    void OnEnable()
    {
        inputActions.Player.Enable();                           
        inputActions.Player.Attack_light.performed += OnLightAttack;
        inputActions.Player.Attack_heavy.performed += OnHeavyAttack;
        inputActions.Player.Dash.performed += OnDash;
    }

    // after said event ends
    void OnDisable()
    {
        inputActions.Player.Attack_light.performed -= OnLightAttack;
        inputActions.Player.Attack_heavy.performed -= OnHeavyAttack;
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Player.Disable();
    }

    // Use Update for input related stuff (dependent on frame rate)
    void Update()
    {   

        /* MOVEMENT AND MOUSE INPUTS */

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();  // read player input
        ApplySquashStretch();       // procedural animation !!1!

        mouseScreenPos = inputActions.Player.Look.ReadValue<Vector2>(); // read the mouse's screen position

        if (moveInput != Vector2.zero)
        {
            lastFacingDirection = moveInput;
        }


        /* VISUALS OR SOMETHING */
        UpdateMoveParticles();

        if (Time.time - lasttime_heavyAttack >= cooldown_heavyAttack && heavyAttackAvailable == false)
        {
            heavyAttackAvailable = true;

            // particle burst when heavy attack becomes available again
            bursts.Emit(1);

            Color c = sr.color;
            c.a = 1f;               // set alpha back to 1 if heavy attack available
            sr.color = c;
        }


        /* INPUT BUFFERS */

        if (bufferTimer_lightAttack > 0)    // if buffered light attack input
        {
            bufferTimer_lightAttack -= Time.deltaTime;
            if (Time.time - lasttime_lightAttack >= cooldown_lightAttack)
            {
                LightAttack();  
            }
        }
        if (bufferTimer_heavyAttack > 0)
        {
            bufferTimer_heavyAttack -= Time.deltaTime;
            if (Time.time - lasttime_heavyAttack >= cooldown_heavyAttack)
            {
                HeavyAttack();
            }
        }
        if (bufferTimer_dash > 0)           // if buffered dash input
        {
            bufferTimer_dash -= Time.deltaTime;
            if (Time.time - lasttime_dash >= dashCooldown)
            {
                Dash();
            }
        }

    }

    // Use FixedUpdate for physics related stuff (independent of frame rate) 
    void FixedUpdate() 
    {   
        // dash physics
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            dashDirection = moveInput.normalized;
            // if standing still, dash to the last facing direction
            if (dashDirection == Vector2.zero)
            {
                dashDirection = lastFacingDirection.normalized;
            }
            body.linearVelocity = dashDirection * dashSpeed;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
            return;     // skip regular movement if dashing
        }



        // get movement inputs
        Vector2 inputMoveDirection = moveInput.normalized;

        // vector movement physics
        if (inputMoveDirection != Vector2.zero && body.linearVelocity.magnitude < runSpeed)
        {
            body.linearVelocity += inputMoveDirection * ACCEL;
        }
        else
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * DAMPING);
        }
    }



    /* PLAYER FUNCTIONS */

    // procedural animation wowzers
    void ApplySquashStretch()
    {
        Vector2 velocity = body.linearVelocity;
        float speed = velocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(speed / dashSpeed);

        if (speed > 0.1f)
        {
            float stretch = 1f + squashStretchAmount * normalizedSpeed;
            float squash = 1f / stretch;

            targetScale = new Vector3(squash, stretch, 1f);

            float angle = Mathf.Atan2(velocity.x, velocity.y) * Mathf.Rad2Deg;
            sprite.rotation = Quaternion.Lerp(
                sprite.rotation,
                Quaternion.Euler(0f, 0f, -angle),
                Time.deltaTime * squashStretchSpeed
            );
        }
        else
        {
            targetScale = Vector3.one;
            // sprite.rotation = Quaternion.Lerp(
            //     sprite.rotation,
            //     Quaternion.identity,
            //     Time.deltaTime * squashStretchSpeed
            // );
        }

        sprite.localScale = Vector3.Lerp(
            sprite.localScale,
            targetScale,
            Time.deltaTime * squashStretchSpeed
        );
    }

    // movement particles
    void UpdateMoveParticles()
    {
        if (body.linearVelocity.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(body.linearVelocity.y, body.linearVelocity.x) * Mathf.Rad2Deg;
            
            // rotate only the particle system, not the visual
            transform.Find("particles").rotation = Quaternion.Euler(0f, 0f, angle + 180f - 25f);

            if (!moveParticles.isPlaying)
                moveParticles.Play();
        }
        else
        {
            if (moveParticles.isPlaying)
                moveParticles.Stop();
        }
    }



    /* EVENT DRIVEN PLAYER ACTIONS (called by input buffers) */

    // on left click (light attack):
    void OnLightAttack(InputAction.CallbackContext context)
    {
        bufferTimer_lightAttack = buffer_lightAttack;   // buffer input
    }

    // on right click (heavy attack):
    void OnHeavyAttack(InputAction.CallbackContext context)
    {
        bufferTimer_heavyAttack = buffer_heavyAttack;
    }

    // on left shift click (dash)
    void OnDash(InputAction.CallbackContext context)
    {
        bufferTimer_dash = buffer_dash;
    }



    /* PLAYER ACTIONS */

    // light attack
    void LightAttack()
    {
        counter += 1;
        bool mirror = (counter % 2 == 0);   // if odd, do normal attack; if even, do mirrored attack flipped 180deg
        Quaternion aimAngleEuler;
        lasttime_lightAttack = Time.time;
        
        // convert the screen pos (pixels) of the mouse to the world pos (coords)
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        // calculate the angle between the mouse and the player
        float aimAngle = Mathf.Atan2(mouseWorldPos.y - transform.position.y, mouseWorldPos.x - transform.position.x) * Mathf.Rad2Deg;
        // get the rotation
        if (mirror)
        {
            aimAngleEuler = Quaternion.Euler(0f, 0f, aimAngle + 180f);
        }
        else
        {
            aimAngleEuler = Quaternion.Euler(0f, 0f, aimAngle);
        }

        // create a new slash for each left click
        // set the slash position
        GameObject slash = Instantiate(lightAttack_prefab, transform.position, aimAngleEuler);
        slash.GetComponent<slash_prefab>().Init(transform);

        if (mirror)
        {
            slash.transform.localScale = new Vector3(-2f, 1.5f, 1f);
        }
        else
        {
            slash.transform.localScale = new Vector3(2f, 1.5f, 1f);
        }

        // destroy it 0.3 seconds after creation (animation lasts 7/30 = 0.233 seconds)
        Destroy(slash, 0.3f);

        // debugging
        Debug.Log("Clicked: " + counter.ToString() + 
            ", body (" + body.position.x.ToString() + "," + body.position.y.ToString() + 
            "), mouse (" + mouseWorldPos.x.ToString() + "," + mouseWorldPos.y.ToString() + 
            "), aimAngle: " + aimAngle.ToString()
        );
    }

    void HeavyAttack()
    {
        heavyAttackAvailable = false;
        Quaternion aimAngleEuler;

        lasttime_heavyAttack = Time.time;
        Color c = sr.color;
        c.a = 0.5f; // change alpha of sprite since heavy attack just performed
        Debug.Log("Heavy attacked, alpha changed");
        sr.color = c;
        bursts.Clear();

        // convert the screen pos (pixels) of the mouse to the world pos (coords)
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        float aimAngle = Mathf.Atan2(mouseWorldPos.y - transform.position.y, mouseWorldPos.x - transform.position.x) * Mathf.Rad2Deg;

        aimAngleEuler = Quaternion.Euler(0f, 0f, aimAngle);

        GameObject heavyAttack = Instantiate(heavyAttack_prefab, transform.position, aimAngleEuler);
        SpriteRenderer heavyAttackSprite = heavyAttack.GetComponentInChildren<SpriteRenderer>();
        //heavyAttackSprite.color = new Color32(236, 229, 62, 255);     // same yellow as sprite
        heavyAttack.transform.localScale = new Vector3(3f, 2f, 1f);

        body.linearVelocity = new Vector2(
            Mathf.Cos((aimAngle + 180f) * Mathf.Deg2Rad),
            Mathf.Sin((aimAngle + 180f) * Mathf.Deg2Rad)
        ) * recoil + body.linearVelocity;

        Destroy(heavyAttack, 0.3f);
    }

    // dash action
    void Dash()
    {
        if (Time.time - lastDashTime >= dashCooldown && !isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;
            dashDirection = moveInput.normalized;

            // if standing still, dash to the last facing direction
            if (dashDirection == Vector2.zero)
            {
                dashDirection = lastFacingDirection.normalized;
            }

            SpawnDashParticles();   // shoot out particle effect behind player when dashing
        }
    }

    // dash particle effect
    void SpawnDashParticles()
    {
        GameObject particles = Instantiate(dashParticlePrefab, transform.position, Quaternion.identity);

        // rotate so particles shoot opposite to dash direction
        float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
        particles.transform.rotation = Quaternion.Euler(0f, 0f, angle + 180f - 20f);

        Destroy(particles, 1f);
    }


    /* TODO:
    + heavy attack spritesheet
        + heavy attack implementation
        + lower player alpha when heavy attack not available
        + particle effect when heavy attack becomes available: player flashes
    + dash implementation
        + fix end of dash re-deceleration
        + dash input buffering
        + dash particle effect
    + redo squash/stretch to be max velocity (dash speed) based
    + fix physics engine (again)

    = hitboxes/hurtboxes
        = player
        = enemy (?)
    = first enemy implementation
        = sprite
        = attack animation (?)
        = ai????
        = pathfinding (probably delay until i actually add a proper map)
    */


}