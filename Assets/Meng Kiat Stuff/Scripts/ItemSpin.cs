using UnityEngine;

public class ItemFloat : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;  // Speed of rotation
    [SerializeField] private float floatSpeed = 1f;      // Speed of floating
    [SerializeField] private float floatHeight = 0.2f;   // Max height offset

    private Vector3 startPos;
    private float floatTimer;

    void Start()
    {
        startPos = transform.position;
        floatTimer = Random.Range(0f, Mathf.PI * 2f);  // Randomize start offset
    }

    void Update()
    {
        // Rotate the item
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Floating effect (only moves upwards from startPos)
        float newY = startPos.y + (Mathf.Abs(Mathf.Sin(floatTimer)) * floatHeight);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Increase time for the sine wave
        floatTimer += Time.deltaTime * floatSpeed;
    }
}
