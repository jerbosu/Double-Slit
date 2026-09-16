// using System;
// using System.Runtime.CompilerServices;
// using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
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
    private const float ACCEL = 50f;        // how fast to speed up (uses addForce so is bigger)
    private const float DAMPING = 5f;     // how fast to slow down (uses lerp so smaller)
    private const float runSpeed = 1.5f;        // base movement speed
    private ParticleSystem moveParticles;

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
    private ParticleSystem heavyAttackIndicator;
    private float cooldown_heavyAttack = 1f;                // cooldown between heavy attacks
    private float lasttime_heavyAttack = -Mathf.Infinity;   // last time player heavy attacked
    private float bufferTimer_heavyAttack = 0f;             // timer for heavy attack's input buffering
    private float buffer_heavyAttack = 0.2f;
    private float recoil = 4f;                             // amount of velocity added when heavy attacking
    private bool heavyAttackAvailable = true;

    [Header("Dash")]
    public GameObject dashParticlePrefab;           // particle effect after dashing
    private bool isDashing = false;                 // dash state
    private float dashTimer = 0f;                   // time until next dash is availabe
    private float lastDashTime = -Mathf.Infinity;   // time of the last dash
    private Vector2 dashDirection;                  // dash direction
    private Vector2 lastFacingDirection;            // direction the player last faced
    private float dashSpeed = 14;                   // dash speed
    private float dashDuration = 0.2f;              // dash duration
    private float dashCooldown = 0.5f;              // dash cooldown (wow these comments are so helpful)
    private float lasttime_dash = -Mathf.Infinity;  // last time player dashed
    private float bufferTimer_dash = 0f;            // timer for dash input buffering
    private float buffer_dash = 0.15f;              // input buffer duration for dash

    [Header("Hitbox Animation")]
    public AnimationCurve hitboxSizeCurve;          // idk
    public Vector2 hitboxStartSize = new Vector2(0.5f, 0.5f);
    public Vector2 hitboxEndSize = new Vector2(2f, 2f);
    public Vector2 hitboxStartOffset = new Vector2(0f, 0.5f);
    public Vector2 hitboxEndOffset = new Vector2(0f, 2f);

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
        sr = GetComponentInChildren<SpriteRenderer>();  // child sprite renderer

        // particles
        moveParticles = transform.Find("particles").GetComponent<ParticleSystem>(); // movement particle system
        heavyAttackIndicator = transform.Find("heavyAttackIndicator").GetComponent<ParticleSystem>();

        GetComponent<Parryable>().onParried += OnParried;
        GetComponentInChildren<Hurtbox>().onHit += () => 
            ScreenFlash.Instance.Flash(new Color(1f, 0f, 0f, 0.1f), 0.05f, true);
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
        ReadInputs();

        /* VISUALS OR SOMETHING */
        // ApplySquashStretch();
        UpdateMoveParticles();
        HeavyAttackCooldown();

        /* INPUT BUFFERS */
        // light attack buffer
        Buffer(ref bufferTimer_lightAttack, lasttime_lightAttack, cooldown_lightAttack, LightAttack);
        // heavy attack buffer
        Buffer(ref bufferTimer_heavyAttack, lasttime_heavyAttack, cooldown_heavyAttack, HeavyAttack);
        // dash buffer
        Buffer(ref bufferTimer_dash, lasttime_dash, dashCooldown, Dash);
    }

    // Use FixedUpdate for physics related stuff (independent of frame rate) 
    void FixedUpdate() 
    {   
        
        // read player input and apply movement
        PlayerMovement();

    }


















    /* PLAYER FUNCTIONS */

    // read mouse position and set a movement flag
    void ReadInputs()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();  // read player input
        mouseScreenPos = inputActions.Player.Look.ReadValue<Vector2>(); // read the mouse's screen position
        if (moveInput != Vector2.zero)
        {
            lastFacingDirection = moveInput;
        }
    }

    // actual movement control
    void PlayerMovement()
    {
        // if dashing, override regular movement
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
                gameObject.layer = LayerMask.NameToLayer("Player"); // change layer back
            }
            return;     // skip regular movement if dashing
        }

        // otherwise, regular movement
        // get movement inputs
        Vector2 inputMoveDirection = moveInput.normalized;
        
        // vector movement physics
        if (inputMoveDirection != Vector2.zero)
        {
            body.AddForce(inputMoveDirection * ACCEL, ForceMode2D.Force);
            // clamp if exceeding max speed
            if (body.linearVelocity.magnitude > runSpeed)
            {
                // body.linearVelocity = body.linearVelocity.normalized * runSpeed;

                // project velocity onto input direction
                float inputAlignedSpeed = Vector2.Dot(body.linearVelocity, inputMoveDirection);
                
                if (inputAlignedSpeed > runSpeed)
                {
                    // lerp the excess back toward cap, preserving recoil
                    Vector2 targetVelocity = body.linearVelocity.normalized * runSpeed;
                    body.linearVelocity = Vector2.Lerp(body.linearVelocity, targetVelocity, Time.fixedDeltaTime * DAMPING * 2.5f);
                }
            }
        }
        else
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * DAMPING);
        }

        // Debug.Log(body.linearVelocity.magnitude);
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

















    /* EVENT FUNCTIONS */
    // the input buffer function
    void Buffer(ref float bufferTimer, float lastTime, float cooldown, System.Action function)
    {
        if (bufferTimer > 0)
        {
            bufferTimer -= Time.unscaledDeltaTime;
            if (Time.unscaledTime - lastTime >= cooldown)
            {
                function();
            }
        }
    }

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

    // if player is parried by enemy
    void OnParried()
    {
        
    }

















    /* PLAYER ACTIONS */

    // light attack
    void LightAttack()
    {
        counter += 1;
        bool mirror = (counter % 2 == 0);   // if odd, do normal attack; if even, do mirrored attack flipped 180deg
        Quaternion aimAngleEuler;
        lasttime_lightAttack = Time.unscaledTime;
        
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
        GameObject slash = Instantiate(lightAttack_prefab, transform.position, aimAngleEuler, transform);
        slash.GetComponent<slash_prefab>().Init(transform);
        Hitbox hitbox = slash.GetComponentInChildren<Hitbox>();        // get the attack's hitbox
        hitbox.attackerHurtbox = GetComponentInChildren<Hurtbox>();     // get the attacker's hurtbox

        if (mirror)
        {
            slash.transform.localScale = new Vector3(-2.5f + Random.Range(-0.1f, 0.1f), 2f + Random.Range(-0.1f, 0.1f), 1f);
        }
        else
        {
            slash.transform.localScale = new Vector3(2.5f + Random.Range(-0.1f, 0.1f), 2f + Random.Range(-0.1f, 0.1f), 1f);
        }

        // destroy it 0.3 seconds after creation (animation lasts 7/30 = 0.233 seconds)
        Destroy(slash, 0.3f);

        // debugging
        // Debug.Log("Clicked: " + counter.ToString() + 
        //     ", body (" + body.position.x.ToString() + "," + body.position.y.ToString() + 
        //     "), mouse (" + mouseWorldPos.x.ToString() + "," + mouseWorldPos.y.ToString() + 
        //     "), aimAngle: " + aimAngle.ToString()
        // );
    }

    void HeavyAttack()
    {
        heavyAttackAvailable = false;
        Quaternion aimAngleEuler;

        lasttime_heavyAttack = Time.unscaledTime;
        Color c = sr.color;
        c.a = 0.5f; // change alpha of sprite since heavy attack just performed
        // Debug.Log("Heavy attacked, alpha changed");
        sr.color = c;
        heavyAttackIndicator.Clear();   // clear the particle indicator
        heavyAttackIndicator.Emit(1);   // restart the particle indicator

        // convert the screen pos (pixels) of the mouse to the world pos (coords)
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        float aimAngle = Mathf.Atan2(mouseWorldPos.y - transform.position.y, mouseWorldPos.x - transform.position.x) * Mathf.Rad2Deg;

        aimAngleEuler = Quaternion.Euler(0f, 0f, aimAngle);

        GameObject heavyAttack = Instantiate(heavyAttack_prefab, transform.position, aimAngleEuler);
        Hitbox hitbox = heavyAttack.GetComponentInChildren<Hitbox>();
        hitbox.isParryAttack = true;
        hitbox.attackerHurtbox = GetComponentInChildren<Hurtbox>();     // get the attacker's hurtbox

        Rigidbody2D spawnedBody = heavyAttack.GetComponent<Rigidbody2D>();
        if (spawnedBody != null) {
            spawnedBody.linearVelocity = body.linearVelocity;   // inherit player velocity
        }
        heavyAttack.transform.localScale = new Vector3(4f, 2f, 1f);

        if (hitbox != null) StartCoroutine(AnimateHitbox(hitbox, 0.25f));

        body.AddForce(((Vector2)transform.position - mouseWorldPos) * recoil, ForceMode2D.Impulse);

        Destroy(heavyAttack, 0.3f);
    }

    // animate heavy attack hitbox
    IEnumerator AnimateHitbox(Hitbox hitbox, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (hitbox == null) yield break; // stop if attack was destroyed early
            elapsed += Time.deltaTime;
            float t = hitboxSizeCurve.Evaluate(elapsed / duration);

            hitbox.SetShape(
                Vector2.Lerp(hitboxStartSize, hitboxEndSize, t),
                Vector2.Lerp(hitboxStartOffset, hitboxEndOffset, t)
            );

            yield return null;
        }
    }

    // calculates time until heavy attack is available again, and sets color indicators
    void HeavyAttackCooldown()
    {
        if (Time.unscaledTime - lasttime_heavyAttack >= cooldown_heavyAttack && heavyAttackAvailable == false)
        {
            heavyAttackAvailable = true;

            Color c = sr.color;
            c.a = 1f;               // set alpha back to 1 if heavy attack available
            sr.color = c;
        }
    }

    // dash action
    void Dash()
    {
        if (Time.unscaledTime - lastDashTime >= dashCooldown && !isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.unscaledTime;
            dashDirection = moveInput.normalized;

            // if standing still, dash to the last facing direction
            if (dashDirection == Vector2.zero)
            {
                dashDirection = lastFacingDirection.normalized;
            }

            // set layer to the dash layer (so dashing lets you pass through enemies)
            gameObject.layer = LayerMask.NameToLayer("PlayerDashing");  
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
}