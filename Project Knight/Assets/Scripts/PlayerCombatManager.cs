using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatManager : MonoBehaviour
{
    // Animator'e eri�im i�in
    private Animator _animator;

    // Yeni Input System'den "Attack" eylemini almak i�in
    [SerializeField] private InputActionReference attackAction;

    // YEN� (Opsiyonel): Sald�r� s�ras�nda hareketi k�s�tlamak i�in
    // ThirdPersonMovement script'inize eri�im
    // private ThirdPersonMovement movementScript;
    void Start()
    {
        _animator = GetComponent<Animator>();

        // Opsiyonel: Di�er script'e eri�
        // movementScript = GetComponent<ThirdPersonMovement>();
    }
    void Update()
    {
        // "Attack" eylemine (Sol T�k) BU FRAME bas�ld� m�?
        if (attackAction.action.WasPressedThisFrame())
        {
            // Animator'e "Attack" ad�ndaki teti�i g�nder
            _animator.SetTrigger("attack");
        }
    }
}