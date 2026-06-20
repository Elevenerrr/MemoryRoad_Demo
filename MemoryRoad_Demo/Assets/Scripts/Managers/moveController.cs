using UnityEngine;

public class moveController : MonoBehaviour
{
    [Tooltip("移动速度")]
    public float moveSpeed = 3f;

    [Tooltip("转向平滑度")]
    public float rotationSpeed = 10f;

    PlayerMove inputs;
    Animator animator;
    Rigidbody body;
    Camera mainCamera;

    Vector3 moveDirection;
    Vector3 moveVelocity;
    bool isMovingPrev;

    void Awake()
    {
        inputs = new PlayerMove();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        if (body != null)
        {
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void OnEnable() => inputs.Player.Enable();
    void OnDisable() => inputs.Player.Disable();

    void Update()
    {
        // 传送期间冻结移动输入，防止漂移
        if (Jump.IsTransitioning) return;

        if (!mainCamera) { mainCamera = Camera.main; return; }

        // 读取 WASD 输入
        Vector2 input = inputs.Player.Move.ReadValue<Vector2>();

        // 转换到世界坐标系（相机朝向）
        Vector3 forward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(mainCamera.transform.right, new Vector3(1, 0, 1)).normalized;
        moveDirection = (forward * input.y + right * input.x).normalized;
        moveVelocity = moveDirection * moveSpeed;

        // 面向移动方向（仅旋转Y轴，不影响FacingCamera）
        if (moveDirection.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), Time.deltaTime * rotationSpeed);

        // 动画切换：移动→walking / 静止→idle
        bool isMoving = moveDirection.sqrMagnitude > 0.01f;
        if (isMoving != isMovingPrev && animator != null)
        {
            if (isMoving) animator.CrossFade("_walking", 0.15f);
            else animator.CrossFade("Idle", 0.15f);
            isMovingPrev = isMoving;
        }
    }

    void FixedUpdate()
    {
        // 传送期间完全冻结物理移动
        if (Jump.IsTransitioning)
        {
            if (body != null) body.velocity = Vector3.zero;
            return;
        }

        if (body != null)
            body.velocity = new Vector3(moveVelocity.x, body.velocity.y, moveVelocity.z);
    }
}
