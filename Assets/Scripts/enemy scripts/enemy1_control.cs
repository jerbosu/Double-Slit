using UnityEngine;

public class enemy1_control : MonoBehaviour
{
    /* VARIABLES */
    [Header("Stats")]
    public float health = 10f;
    private float lastTakenDamage = 0f;
    private bool scared = false;

    //[Header("Circling")]
    private float circleDirection; // if greater than 0.5, 1 (CW), else -1 (CCW)


    [Header("Movement")]
    public float moveSpeed = 3f;
    public float accel = 10f;
    public float damping = 1f;

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 10f;
    public GameObject attackPrefab;

    private Rigidbody2D body;
    private Transform player;
    private float lastAttackTime = -Mathf.Infinity;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
        circleDirection = Random.value > 0.5f ? 1f : -1f;
    }

    // Update is called once per frame
    void Update()
    {
        StateMachine();
    }









    /* STATE FUNCTIONS */
    // state machine function (determines which state to be in)
    void StateMachine()
    {
        // get direction and distance to player
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance((Vector2)player.position, (Vector2)transform.position);

        if (distance > 2f)
        {
            TooFar(direction);
        }
        else if (distance < 1f)
        {
            TooClose(direction);
        }
        else if (scared == false)
        {
            Circle(direction, distance);
        }

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
    }

    // if player is 1-2 units away, circle with a chance to attack
    void Circle(Vector2 direction, float distance)
    {
        Vector2 perp = Vector2.Perpendicular(direction) * circleDirection;

        // circular motion calcs :suffering:
        //float centripetalAccel = Mathf.Pow(body.linearVelocity.magnitude, 2) / distance;

        body.AddForce(perp * accel, ForceMode2D.Force);

        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
    }

    // if just attacked or took significant damage, retreat a short distance
    void Retreat(Vector2 direction)
    {
        
    }


    
    /* ACTION FUNCTIONS */
    // 
    void Attack(Vector2 direction)
    {
        
    }
}
