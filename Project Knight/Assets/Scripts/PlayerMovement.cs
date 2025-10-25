using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour // Veya ThirdPersonMovement
{
    // Orijinal Deðiþkenler
    public Vector2 _moveDirection;
    private Rigidbody rb;
    private Animator _animator;

    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference run;

    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    // YENÝ: Kamerayý ve dönüþ hýzýný referans almak için
    private Transform mainCameraTransform;
    [SerializeField] private float turnSpeed = 10f; // Karakterin hareket yönüne dönme hýzý

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        // YENÝ: Ana kameranýn transformunu al (Kameranýzýn "MainCamera" tag'ine sahip olduðundan emin olun)
        mainCameraTransform = Camera.main.transform;
    }

    void Update() // Animasyon ve Input okuma için ideal
    {
        // Input okuma
        _moveDirection = move.action.ReadValue<Vector2>();

        // Animasyon ayarlarý (Bu kýsým ayný kalabilir)
        float inputMagnitude = _moveDirection.magnitude;
        bool isRunning = run.action.IsPressed();
        float animationSpeed = inputMagnitude * (isRunning ? 1f : 0.5f);

        _animator.SetFloat("speed", animationSpeed);
    }

    void FixedUpdate() // Fizik tabanlý hareket için ideal
    {
        // DEÐÝÞTÝ: Tüm hareket mantýðý kamera yönüne göre yeniden yazýldý

        bool isRunning = run.action.IsPressed();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // --- HAREKET HESAPLAMASI (Kameraya Göre) ---

        // Eðer input yoksa, karakterin yatay hýzýný sýfýrla (yerçekimi etkilenmesin)
        if (_moveDirection == Vector2.zero)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        // 1. Kameranýn ileri (forward) ve sað (right) yönlerini al
        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;

        // 2. Y eksenini sýfýrla (karakterin havaya uçmasýný/yere batmasýný engelle)
        camForward.y = 0f;
        camRight.y = 0f;

        // 3. Vektörleri normalize et (Y eksenini kaldýrýnca büyüklükleri deðiþir)
        camForward.Normalize();
        camRight.Normalize();

        // 4. Input'u kamera yönleriyle birleþtirerek hareket yönünü hesapla
        //    (_moveDirection.y = W/S tuþlarý, _moveDirection.x = A/D tuþlarý)
        Vector3 moveDirection = (camForward * _moveDirection.y + camRight * _moveDirection.x).normalized;

        // 5. Rigidbody'e hýzý uygula
        Vector3 targetVelocity = moveDirection * currentSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);


        // --- ROTASYON HESAPLAMASI ---

        // Karakterin, hesaplanan hareket yönüne (moveDirection) bakmasýný saðla
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        // Karakterin aniden deðil, yumuþak bir þekilde dönmesi için Slerp kullan
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
    }
}
