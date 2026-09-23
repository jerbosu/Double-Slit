using UnityEngine;
using System.Collections;

public class enemy1_control : BaseEnemy
{
    /* VARIABLES */
    [Header("Attack")]

    public GameObject telegraphPrefab;          // telegraph for the attack AoE
    public GameObject attackPrefab;             // placeholder prefab for the attack hitbox
    // public GameObject deathParticlePrefab;
    private SpriteRenderer telegraphFlash;      // telegraph for the attack timing flash
    private Parryable parryWindow;              // parry obj to track whether enemy is in parry state
    private Coroutine telegraphCoroutine;       // track the telegraph coroutine
    private Coroutine brakeCoroutine;           // apply braking after the attack finishes
    private GameObject activeTelegraph;         // the current active attack telegraph
    private GameObject attack;                  // stores the attackPrefab instantiation

    private Vector2 aimDirection;           // aiming direction
    private float aimLockTime = 0.3f;       // how long before the enemy attack should their aim be locked for
    private bool aimLocked = false;         // whether enemy aim is locked
    private Vector2 attackStartPos;         // start pos of attack, brake after traveling attackRange distance
    private Vector2 futurePos;              // predicted player future location

    private float foreswingTimer;

    /* STATE MACHINE VARIABLES */
    protected override bool PlayerTooFar => distance > attackRange * 0.9f;

    private Rigidbody2D playerBody;
    private Hurtbox hurtbox;
    private SpriteRenderer sprite;

    void Awake()
    {
        hurtbox = GetComponentInChildren<Hurtbox>();
        hurtbox.maxHealth = health;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        // BaseEnemy init
        base.Start();

        playerBody = playerTransform.GetComponent<Rigidbody2D>();
        telegraphFlash = transform.Find("telegraph_0").GetComponent<SpriteRenderer>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        
        parryWindow = GetComponent<Parryable>();

        // behaviour init
        circleDirection = Random.value > 0.5f ? 1f : -1f;   // if greater than 0.5, 1 (CW), else -1 (CCW)
        circleTimer = Random.Range(minCircleTime, maxCircleTime);
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        foreswingTimer = attackForeswing;

        GetComponentInChildren<Hurtbox>().onDeathWithInfo += Die;
        GetComponent<Parryable>().onParried += OnParried;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();  // calculates distance and direction

        // Debug.Log("Parryable: " + parryWindow.isParryable);
    }

    protected override void FixedUpdate()
    {
        cooldownTimer -= Time.deltaTime;
        base.SwitchState();
    }









    /* STATE UPDATE FUNCTIONS */
    protected override void EnterIdle()
    {
        base.EnterIdle();

        if (brakeCoroutine != null)
        {
            StopCoroutine(brakeCoroutine);
            brakeCoroutine = null;
        }
    }

    /* ACTION FUNCTIONS */
    // Attack (ram the player)
    protected override void Attack()
    {
        // if player moves out of range, clean up
        if (distance > attackRange && foreswingTimer > aimLockTime) 
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

            if (brakeCoroutine != null)
            {
                StopCoroutine(brakeCoroutine);
                brakeCoroutine = null;
            }
            
            return; 
        }

        if (foreswingTimer > 0)    // attack telegraph, etc.
        {
            if (telegraphCoroutine == null)
            {
                telegraphCoroutine = StartCoroutine(TelegraphAoE());
            }
            foreswingTimer -= Time.deltaTime;
            if (foreswingTimer > aimLockTime)  // freeze aim direction aimLockTime seconds before attacking
            {
                // predict player's future location
                futurePos = (Vector2)playerTransform.position + playerBody.linearVelocity * aimLockTime;
            }
            else
            {
                // aimDirection = (futurePos - (Vector2)transform.position).normalized;
                if (aimLocked == false)
                {
                    aimDirection = (futurePos - (Vector2)transform.position).normalized;
                    aimLocked = true;
                }
                telegraphFlash.color = new Color(1f, 0f, 0f, foreswingTimer / aimLockTime);
                telegraphFlash.transform.localScale = telegraphFlash.transform.localScale * 0.99f;
                parryWindow.isParryable = true;
                
            }
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping * 5);
        }
        else
        {
            attackStartPos = body.position;                                 // get the attack starting pos
            body.bodyType = RigidbodyType2D.Dynamic;                        // kinematic --> dynamic
            telegraphFlash.color = new Color(1f, 0f, 0f, 0f);               // make the flash invisible again
            telegraphFlash.transform.localScale = new Vector3(1f, 1f, 1f);  // idk
            attack = Instantiate(attackPrefab, transform.position, transform.rotation, transform);
            Hitbox hitbox = attack.GetComponentInChildren<Hitbox>();        // get the attack's hitbox
            hitbox.attackerHurtbox = GetComponentInChildren<Hurtbox>();     // get the attacker's hurtbox
            Destroy(attack, 0.2f);
            body.AddForce(aimDirection * 36f, ForceMode2D.Impulse);         // enemy ram attack
            circleTimer = Random.Range(minCircleTime, maxCircleTime);       // set timer
            cooldownTimer = attackCooldown;                                 // set timer
            foreswingTimer = attackForeswing;                               // reset foreswing
            aimLocked = false;
            // futurePos = Vector2.zero;

            // after travelling attackRange distance, damp strongly
            if (brakeCoroutine == null)
            {
                brakeCoroutine = StartCoroutine(Brake());
            }
            scared = true;  
            scaredTimer = attackBackswing;
        }

        // Debug.Log("Attacking");
    }

    // Attack AoE telegraph
    IEnumerator TelegraphAoE()
    {
        
        // aim telegraph toward player
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        activeTelegraph = Instantiate(telegraphPrefab, transform.position, rotation, transform);
        AttackTelegraph telegraph = activeTelegraph.GetComponent<AttackTelegraph>();
        telegraph.duration = foreswingTimer;
        telegraph.size = new Vector2(0.3f, attackRange);
        telegraph.lockTime = aimLockTime;
        telegraph.Init(transform, playerTransform);



        while (aimLocked == false)
        {
            telegraph.SetTargetOverride(futurePos);
            yield return null;
        }
        // yield return new WaitForSeconds(foreswingTimer - aimLockTime - 0.05f);

        // telegraph.SetTargetOverride(futurePos);

        yield return new WaitForSeconds(aimLockTime);

        Destroy(activeTelegraph);
        telegraphCoroutine = null;
    }

    // if just attacked or got parried, lerp to zero ("stun")
    protected override void Retreat()
    {
        // body.AddForce(-direction * accel * 0.5f, ForceMode2D.Force);
        // if (body.linearVelocity.magnitude > moveSpeed)
        // {
        body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping * 2f);
        // }
        scaredTimer -= Time.deltaTime;

        // Debug.Log("Retreating: " + scaredTimer.ToString());
    }

    IEnumerator Brake()
    {
        // wait until enemy has traveled far enough
        while ((body.position - attackStartPos).magnitude < attackRange)
            yield return new WaitForFixedUpdate();

        // now brake
        parryWindow.isParryable = false;
        Destroy(attack);

        while (body.linearVelocity.magnitude > 0.1f)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * damping * 5f);
            yield return new WaitForFixedUpdate();
        }
    }





    /* EVENT FUNCTIONS */

    void OnParried()
    {
        // variable cleanup
        // "stun" enemy and set to scared
        body.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        parryWindow.isParryable = false;
        state = EnemyState.Retreating; 
        body.bodyType = RigidbodyType2D.Dynamic;
        foreswingTimer = attackForeswing;   // reset foreswing timer
        
        telegraphFlash.color = new Color(1f, 0f, 0f, 0f);               // make the flash invisible again
        telegraphFlash.transform.localScale = new Vector3(1f, 1f, 1f);  // idk
        circleTimer = Random.Range(minCircleTime, maxCircleTime);       // set timer
        scared = true;  
        scaredTimer = attackBackswing;

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

        if (brakeCoroutine != null)
        {
            StopCoroutine(brakeCoroutine);
            brakeCoroutine = null;
        }
    }

    protected override void Die(float damage)
    {
        base.Die(damage);


        // // death particle effect here
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;

        // GameObject deathParticles = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        // ParticleSystem ps = deathParticles.GetComponent<ParticleSystem>();
        
        // deathParticles.transform.rotation = Quaternion.Euler(0f, 0f, angle - ps.shape.arc / 2);

        // var main = ps.main;
        // main.startSpeed = new ParticleSystem.MinMaxCurve(damage * 1f, damage * 2f);
        // ps.Emit(12);

        // Destroy(deathParticles, 1f);

        // // death handling
        // CameraFollow.Instance.Shake(4f, 0.1f);
        // Destroy(gameObject, 0.01f);
    }

}
