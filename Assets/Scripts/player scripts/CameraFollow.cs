using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{

    public Transform target;            // follow target
    public float smoothSpeed = 5f;      // how smoothly camera responds to target accel changes
    public Vector3 offset = new Vector3(0f, 0f, -10f);  // z value is camera zoom

    // zoom vars
    public float zoomSpeed = 0.01f;
    public float minZoom = 1f;
    public float maxZoom = 10f;

    // screenshake vars
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;
    private Vector3 shakeOffset;
    public static CameraFollow Instance;


    private Camera cam;
    private PlayerInputActions inputActions;









    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float scroll = inputActions.Player.Zoom.ReadValue<Vector2>().y * 20f;
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    void FixedUpdate()
    {
        Vector3 targetPos = target.position + offset + shakeOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }

    void LateUpdate()
    {
        
    }














    /* USER FUNCTIONS */
    public void Shake(float intensity, float duration)
    {
        StartCoroutine(DoShake(intensity, duration));
    }

    IEnumerator DoShake(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = 1f - elapsed / duration; // fades out over duration
            shakeOffset = Random.insideUnitCircle * intensity * progress;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }

}
