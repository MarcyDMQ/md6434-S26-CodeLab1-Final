using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // the ball that the camera follows
    public Vector3 offset = new Vector3(0, 7, -7); // camera offset
    public float smoothTime = 0.3f; 
    
    private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate() // update camera position after ball moves
    {
        if (target != null)
        {
            // calculate camera position
            Vector3 targetPosition = target.position + offset;
            
            // use SmoothDamp to make smooth following
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
            
            // look at the ball at all times
            transform.LookAt(target.position);
        }
    }
}