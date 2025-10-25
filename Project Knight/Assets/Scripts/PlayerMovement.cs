/*using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour // Veya ThirdPersonMovement
{
    // Orijinal De�i�kenler
    public Vector2 _moveDirection;
    private Rigidbody rb;
    private Animator _animator;

    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference run;

    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    // YEN�: Kameray� ve d�n�� h�z�n� referans almak i�in
    private Transform mainCameraTransform;
    [SerializeField] private float turnSpeed = 10f; // Karakterin hareket y�n�ne d�nme h�z�

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        // YEN�: Ana kameran�n transformunu al (Kameran�z�n "MainCamera" tag'ine sahip oldu�undan emin olun)
        mainCameraTransform = Camera.main.transform;
    }

    void Update() // Animasyon ve Input okuma i�in ideal
    {
        // Input okuma
        _moveDirection = move.action.ReadValue<Vector2>();

        // Animasyon ayarlar� (Bu k�s�m ayn� kalabilir)
        float inputMagnitude = _moveDirection.magnitude;
        bool isRunning = run.action.IsPressed();
        float animationSpeed = inputMagnitude * (isRunning ? 1f : 0.5f);

        _animator.SetFloat("speed", animationSpeed);
    }

    void FixedUpdate() // Fizik tabanl� hareket i�in ideal
    {
        // DE���T�: T�m hareket mant��� kamera y�n�ne g�re yeniden yaz�ld�

        bool isRunning = run.action.IsPressed();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // --- HAREKET HESAPLAMASI (Kameraya G�re) ---

        // E�er input yoksa, karakterin yatay h�z�n� s�f�rla (yer�ekimi etkilenmesin)
        if (_moveDirection == Vector2.zero)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        // 1. Kameran�n ileri (forward) ve sa� (right) y�nlerini al
        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;

        // 2. Y eksenini s�f�rla (karakterin havaya u�mas�n�/yere batmas�n� engelle)
        camForward.y = 0f;
        camRight.y = 0f;

        // 3. Vekt�rleri normalize et (Y eksenini kald�r�nca b�y�kl�kleri de�i�ir)
        camForward.Normalize();
        camRight.Normalize();

        // 4. Input'u kamera y�nleriyle birle�tirerek hareket y�n�n� hesapla
        //    (_moveDirection.y = W/S tu�lar�, _moveDirection.x = A/D tu�lar�)
        Vector3 moveDirection = (camForward * _moveDirection.y + camRight * _moveDirection.x).normalized;

        // 5. Rigidbody'e h�z� uygula
        Vector3 targetVelocity = moveDirection * currentSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);


        // --- ROTASYON HESAPLAMASI ---

        // Karakterin, hesaplanan hareket y�n�ne (moveDirection) bakmas�n� sa�la
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        // Karakterin aniden de�il, yumu�ak bir �ekilde d�nmesi i�in Slerp kullan
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
    }
}*/
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


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;
    }

    void Update() // Input ve Animasyon
    {
        // --- YEN�: Yer Kontrol� ---
        // Her frame karakterin yerde olup olmad���n� kontrol et
        // groundCheck pozisyonunda, groundCheckRadius yar��ap�nda bir k�re olu�turur
        // ve bu k�re groundLayer'a temas ediyorsa 'true' d�ner.
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // --- Input Okuma ---
        _moveDirection = move.action.ReadValue<Vector2>();

        // --- YEN�: Z�plama Input'u ---
        // E�er z�plama tu�una BU FRAME bas�ld�ysa VE karakter yerdeyse
        if (jump.action.WasPressedThisFrame() && isGrounded)
        {
            // Y eksenindeki mevcut h�z� s�f�rlay�p (d��erken z�plarsa daha tutarl� olur)
            // yukar� y�nl� anl�k bir kuvvet (Impulse) uygula.
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // --- Animasyon Ayarlar� ---
        float inputMagnitude = _moveDirection.magnitude;
        bool isRunning = run.action.IsPressed();
        float animationSpeed = inputMagnitude * (isRunning ? 1f : 0.5f);
        _animator.SetFloat("speed", animationSpeed);

        // YEN� (Opsiyonel): Z�plama animasyonlar� i�in
        // Animator'�n�zde "isGrounded" ad�nda bir bool parametresi olu�turun.
        // _animator.SetBool("isGrounded", isGrounded);
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
}
