using UnityEngine;

public class Parryable : MonoBehaviour
{
    public bool isParryable = false;
    public System.Action onParried;

    public void Parry()
    {
        if (isParryable)
            onParried?.Invoke();
    }
}
