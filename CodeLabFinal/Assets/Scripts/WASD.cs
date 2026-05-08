using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody))]

public class WASD : MonoBehaviour
{
    public float moveForce = 15f;    
    public float maxVelocity = 8f; 
    
    private Rigidbody rb;
    private float ballRadius;    
    public float acceleration = 10f;
    // variables that controls speed boost and speed down
    public float boostMultiplier = 2f; 
    public float lossMultiplier = 0.5f;   
    public float boostDuration = 3f;      
    private float originalMaxVelocity;    
    private float originalMoveForce; 
    private bool isBoosting = false;
    private bool isSlowing = false;
    
    public Key keyUp = Key.W;     
    public Key keyDown = Key.S;   
    public Key keyLeft = Key.A;   
    public Key keyRight = Key.D;  

    Keyboard keyboard = Keyboard.current;//get the keyboard input for this device

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalMaxVelocity = maxVelocity;
        // get the radius and apply correct rolling
        SphereCollider sc = GetComponent<SphereCollider>();
        ballRadius = (sc != null) ? sc.radius * transform.localScale.y : 0.5f;
    }

    void FixedUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        // get the input vectoe
        Vector3 moveInput = Vector3.zero;
        if (keyboard[keyUp].isPressed) moveInput += Vector3.forward;
        if (keyboard[keyDown].isPressed) moveInput += Vector3.back;
        if (keyboard[keyLeft].isPressed) moveInput += Vector3.left;
        if (keyboard[keyRight].isPressed) moveInput += Vector3.right;
        moveInput = moveInput.normalized;
        // give a velocity to this vector
        Vector3 targetVelocity = moveInput * maxVelocity;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        // make the ball rolls naturally
        // (the planets were able to be attached to player but we deleted this factor anyways)
        ApplyNaturalRolling();
    }

    void ApplyNaturalRolling()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0;

        if (currentVelocity.magnitude < 0.01f) return;

        float distanceThisFrame = currentVelocity.magnitude * Time.fixedDeltaTime;
        float rotationAngle = (distanceThisFrame / ballRadius) * Mathf.Rad2Deg;

        Vector3 rotationAxis = Vector3.Cross(Vector3.up, currentVelocity.normalized);
        transform.Rotate(rotationAxis, rotationAngle, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("point"))
        {
            // change tag after trigger to prevent multiple trigger
            other.gameObject.tag = "Untagged";
            // disable collider too to prevent multiple trigger
            other.enabled = false;
            // play audio when trigger happens
            AudioSource audio = other.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }
            // get score
            if (GameManager.instance != null)
            {
                GameManager.instance.incrementScore();
            }
            // start shrinking process
            StartCoroutine(ShrinkAndDisable(other.gameObject));
        }
        if (other.CompareTag("speed"))
        {
            // change tag after trigger to prevent multiple trigger
            other.gameObject.tag = "Untagged";
            // disable collider too to prevent multiple trigger
            other.enabled = false;
            // speed boost logic
            StartCoroutine(SpeedBoost());
            // play audio when trigger happens
            AudioSource audio = other.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }
            // start shrinking process
            StartCoroutine(ShrinkAndDisable(other.gameObject));
        }

        if (other.CompareTag("enemy"))
        {
            // change tag after trigger to prevent multiple trigger
            other.gameObject.tag = "Untagged";
            // disable collider too to prevent multiple trigger
            other.enabled = false;
            // speed down logic
            StartCoroutine(SpeedLoss());
            // play audio when trigger happens
            AudioSource audio = other.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }
            // minus scores
            if (GameManager.instance != null)
            {
                GameManager.instance.decrementScore();
            }
            // start shrinking process
            StartCoroutine(ShrinkAndDisable(other.gameObject));
        }
    }
    private System.Collections.IEnumerator SpeedBoost()
    {
        if (isBoosting) yield break; // avoid multiple speed boost

        isBoosting = true;
        maxVelocity = originalMaxVelocity * boostMultiplier; 

        yield return new WaitForSeconds(boostDuration); 

        maxVelocity = originalMaxVelocity;
        isBoosting = false;
    }
    private System.Collections.IEnumerator SpeedLoss()
    {
        if (isSlowing) yield break; // avoid multiple speed down

        isSlowing = true;
        maxVelocity = originalMaxVelocity * lossMultiplier; 

        yield return new WaitForSeconds(boostDuration); 

        maxVelocity = originalMaxVelocity;
        isSlowing = false;
    }
    private System.Collections.IEnumerator ShrinkAndDisable(GameObject target)
    {
        float duration = 0.3f; 
        float currentTime = 0f;
    
        // check if the object exist at first 安全检查：如果开始时物体就没了，直接退出
        if (target == null) yield break;
        // shrink transform
        Vector3 startScale = target.transform.localScale;
        Vector3 endScale = Vector3.zero;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;

            // check if the object exist through time
            if (target == null) yield break; 
            // if so do the actual shrink process
            target.transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
    
            yield return null; 
        }

        // final check, and set false after the shrinking
        if (target != null)
        {
            target.SetActive(false);
        }
    }
}