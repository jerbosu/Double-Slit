using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class enemy2_control : BaseEnemy
{
    /* VARIABLE OVERRIDES */

    /* CUSTOM VARIABLES */
    public GameObject projectile_prefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        tooFar = 4f;
        tooClose = 2f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        // Debug.Log("Enemy2 State: " + state);
    }














    /* STATE MACHINE FUNCTIONS */
    protected override void Circle(float mult)
    {
        base.Circle(0.5f);
    }

    protected override void Attack()
    {
        GameObject projectile = Instantiate(projectile_prefab, transform.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().Init(gameObject);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.AddForce(10f * direction, ForceMode2D.Impulse);
        // Destroy(projectile, 10f);
        base.Attack();
    }
}
