using UnityEngine;

// Set a trigger named "blink" in the animator controller
public class RandomEyeBlink : MonoBehaviour
{
    [Range(0.1f, 10f)]
    public float timeBetweenBlink;

    [Range(0.0f, 10f)]
    public float maxTimeOffset;

    private Animator animator;
    private float nextBlinkTime = 0f;

    private void Update()
    {
        if (Time.time > nextBlinkTime)
        {
            animator.SetTrigger("Blink");
            nextBlinkTime = Time.time + timeBetweenBlink + Random.Range(0f, maxTimeOffset);
        }
    }

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }
}