using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour // Script ad�n� de�i�tirdim
{
    // Orijinal De�i�kenler
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

    // --- YEN�: Z�plama De�i�kenleri ---
    [Header("Jump")] // Inspector'da ba�l�k olu�turur
    [SerializeField] private InputActionReference jump;
    [SerializeField] private float jumpForce = 5f;

    // --- YEN�: Yer Kontrol� De�i�kenleri ---
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // Karakterin aya��n�n alt�ndaki bir objenin Transform'u
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer; // Nelerin "zemin" olarak kabul edilece�ini belirler
    private bool isGrounded; // Karakterin yerde olup olmad���n� tutar

    [Header("Use Weapons")]
    [SerializeField] private InputActionReference equip;
    private bool isEquipped;
    private int pressCounter = 0;
    public GameObject sword;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;
    }

    void Update() // Input ve Animasyon
    {
        if (equip.action.WasPressedThisFrame())
        {
            pressCounter++;
            if(pressCounter%2 != 0) 
            {
                isEquipped = true;
                _animator.SetTrigger("getSword");
            }
            else
            {
                isEquipped = false;
                _animator.SetTrigger("dropSword");
            }
            sword.SetActive(isEquipped);
            Debug.Log("Bastım " + isEquipped);
        }
        // --- YENİ: Önceki Durumu Sakla ---
        // isGrounded'ın bu frame'deki değerini hesaplamadan önce, geçen frame'deki değerini bir değişkene al.
        // 'isGrounded' değişkeninin class seviyesinde (en üstte) tanımlı olması gerekiyor, ki zaten öyle.
        bool wasGrounded = isGrounded;

        // --- Yer Kontrolü ---
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // --- Animator'a 'isGrounded' Bilgisini Gönder ---
        _animator.SetBool("isGrounded", isGrounded);

        // --- YENİ: "Land" Tetikleyicisini Gönder ---
        // Eğer geçen frame havadaysak (wasGrounded == false) VE
        // bu frame yerdeysek (isGrounded == true)...
        // Bu, "YERE YENİ İNDİK" demektir.
        if (isGrounded && !wasGrounded)
        {
            _animator.SetTrigger("land");
        }
        // --- Input Okuma ---
        _moveDirection = move.action.ReadValue<Vector2>();

        // --- Zıplama Input'u ---
        if (jump.action.WasPressedThisFrame() && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // --- YENİ: Zıplama Animasyonunu Tetikle ---
            if (isEquipped)
            {
                _animator.SetTrigger("jumpWithSword");
            }
            else
            {
                _animator.SetTrigger("jump");
            }
        }

        // --- Animasyon Ayarları (Hız) ---
        float inputMagnitude = _moveDirection.magnitude;
        bool isRunning = run.action.IsPressed();
        float animationSpeed = inputMagnitude * (isRunning ? 1f : 0.5f);
        _animator.SetFloat("speed", animationSpeed);
    }

    void FixedUpdate() // Fizik tabanl� hareket
    {
        bool isRunning = run.action.IsPressed();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // --- HAREKET HESAPLAMASI (DE���MED�) ---
        if (_moveDirection == Vector2.zero)
        {
            // DE���T�: rb.linearVelocity.y idi, yer�ekiminin daha iyi �al��mas� i�in rb.velocity.y kullanmak daha do�rudur.
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // Kaymay� durdur ama d��meyi engelleme
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

        // DE���T�: Y eksenindeki h�z� (z�plama ve yer�ekimi) koru
        // rb.linearVelocity.y yerine rb.velocity.y kullanmak daha standartt�r.
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // --- ROTASYON HESAPLAMASI (DE���MED�) ---
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
    }

    void NoSwordAnimations()
    {

    }
    void WithSwordAnimations()
    {

    }
}
