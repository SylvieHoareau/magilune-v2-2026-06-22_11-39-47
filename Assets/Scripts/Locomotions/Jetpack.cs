using UnityEngine;

public class Jetpack : MonoBehaviour
{
    [SerializeField] private float thrustForce = 8f;
    [SerializeField] private float maxFuel = 100f;
    private float currentFuel;
    private bool isActive;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentFuel = maxFuel;
    }

    public void Activate(bool state) => isActive = state;

    private void FixedUpdate()
    {
        if (isActive && currentFuel > 0)
        {
            rb.AddForce(Vector2.up * thrustForce);
            currentFuel -= Time.fixedDeltaTime * 10f;
        }
    }
}
