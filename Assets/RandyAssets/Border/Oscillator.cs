using UnityEngine;

public class Oscillator : MonoBehaviour
{
    private float amplitude; // Maximum height of oscillation
    private float speed; // The actual speed for this object
    private float initialY; // Store the initial Y position

    public void Initialize(float amplitude, float baseSpeed, bool randomizeSpeed)
    {
        this.amplitude = amplitude;
        this.speed = randomizeSpeed ? baseSpeed * Random.Range(0.5f, 1.5f) : baseSpeed;
        this.initialY = transform.position.y; // Record the starting Y position
    }

    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newY = initialY + Mathf.Sin(Time.time * speed) * amplitude;

        // Update the object's position
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}