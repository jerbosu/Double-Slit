using UnityEngine;



public class slash_prefab : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    public void Init(Transform playerTransform)
    {
        target = playerTransform;
        offset = transform.position - playerTransform.position;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        transform.position = target.position + offset;
    }
}
