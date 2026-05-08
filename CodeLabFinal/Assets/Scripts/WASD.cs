using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody))]

public class WASD : MonoBehaviour
{
    public float moveForce = 15f;    
    public float maxVelocity = 8f; 
    
    private Rigidbody rb;
    private float ballRadius;    
    // 转向平滑度（数值越高，变向越快）
    public float acceleration = 10f;
    // 加速相关变量
    public float boostMultiplier = 2f; 
    public float lossMultiplier = 0.5f;   
    public float boostDuration = 3f;      
    private float originalMaxVelocity;    
    private float originalMoveForce; 
    private bool isBoosting = false;
    private bool isSlowing = false;
    
    public Key keyUp = Key.W;     // 前
    public Key keyDown = Key.S;   // 后
    public Key keyLeft = Key.A;   // 左
    public Key keyRight = Key.D;  // 右

    Keyboard keyboard = Keyboard.current;//get the keyboard input for this device

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalMaxVelocity = maxVelocity;
        // 自动获取半径，用于滚动表现
        SphereCollider sc = GetComponent<SphereCollider>();
        ballRadius = (sc != null) ? sc.radius * transform.localScale.y : 0.5f;
    }

    void FixedUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        // 获取输入向量
        Vector3 moveInput = Vector3.zero;
        if (keyboard[keyUp].isPressed) moveInput += Vector3.forward;
        if (keyboard[keyDown].isPressed) moveInput += Vector3.back;
        if (keyboard[keyLeft].isPressed) moveInput += Vector3.left;
        if (keyboard[keyRight].isPressed) moveInput += Vector3.right;
        moveInput = moveInput.normalized;
        // 直接计算并赋予目标速度
        Vector3 targetVelocity = moveInput * maxVelocity;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        // 处理滚动动画
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
            // 立即修改标签，防止在缩小过程中被重复触发
            other.gameObject.tag = "Untagged";
            // 禁用 Collider，防止在缩小过程中继续发生物理碰撞
            other.enabled = false;
            // (如有)播放声音
            AudioSource audio = other.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }
            // 加分
            if (GameManager.instance != null)
            {
                GameManager.instance.incrementScore();
            }
            // 开启“慢慢缩小”的协程
            StartCoroutine(ShrinkAndDisable(other.gameObject));
        }
        if (other.CompareTag("speed"))
        {
            // 立即修改标签，防止在缩小过程中被重复触发
            other.gameObject.tag = "Untagged";
            // 禁用 Collider，防止在缩小过程中继续发生物理碰撞
            other.enabled = false;
            // 加速逻辑
            StartCoroutine(SpeedBoost());
            // (如有)播放声音
            AudioSource audio = other.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }
            // 开启“慢慢缩小”的协程
            StartCoroutine(ShrinkAndDisable(other.gameObject));
        }

        if (other.CompareTag("enemy"))
        {
            // 立即修改标签，防止在缩小过程中被重复触发
            other.gameObject.tag = "Untagged";
            // 禁用 Collider，防止在缩小过程中继续发生物理碰撞
            other.enabled = false;
            // 减速
            StartCoroutine(SpeedLoss());
            // (如有)播放声音
            AudioSource audio = other.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }
            // 减分
            if (GameManager.instance != null)
            {
                GameManager.instance.decrementScore();
            }
            // 开启“慢慢缩小”的协程
            StartCoroutine(ShrinkAndDisable(other.gameObject));
        }
    }
    private System.Collections.IEnumerator SpeedBoost()
    {
        if (isBoosting) yield break; // 防止重复叠加

        isBoosting = true;
        maxVelocity = originalMaxVelocity * boostMultiplier; 

        yield return new WaitForSeconds(boostDuration); 

        maxVelocity = originalMaxVelocity;
        isBoosting = false;
    }
    private System.Collections.IEnumerator SpeedLoss()
    {
        if (isSlowing) yield break; // 防止重复叠加

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
    
        // 安全检查：如果开始时物体就没了，直接退出
        if (target == null) yield break;

        Vector3 startScale = target.transform.localScale;
        Vector3 endScale = Vector3.zero;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;

            // 【关键修复】：每一帧操作前都检查物体是否还活着
            if (target == null) yield break; 

            target.transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
    
            yield return null; 
        }

        // 最后检查一次
        if (target != null)
        {
            target.SetActive(false);
        }
    }
}