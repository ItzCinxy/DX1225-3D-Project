using UnityEngine;

public class FloatingWeapon : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float rotationSpeed = 50f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Floating up and down effect
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * 0.2f, 0);

        // Slow rotation
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}