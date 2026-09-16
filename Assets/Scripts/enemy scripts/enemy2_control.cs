using UnityEngine;
using System.Collections;

public class enemy2_control : BaseEnemy
{
    /* VARIABLE OVERRIDES */
    


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
        Debug.Log("Enemy2 State: " + state);
    }














    /* STATE MACHINE FUNCTIONS */
    protected override void Circle(float mult)
    {
        base.Circle(0.5f);
    }
}
