using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // 要跟随的小球
    public Vector3 offset = new Vector3(0, 7, -7); // 相机相对于球的偏移量
    public float smoothTime = 0.3f; // 跟随的平滑时间
    
    private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate() // LateUpdate 确保在球移动后更新相机位置
    {
        if (target != null)
        {
            // 计算目标相机位置
            Vector3 targetPosition = target.position + offset;
            
            // 使用 SmoothDamp 实现丝滑跟随
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
            
            // 始终盯着球看
            transform.LookAt(target.position);
        }
    }
}