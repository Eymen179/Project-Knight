using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatManager : MonoBehaviour
{
    // Animator'e eriþim için
    private Animator _animator;

    // Yeni Input System'den "Attack" eylemini almak için
    [SerializeField] private InputActionReference attackAction;

    // YENÝ (Opsiyonel): Saldýrý sýrasýnda hareketi kýsýtlamak için
    // ThirdPersonMovement script'inize eriþim
    // private ThirdPersonMovement movementScript;
    void Start()
    {
        _animator = GetComponent<Animator>();

        // Opsiyonel: Diðer script'e eriþ
        // movementScript = GetComponent<ThirdPersonMovement>();
    }
    void Update()
    {
        // "Attack" eylemine (Sol Týk) BU FRAME basýldý mý?
        if (attackAction.action.WasPressedThisFrame())
        {
            // Animator'e "Attack" adýndaki tetiði gönder
            _animator.SetTrigger("attack");
        }
    }
}