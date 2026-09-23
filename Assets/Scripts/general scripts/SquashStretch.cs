using UnityEngine;

public class ApplySquashStretch : MonoBehaviour
{
    [Header("Effect Strength")]
    public float squashStretchAmount = 0.4f;         // squash/stretch magnitude
    public float squashStretchSpeed = 100f;         // squash/stretch speed
    public float maxSpeed = 14f;

    private Rigidbody2D body;
    private Transform visual;
    private Vector3 targetScale;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponentInParent<Rigidbody2D>();
        visual = transform;
    }

    // Update is called once per frame
    void Update()
    {
        Apply();
    }




    /* NON-UNITY FUNCTIONS */
    void Apply()
    {
        Vector2 velocity = body.linearVelocity;
        float speed = velocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(speed / maxSpeed);

        if (speed > 0.1f)
        {
            float stretch = 1f + squashStretchAmount * normalizedSpeed;
            float squash = 1f / stretch;

            targetScale = new Vector3(squash, stretch, 1f);

            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            visual.rotation = Quaternion.Lerp(
                visual.rotation,
                Quaternion.Euler(0f, 0f, angle - 90f),
                Time.deltaTime * squashStretchSpeed
            );
        }
        else
        {
            targetScale = Vector3.one;
            // visual.rotation = Quaternion.Lerp(
            //     visual.rotation,
            //     Quaternion.identity,
            //     Time.deltaTime * squashStretchSpeed
            // );
        }

        visual.localScale = Vector3.Lerp(
            visual.localScale,
            targetScale,
            Time.deltaTime * squashStretchSpeed
        );
    }

}
