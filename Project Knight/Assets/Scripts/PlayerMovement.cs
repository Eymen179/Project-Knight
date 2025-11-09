using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Orijinal Değişkenler
    public Vector2 _moveDirection;
    private Rigidbody rb;
    private Animator _animator;

    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference run;

    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    // Kamera ve Rotasyon
    private Transform mainCameraTransform;
    [SerializeField] private float turnSpeed = 10f;

    // --- Zıplama Değişkenleri ---
    [Header("Jump")]
    [SerializeField] private InputActionReference jump;
    [SerializeField] private float jumpForce = 5f;

    // --- Yer Kontrolü Değişkenleri ---
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    // --- Kılıç Durumu ---
    // Bu değişkeni EquipmentManager güncelleyecek
    private bool isEquipped;

    // --- SİLİNDİ: Bu script artık "equip" input'unu dinlemeyecek ---
    // [SerializeField] private InputActionReference equip;
    // private int pressCounter = 0;
    // public GameObject sword;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;
    }

    void Update() // Input ve Animasyon
    {
        // --- SİLİNDİ: Kılıç kuşanma input'u buradan kaldırıldı ---
        /*
        if (equip.action.WasPressedThisFrame())
        {
            // ... (tüm eski kod silindi) ...
        }
        */

        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        _animator.SetBool("isGrounded", isGrounded);

        if (isGrounded && !wasGrounded)
        {
            _animator.SetTrigger("land");
        }

        _moveDirection = move.action.ReadValue<Vector2>();

        if (jump.action.WasPressedThisFrame() && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // --- GÜNCELLENDİ: Sadece 'isEquipped' durumunu kontrol eder ---
            if (isEquipped) // Bu değişken artık EquipmentManager tarafından ayarlanacak
            {
                _animator.SetTrigger("jumpWithSword");
            }
            else
            {
                _animator.SetTrigger("jump");
            }
        }

        float inputMagnitude = _moveDirection.magnitude;
        bool isRunning = run.action.IsPressed();
        float animationSpeed = inputMagnitude * (isRunning ? 1f : 0.5f);
        _animator.SetFloat("speed", animationSpeed);
    }

    void FixedUpdate()
    {
        // ... (FixedUpdate içeriği aynı kaldı, değişiklik yok) ...
        bool isRunning = run.action.IsPressed();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (_moveDirection == Vector2.zero)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * _moveDirection.y + camRight * _moveDirection.x).normalized;
        Vector3 targetVelocity = moveDirection * currentSpeed;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
    }

    // --- YENİ EKLENDİ: Dışarıdan durumu ve animasyonları güncellemek için ---
    // EquipmentManager bu fonksiyonu çağıracak.
    public void SetEquippedState(bool state)
    {
        isEquipped = state;

        // Kılıç alma/bırakma animasyon tetiklemelerini buraya taşıdık
        if (isEquipped)
        {
            _animator.SetTrigger("getSword");
        }
        else
        {
            _animator.SetTrigger("dropSword");
        }
    }

    // --- Bu fonksiyonlar artık kullanılmıyor, silebilirsin ---
    // void NoSwordAnimations() { }
    // void WithSwordAnimations() { }
}