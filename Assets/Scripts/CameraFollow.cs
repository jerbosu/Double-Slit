using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform target;            // follow target
    public float smoothSpeed = 5f;      // how smoothly camera responds to target accel changes
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }

    void LateUpdate()
    {
        
    }
}
