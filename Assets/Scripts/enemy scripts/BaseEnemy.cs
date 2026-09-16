using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    /* STATS */
    [Header("Stats")]
    [SerializeField] protected float health = 10f;
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float accel = 20f;
    [SerializeField] protected float damping = 10f;

    /* STATE VARIABLES */
    protected enum EnemyState {TooFar, TooClose, Idle, Circling, Attacking, Retreating};
    protected EnemyState state = EnemyState.Idle;
    // idle
    [Header("State Stuff")]
    [SerializeField] protected float minIdleTime = 1f;
    [SerializeField] protected float maxIdleTime = 3f;
    protected float idleTimer;
    protected Vector2 idleDir;              // when idle, move in a random direction
    protected bool initialContact = true;   // true before spotting player, false after
    // circling
    [SerializeField] protected float minCircleTime = 1f;
    [SerializeField] protected float maxCircleTime = 3f;
    protected float circleTimer;
    protected float circleDirection;        // 1 or -1, multiplier on the perp vector for CW or CCW
    // retreating
    protected bool scared = false;
    protected float scaredTimer = 0;
    // attacking

    /* ATTACK STUFF */
    [Header("Attack Stuff")]
    [SerializeField] protected float attackCooldown = 3f;
    [SerializeField] protected float attackDamage = 20f;
    [SerializeField] protected float attackForeswing = 0.7f;
    [SerializeField] protected float attackBackswing = 0.5f;
    [SerializeField] protected float attackRange = 2.5f;
    protected float cooldownTimer = 0;
    
    /* METHODS */
    protected float tooFar = 3f;
    protected float tooClose = 1f;
    protected virtual bool PlayerTooFar => distance > tooFar;
    protected virtual bool PlayerTooClose => distance < tooClose;
    protected virtual bool PlayerInRange => !PlayerTooFar && !PlayerTooClose;
    protected virtual bool ReadyToAttack => circleTimer <= 0 && cooldownTimer <= 0;
    
    /* OTHER STUFF */
    protected Rigidbody2D body;             // enemy rigidbody
    protected Transform playerTransform;    // player position
    protected float distance;               // distance from enemy to player
    protected Vector2 direction;            // direction from enemy to player
    protected float activationRange = 5f;   // distance enemy can spot player from (radius)
















/* UNITY FUNCTIONS */

    protected virtual void Start()
    {
        body = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    /// <summary>
    /// Calculates float distance and Vector2 direction
    /// </summary>
    protected virtual void Update()
    {
        distance = Vector2.Distance(transform.position, playerTransform.position);
        direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        UpdateState();
    }

    protected virtual void FixedUpdate()
    {
        cooldownTimer -= Time.deltaTime;
        SwitchState();
    }










/* USER FUNCTIONS */

    /* STATE UPDATE FUNCTIONS */

    /// <summary>
    /// Generic state switching function. Runs the function corresponding to the state the enemy is in. 
    /// </summary>
    protected virtual void SwitchState()
    {
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
                Circle(1);
                break;
            case EnemyState.Attacking:
                // body.bodyType = RigidbodyType2D.Kinematic;
                Attack();
                break;
            case EnemyState.Retreating:
                // body.bodyType = RigidbodyType2D.Dynamic;
                Retreat();
                break;
        }
    }

    /// <summary>
    /// Placeholder state machine updating function. Runs the state update functions.
    /// </summary>
    protected virtual void UpdateState()
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

    /// <summary>
    /// Idle is the base state for an enemy. 
    /// </summary>
    protected virtual void UpdateIdle()
    {
        // idle until player gets close, then follow player forever
        if (distance > activationRange && initialContact == true) return;
        initialContact = false;
        idleTimer -= Time.deltaTime;
        // if idleTimer not over yet keep idling unless player is too close
        if (idleTimer > 0f && distance > activationRange) return;

        // if idleTimer ended or player too close change behaviour
        idleTimer = 0f;
        if (scared)         { state = EnemyState.Retreating; return; }
        if (PlayerTooFar)   { state = EnemyState.TooFar; return; }
        if (PlayerTooClose) { state = EnemyState.TooClose; return; }
        // if all of the above are false, begin circling
        state = EnemyState.Circling;
    }

    /// <summary>
    /// If the player is no longer too far, enter the idle state
    /// </summary>
    protected virtual void UpdateTooFar()
    {
        if (!PlayerTooFar) state = EnemyState.Idle;
    }

    /// <summary>
    /// If no longer too close, enter idle state. 
    /// circleTimer continues so enemy eventually stops running and attacks.
    /// </summary>
    protected virtual void UpdateTooClose()
    {
        circleTimer -= Time.deltaTime;
        if (ReadyToAttack) {state = EnemyState.Attacking; return;}
        if (!PlayerTooClose) state = EnemyState.Idle;
    }

    /// <summary>
    /// Cancel the Circling state if certain conditions are met. 
    /// These are [scared], [!PlayerInRange], [ReadyToAttack]
    /// </summary>
    protected virtual void UpdateCircling()
    {
        if (scared)         { state = EnemyState.Retreating; return; }
        if (!PlayerInRange) { state = EnemyState.Idle; return; }

        circleTimer -= Time.deltaTime;
        if (ReadyToAttack) state = EnemyState.Attacking;
    }

    protected virtual void UpdateAttacking()
    {
        if (scared) { state = EnemyState.Retreating; return; }
        // else { EnterIdle(); return; }
    }

    // base behaviour for retreating
    /// <summary>
    /// If scaredTimer has finished, enter idle state
    /// </summary>
    protected virtual void UpdateRetreating()
    {
        scaredTimer -= Time.deltaTime;
        if (scaredTimer <= 0) EnterIdle();
    }

    // base behaviour for entering idle
    /// <summary>
    /// Enter idle state, set idle timer
    /// </summary>
    protected virtual void EnterIdle()
    {
        state = EnemyState.Idle;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        scared = false;
    }












    /* STATE BEHAVIOUR FUNCTIONS */

    /// <summary>
    /// State Idle: move around randomly until spotting the player
    /// </summary>
    protected virtual void Idle()
    {
        if (initialContact == true)
        {
            idleDir = Random.insideUnitCircle.normalized;
            if (distance < activationRange) {initialContact = false;}
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

    /// <summary>
    /// State TooFar: enemy wants to get closer
    /// </summary>
    protected virtual void TooFar()
    {
        body.AddForce(direction * accel, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
        circleDirection = Random.value > 0.5f ? 1f : -1f;
    }

    /// <summary>
    /// State TooClose: enemy wants to move away
    /// </summary>
    protected virtual void TooClose()
    {
        body.AddForce(-direction * accel, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
        circleDirection = Random.value > 0.5f ? 1f : -1f;
    }

    /// <summary>
    /// State Circling: enemy circles the player in a random direction
    /// </summary>
    /// <param name="mult">Multiplier on the speed at which to circle. Value of 1 = circle at moveSpeed speed.</param>
    protected virtual void Circle(float mult)
    {
        Vector2 perp = Vector2.Perpendicular(direction) * circleDirection;

        // tangential accel
        body.AddForce(perp * accel * mult, ForceMode2D.Force);
        // normal accel
        float tanVelocity = Vector2.Dot(body.linearVelocity, perp);
        body.AddForce(direction * Mathf.Pow(tanVelocity, 2) / distance * mult, ForceMode2D.Force);

        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = body.linearVelocity.normalized * moveSpeed;
        }
    }

    /// <summary>
    /// State Aiming: enemy aims at player (ranged enemies only). Currently unused.
    /// </summary>
    protected virtual void Aiming()
    {
        
    }

    /// <summary>
    /// A placeholder Attack() function. Meant to be overridden.
    /// </summary>
    protected virtual void Attack()
    {
        cooldownTimer = attackCooldown;
        EnterIdle();
    }

    /// <summary>
    /// Generic Retreat() function. Runs down scaredTimer while retreating from the player. 
    /// </summary>
    protected virtual void Retreat()
    {
        body.AddForce(-direction * accel * 0.5f, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping * 2f);
        }
        scaredTimer -= Time.deltaTime;

        // Debug.Log("Retreating: " + scaredTimer.ToString());
    }
}
