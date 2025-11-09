using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatManager : MonoBehaviour
{
    // Animator'e eriþim için
    private Animator _animator;

    // Yeni Input System'den "Attack" eylemini almak için
    [SerializeField] private InputActionReference attack;
    [SerializeField] private InputActionReference block;

    // YENÝ (Opsiyonel): Saldýrý sýrasýnda hareketi kýsýtlamak için
    // ThirdPersonMovement script'inize eriþim
    // private ThirdPersonMovement movementScript;
    void Start()
    {
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        bool isBlocking = block.action.IsPressed();

        _animator.SetBool("isBlocking", isBlocking);

        // "Attack" eylemine (Sol Týk) BU FRAME basýldý mý?
        if (attack.action.WasPressedThisFrame() && !isBlocking)
        {
            // Animator'e "Attack" adýndaki tetiði gönder
            _animator.SetTrigger("attack");
        }
    }
}