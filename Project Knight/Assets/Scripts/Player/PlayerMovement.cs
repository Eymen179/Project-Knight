using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 _moveDirection;
    private Rigidbody rb;
    private Animator _animator;

    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference run;

    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    //Kamera ve Rotasyon
    private Transform mainCameraTransform;
    [SerializeField] private float turnSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private InputActionReference jump;
    [SerializeField] private float jumpForce = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    //Kilic Durumu
    private bool isEquipped;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;
    }
    void Update()
    {
        CheckGroundStatus(); //Yer Kontrolcusu
        HandleInput();       //Tus Basma Kontrolcusu
        HandleJump();        //Ziplama Kontrolcusu
        UpdateAnimations();  //Animasyon Kontrolcusu
    }

    void FixedUpdate()
    {
        ApplyMovement();     //Hareket
        ApplyRotation();     //Donus
    }


    //Yer Kontrolcu Metodu
    private void CheckGroundStatus()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            _animator.SetTrigger("land");
        }
    }

    //Tus Basma Kontrolcu Metodu
    private void HandleInput()
    {
        _moveDirection = move.action.ReadValue<Vector2>();
    }

    //Ziplama Kontrolcu Metodu
    private void HandleJump()
    {
        if (jump.action.WasPressedThisFrame() && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (isEquipped)
                _animator.SetTrigger("jumpWithSword");
            else
                _animator.SetTrigger("jump");
        }
    }
    
    //Animasyon Kontrolcu Metodu
    private void UpdateAnimations()
    {
        _animator.SetBool("isGrounded", isGrounded);

        float inputMagnitude = _moveDirection.magnitude;
        bool isRunning = run.action.IsPressed();
        float animationSpeed = inputMagnitude * (isRunning ? 1f : 0.5f);

        _animator.SetFloat("speed", animationSpeed);
    }
    
    //Hareket Metodu
    private void ApplyMovement()
    {
        if (_moveDirection == Vector2.zero)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        //Kosma - Yurume Kontrolcusu
        bool isRunning = run.action.IsPressed();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        //Kamera Yonu Ayarlari
        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * _moveDirection.y + camRight * _moveDirection.x).normalized;
        Vector3 targetVelocity = moveDirection * currentSpeed;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    //Donus Metodu
    private void ApplyRotation()
    {
        if (_moveDirection == Vector2.zero) return;

        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * _moveDirection.y + camRight * _moveDirection.x).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
    }

    //Hareket Animasyonunu Kilicli mi Kilicsiz mi gosterecegimizi belirleyen metot
    public void SetEquippedState(bool state)
    {
        isEquipped = state;

        if (isEquipped)
        {
            _animator.SetTrigger("getSword");
        }
        else
        {
            _animator.SetTrigger("dropSword");
        }
    }
}