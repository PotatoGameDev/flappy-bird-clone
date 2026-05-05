using UnityEngine;

public class SpinTransform : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond;

    void Update()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
    }
}
