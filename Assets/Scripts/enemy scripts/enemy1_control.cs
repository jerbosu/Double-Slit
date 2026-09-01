using UnityEngine;
using System.Collections;

public class enemy1_control : MonoBehaviour
{
    /* VARIABLES */
    [Header("Stats")]
    public float health = 10f;
    private bool scared = false;
    private float scaredTimer = 0;

    [Header("Timers")]
    public float minCircleTime = 1f;
    public float maxCircleTime = 3f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;
    
    private float circleDirection; 
    private float circleTimer;
    private float idleTimer;
    private Vector2 idleDir;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float accel = 10f;
    public float damping = 1f;

    [Header("Attack")]
    public float attackCooldown = 3f;
    public float attackDamage = 20f;
    public float attackForeswing = 0.7f;
    public GameObject telegraphPrefab;              // telegraph for the attack AoE
    private SpriteRenderer telegraphFlash;          // telegraph for the attack timing flash
    private Parryable parryWindow;
    private Coroutine telegraphCoroutine;
    private GameObject activeTelegraph;
    private Vector2 aimDirection;
    private float aimLockTime = 0.3f;       // how long before the enemy attack should their aim be locked for

    private float cooldownTimer = 0;
    private float foreswingTimer;

    /* STATE MACHINE VARIABLES */
    private enum EnemyState {TooFar, TooClose, Idle, Circling, Attacking, Retreating}
    private EnemyState state = EnemyState.Idle;
    private float distance = Mathf.Infinity;
    private Vector2 direction;
    private float activationRange = 5f;
    private bool initialContact = true;
    private bool PlayerTooFar => distance > 2f;
    private bool PlayerTooClose => distance < 1f;
    private bool PlayerInRange => !PlayerTooFar && !PlayerTooClose;
    private bool ReadyToAttack => circleTimer <= 0 && cooldownTimer <= 0;

    private Rigidbody2D body;
    private Transform player;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // gameobject init
        body = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
        telegraphFlash = transform.Find("telegraph_0").GetComponent<SpriteRenderer>();
        parryWindow = GetComponent<Parryable>();

        // behaviour init
        circleDirection = Random.value > 0.5f ? 1f : -1f;   // if greater than 0.5, 1 (CW), else -1 (CCW)
        circleTimer = Random.Range(minCircleTime, maxCircleTime);
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        foreswingTimer = attackForeswing;

        GetComponentInChildren<Hurtbox>().onDeath += Die;
        GetComponent<Parryable>().onParried += OnParried;
    }

    // Update is called once per frame
    void Update()
    {
        // StateMachine();

        // Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        distance = Vector2.Distance((Vector2)player.position, (Vector2)transform.position);
        direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        UpdateState();  
        // Debug.Log("Parryable: " + parryWindow.isParryable);
    }

    void FixedUpdate()
    {
        
        cooldownTimer -= Time.deltaTime;

        // run the corresponding state function
        switch(state)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.TooFar:
                TooFar();
                break;
            case EnemyState.TooClose:
                TooClose();
                break;
            case EnemyState.Circling:
                Circle();
                break;
            case EnemyState.Attacking:
                body.bodyType = RigidbodyType2D.Kinematic;
                Attack();
                break;
            case EnemyState.Retreating:
                body.bodyType = RigidbodyType2D.Dynamic;
                Retreat();
                break;
        }
    }









    /* STATE UPDATE FUNCTIONS */
    // call the update functions
    void UpdateState()
    {
        switch (state)
        {
            case EnemyState.Idle:       UpdateIdle();       break;
            case EnemyState.TooFar:     UpdateTooFar();     break;
            case EnemyState.TooClose:   UpdateTooClose();   break;
            case EnemyState.Circling:   UpdateCircling();   break;
            case EnemyState.Attacking:  UpdateAttacking();  break;
            case EnemyState.Retreating: UpdateRetreating(); break;
        }
    }

    // update while in Idle state
    void UpdateIdle()
    {
        // idle until player gets close, then follow player forever
        if (distance > activationRange && initialContact == true) return;
        initialContact = false;
        idleTimer -= Time.deltaTime;
        // if idleTimer not over yet keep idling unless player is too close
        if (idleTimer > 0f && distance > activationRange) return;

        // if idleTimer ended change behaviour
        idleTimer = 0f;
        if (scared)         { EnterRetreating(); return; }
        if (PlayerTooFar)   { state = EnemyState.TooFar; return; }
        if (PlayerTooClose) { state = EnemyState.TooClose; return; }
        // if all of the above are false, begin circling
        state = EnemyState.Circling;
    }

    // update while in TooFar state
    void UpdateTooFar()
    {
        if (!PlayerTooFar) state = EnemyState.Idle;
    }

    // update while in TooClose state
    void UpdateTooClose()
    {
        circleTimer -= Time.deltaTime;
        if (ReadyToAttack) {state = EnemyState.Attacking; return;}
        if (!PlayerTooClose) state = EnemyState.Idle;
    }

    // update while in Circling state
    void UpdateCircling()
    {
        if (scared)         { EnterRetreating(); return; }
        if (!PlayerInRange) { state = EnemyState.Idle; return; }

        circleTimer -= Time.deltaTime;
        if (ReadyToAttack) state = EnemyState.Attacking;
    }

    // update while in Attacking state
    void UpdateAttacking()
    {
        if (scared) { EnterRetreating(); return;}
    }

    // update while in Retreating state
    void UpdateRetreating()
    {
        scaredTimer -= Time.deltaTime;
        if (scaredTimer <= 0) EnterIdle();
    }



    /* STATE TRANSITION FUNCTIONS */
    // Idle is the base state, every other state can be reached from it
    void EnterIdle()
    {
        state = EnemyState.Idle;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        scared = false;
    }

    // Retreat if scared
    void EnterRetreating()
    {
        state = EnemyState.Retreating;
    }



    /* STATE BEHAVIOUR FUNCTIONS */
    // observe the player
    void Idle()
    {
        if (initialContact == true)
        {
            idleDir = Random.insideUnitCircle.normalized;
        }
        else
        {
            idleDir = Quaternion.Euler(0f, 0f, Random.Range(-90f, 90f)) * direction;
        }
        
        body.AddForce(0.5f * accel * idleDir, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }

        // Debug.Log("Idle: " + idleTimer.ToString());
    }

    // if player is >2 units away, pathfind to player
    void TooFar()
    {
        body.AddForce(direction * accel, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
        circleDirection = Random.value > 0.5f ? 1f : -1f; // if greater than 0.5, 1 (CW), else -1 (CCW)

        // Debug.Log("Too far: " + distance.ToString());
    }

    // if player is <1 unit away, retreat slightly
    void TooClose()
    {
        body.AddForce(-direction * accel, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
        circleDirection = Random.value > 0.5f ? 1f : -1f; // if greater than 0.5, 1 (CW), else -1 (CCW)

        // Debug.Log("Too close: " + distance.ToString());
    }

    // if player is 1-2 units away, circle with a chance to attack
    void Circle()
    {
        Vector2 perp = Vector2.Perpendicular(direction) * circleDirection;

        // tangential accel
        body.AddForce(perp * accel, ForceMode2D.Force);
        // normal accel
        float tanVelocity = Vector2.Dot(body.linearVelocity, perp);
        body.AddForce(direction * Mathf.Pow(tanVelocity, 2) / distance, ForceMode2D.Force);

        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = body.linearVelocity.normalized * moveSpeed;
        }

        // Debug.Log("Circling: " + circleTimer.ToString());
    }



    
    /* ACTION FUNCTIONS */
    // Attack (ram the player)
    void Attack()
    {
        // if player moves out of range, clean up
        if (distance > 2.5f && foreswingTimer > aimLockTime) 
        {
            state = EnemyState.Idle; 
            body.bodyType = RigidbodyType2D.Dynamic;
            foreswingTimer = attackForeswing;   // reset foreswing timer
            // circleTimer = Random.Range(minCircleTime, maxCircleTime);   // reset circling timer

            if (telegraphCoroutine != null)     // if telegraph coroutine started, stop it
            {
                StopCoroutine(telegraphCoroutine);
                telegraphCoroutine = null;
            }

            if (activeTelegraph != null)        // if the telegraph sprite is active, destroy it
            {
                Destroy(activeTelegraph);
                activeTelegraph = null;
            }
            
            return; 
        }

        // attack sequence
        if (body.linearVelocity.magnitude > 0.5)    // slow down before attacking
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping * 5);
        }
        else if (foreswingTimer > 0)    // attack telegraph, etc.
        {
            if (telegraphCoroutine == null)
            {
                telegraphCoroutine = StartCoroutine(TelegraphAoE());
            }
            foreswingTimer -= Time.deltaTime;
            if (foreswingTimer > aimLockTime)  // freeze aim direction 0.5 seconds before attacking
            {
                aimDirection = direction;
            }
            else
            {
                telegraphFlash.color = new Color(1f, 0f, 0f, foreswingTimer / aimLockTime);
                telegraphFlash.transform.localScale = telegraphFlash.transform.localScale * 0.99f;
                parryWindow.isParryable = true;
                
            }
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping * 3);
        }
        else
        {
            body.bodyType = RigidbodyType2D.Dynamic;                        // kinematic --> dynamic
            telegraphFlash.color = new Color(1f, 0f, 0f, 0f);               // make the flash invisible again
            telegraphFlash.transform.localScale = new Vector3(1f, 1f, 1f);  // idk
            body.AddForce(aimDirection * 25f, ForceMode2D.Impulse);         // enemy ram attack
            circleTimer = Random.Range(minCircleTime, maxCircleTime);       // set timer
            cooldownTimer = attackCooldown;                                 // set timer
            foreswingTimer = attackForeswing;                               // reset foreswing
            scared = true;  
            scaredTimer = 1f;
        }

        // Debug.Log("Attacking");
    }

    // Attack AoE telegraph
    IEnumerator TelegraphAoE()
    {
        
        // aim telegraph toward player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        activeTelegraph = Instantiate(telegraphPrefab, transform.position, rotation);
        AttackTelegraph telegraph = activeTelegraph.GetComponent<AttackTelegraph>();
        telegraph.duration = foreswingTimer;
        telegraph.size = new Vector2(0.3f, 2.5f);
        telegraph.lockTime = aimLockTime;
        telegraph.Init(transform, player);

        yield return new WaitForSeconds(foreswingTimer);

        Destroy(activeTelegraph);
        telegraphCoroutine = null;
    }

    // if just attacked or took significant damage, retreat a short distance
    void Retreat()
    {
        if (body.linearVelocity.magnitude > 1f && scaredTimer <= 0.8f)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, body.linearVelocity.normalized, damping * 5f);
        }
        else { parryWindow.isParryable = false; }
        
        body.AddForce(-direction * accel * 0.5f, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
        scaredTimer -= Time.deltaTime;

        // Debug.Log("Retreating: " + scaredTimer.ToString());
    }





    /* EVENT FUNCTIONS */

    void OnParried()
    {
        // "stun" enemy and set to scared
        StopAllCoroutines();
        parryWindow.isParryable = false;
        state = EnemyState.Retreating; 
        body.bodyType = RigidbodyType2D.Dynamic;
        foreswingTimer = attackForeswing;   // reset foreswing timer
        
        telegraphFlash.color = new Color(1f, 0f, 0f, 0f);               // make the flash invisible again
        telegraphFlash.transform.localScale = new Vector3(1f, 1f, 1f);  // idk
        circleTimer = Random.Range(minCircleTime, maxCircleTime);       // set timer
        scared = true;  
        scaredTimer = 1f;

        if (telegraphCoroutine != null)     // if telegraph coroutine started, stop it
        {
            StopCoroutine(telegraphCoroutine);
            telegraphCoroutine = null;
        }

        if (activeTelegraph != null)        // if the telegraph sprite is active, destroy it
        {
            Destroy(activeTelegraph);
            activeTelegraph = null;
        }

        // hitstp
        StartCoroutine(ParryHitstop());
    }

    IEnumerator ParryHitstop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;
    }

    void Die()
    {
        // death particle effect here
        Destroy(gameObject);
    }

}
