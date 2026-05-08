using UnityEngine;

public class audioPlayer : MonoBehaviour
{

    AudioSource thisSource;
    public AudioClip[] clips;
    public string keypress_1 = "A";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKey(KeyCode.A))
        {
            // Move backwards relative to the ball's current direction
            thisSource.clip = clips[0];
            thisSource.Play();
        }

        if (Input.GetKey(KeyCode.S))
        {
            // Move backwards relative to the ball's current direction
            thisSource.clip = clips[1];
            thisSource.Play();
        }

        if (Input.GetKey(KeyCode.D))
        {
            // Move backwards relative to the ball's current direction
            thisSource.clip = clips[2];
            thisSource.Play();
        }

        if (Input.GetKey(KeyCode.F))
        {
            // Move backwards relative to the ball's current direction
            thisSource.clip = clips[3];
            thisSource.Play();
        }
    }
}
