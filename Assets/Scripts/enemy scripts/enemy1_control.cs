using Unity.VisualScripting;
using UnityEngine;

public class enemy1_control : MonoBehaviour
{
    /* VARIABLES */
    [Header("Stats")]
    public float health = 10f;
    private bool scared = false;
    private float scaredTimer = 0;

    [Header("Timers")]
    public float minCircleTime = 2f;
    public float maxCircleTime = 5f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 5f;
    
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
    public float attackForeswing = 2f;
    public GameObject telegraphPrefab;

    private float cooldownTimer = 0;
    private float foreswingTimer;

    private enum EnemyState {TooFar, TooClose, Idle, Circling, Attacking, Retreating}
    private EnemyState state = EnemyState.Idle;
    private Rigidbody2D body;
    private Transform player;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;

        circleDirection = Random.value > 0.5f ? 1f : -1f;   // if greater than 0.5, 1 (CW), else -1 (CCW)
        circleTimer = Random.Range(minCircleTime, maxCircleTime);

        idleTimer = Random.Range(minIdleTime, maxIdleTime);

        foreswingTimer = attackForeswing;
    }

    // Update is called once per frame
    void Update()
    {
        // StateMachine();

        // Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance((Vector2)player.position, (Vector2)transform.position);

        // switch states if conditions met
        switch (state) {
            // if idle, switch to moving or circling based on distance, or retreating if scared
            // idle acts as a base state
            case EnemyState.Idle:
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0)
                {
                    if (distance > 2f && scared == false)
                    {
                        state = EnemyState.TooFar;
                    }
                    else if (distance < 1f && scared == false)
                    {
                        state = EnemyState.TooClose;
                    }
                    else if (scared == false)
                    {
                        state = EnemyState.Circling;
                    }
                    else
                    {
                        state = EnemyState.Retreating;
                    }
                }
                break;
            case EnemyState.TooFar:
                if (distance <= 2f)
                {
                    state = EnemyState.Idle;
                }
                break;
            case EnemyState.TooClose:
                if (distance >= 1f)
                {
                    state = EnemyState.Idle;
                }
                if (circleTimer > 0)
                {
                    circleTimer -= Time.deltaTime;
                }
                else if (circleTimer <= 0)
                {
                    state = EnemyState.Attacking;
                }
                break;
            case EnemyState.Circling:
                circleTimer -= Time.deltaTime;
                if (distance < 1f || distance > 2f)
                {
                    state = EnemyState.Idle;
                }
                else if (circleTimer <= 0 && scared == false)
                {
                    state = EnemyState.Attacking;
                }
                else if (scared == true)
                {
                    state = EnemyState.Retreating;
                }
                break;
            case EnemyState.Attacking:
                if (scared == true)
                {
                    state = EnemyState.Retreating;
                }
                // else if (distance > 3f)
                // {
                //     state = EnemyState.TooFar;
                // }
                break;
            case EnemyState.Retreating:
                scaredTimer -= Time.deltaTime;
                if (scaredTimer <= 0)
                {
                    state = EnemyState.Idle;
                    idleTimer = Random.Range(minIdleTime, maxIdleTime);
                    scared = false;
                }
                break;
        }
    }

    void FixedUpdate()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance((Vector2)player.position, (Vector2)transform.position);

        // run the corresponding state function
        switch(state)
        {
            case EnemyState.Idle:
                Idle(distance);
                break;
            case EnemyState.TooFar:
                TooFar(direction);
                break;
            case EnemyState.TooClose:
                TooClose(direction);
                break;
            case EnemyState.Circling:
                Circle(direction, distance);
                break;
            case EnemyState.Attacking:
                body.bodyType = RigidbodyType2D.Kinematic;
                Attack(direction);
                break;
            case EnemyState.Retreating:
                body.bodyType = RigidbodyType2D.Dynamic;
                Retreat(direction);
                break;
        }
    }









    /* STATE FUNCTIONS */
    // observe the player
    void Idle(float distance)
    {
        if (distance <= 2)
        {
            idleTimer = 0;
        }
        idleDir = Random.insideUnitCircle.normalized;
        body.AddForce(0.5f * accel * idleDir, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }

        Debug.Log("Idle");
    }

    // if player is >2 units away, pathfind to player
    void TooFar(Vector2 direction)
    {
        body.AddForce(direction * accel, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
        circleDirection = Random.value > 0.5f ? 1f : -1f; // if greater than 0.5, 1 (CW), else -1 (CCW)

        Debug.Log("Too far");
    }

    // if player is <1 unit away, retreat slightly
    void TooClose(Vector2 direction)
    {
        body.AddForce(-direction * accel, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
        circleDirection = Random.value > 0.5f ? 1f : -1f; // if greater than 0.5, 1 (CW), else -1 (CCW)

        Debug.Log("Too close");
    }

    // if player is 1-2 units away, circle with a chance to attack
    void Circle(Vector2 direction, float distance)
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

        // circleTimer -= Time.deltaTime;
        // if (circleTimer <= 0 && cooldownTimer <= 0)
        // {
        //     Attack(direction);
        // }

        Debug.Log("Circling");
    }



    
    /* ACTION FUNCTIONS */
    // Attack (ram the player)
    void Attack(Vector2 direction)
    {

        if (body.linearVelocity.magnitude > 0.1)
        {
            
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping * 3);
        }
        else if (foreswingTimer > 0) 
        {
            
            foreswingTimer -= Time.deltaTime;
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping * 3);
        }
        else
        {
            body.bodyType = RigidbodyType2D.Dynamic;
            body.AddForce(direction * 25f, ForceMode2D.Impulse);
            circleTimer = Random.Range(minCircleTime, maxCircleTime);
            cooldownTimer = attackCooldown;
            foreswingTimer = attackForeswing;
            scared = true;
            scaredTimer = 1f;
        }

        Debug.Log("Attacking");
    }

    // if just attacked or took significant damage, retreat a short distance
    void Retreat(Vector2 direction)
    {
        body.AddForce(-direction * accel * 0.5f, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
        scaredTimer -= Time.deltaTime;

        Debug.Log("Retreating");
    }

}
