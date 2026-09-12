using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float health = 10f;
    public float moveSpeed = 5f;
    public float accel = 20f;
    public float damping = 10f;


    [Header("Timers")]
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;
    
    private float idleTimer;
    private Vector2 idleDir;
    private bool scared = false;
    private float scaredTimer = 0;
    private float circleDirection;
    private bool initialContact = true;

    protected Rigidbody2D body;             // enemy rigidbody
    protected Transform playerTransform;    // player position
    protected float distance;               // distance from enemy to player
    protected Vector2 direction;            // direction from enemy to player


/* UNITY FUNCTIONS */

    protected virtual void Start()
    {
        body = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        distance = Vector2.Distance(transform.position, playerTransform.position);
        direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
    }










/* USER FUNCTIONS */

    /// <summary>
    /// State Idle: move around randomly until spotting the player
    /// </summary>
    /// <param name="activationRange">How close the player can get before being spotted.</param>
    protected virtual void Idle(float activationRange)
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
    /// <param name="maxDist">The enemy's preferred max distance from the player.</param>
    protected virtual void TooFar(float maxDist)
    {
        if (distance < maxDist) return;     // redundant?
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
    /// <param name="minDist">The enemy's preferred min distance from the player.</param>
    protected virtual void TooClose (float minDist)
    {
        if (distance > minDist) return;
        body.AddForce(-direction * accel, ForceMode2D.Force);
        if (body.linearVelocity.magnitude > moveSpeed)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping);
        }
        circleDirection = Random.value > 0.5f ? 1f : -1f;
    }

    /// <summary>
    /// State Circling: enemy circles the player in a random direction (for melee enemies only)
    /// </summary>
    protected virtual void Circle()
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
    }

    /// <summary>
    /// State Aiming: enemy aims at player (ranged enemies only)
    /// </summary>
    protected virtual void Aiming()
    {
        
    }
}
