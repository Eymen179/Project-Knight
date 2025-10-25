using UnityEngine;
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
}
