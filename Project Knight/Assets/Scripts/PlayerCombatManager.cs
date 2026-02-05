using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatManager : MonoBehaviour
{
    /*// Animator'e eriþim için
    private Animator _animator;

    // Yeni Input System'den "Attack" eylemini almak için
    [SerializeField] private InputActionReference attack;
    [SerializeField] private InputActionReference block;

    public bool isWeaponEquipped = false;
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
        if (isWeaponEquipped && attack.action.WasPressedThisFrame() && !isBlocking)
        {
            // Animator'e "Attack" adýndaki tetiði gönder
            _animator.SetTrigger("attack");
        }
    }*/
    private Animator _animator;

    [SerializeField] private InputActionReference attack;
    [SerializeField] private InputActionReference block;

    public bool isWeaponEquipped = false;

    // Animasyonun % kaçýnda input kabul edelim? (0.75 = %75)
    [SerializeField] private float attackInputThreshold = 0.75f;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        ApplyAttackInputs();
    }
    public void ApplyAttackInputs()
    {
        // Bloklama kontrolü
        bool isBlocking = block.action.IsPressed();
        _animator.SetBool("isBlocking", isBlocking);

        // Eðer silah yoksa veya blokluyorsak saldýramayýz
        if (!isWeaponEquipped || isBlocking) return;

        // Sol týk basýldý mý?
        if (attack.action.WasPressedThisFrame())
        {
            if (_animator.IsInTransition(0)) return;
            // O anki animasyon durumunu al (Layer 0 = Base Layer)
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(1);

            // EÐER þu an zaten bir saldýrý animasyonu içindeysek...
            if (stateInfo.IsTag("Attack"))
            {
                // ...ve animasyonun tamamlanma oraný belirlenen eþiðin (0.75) altýndaysa...
                if (stateInfo.normalizedTime < attackInputThreshold)
                {
                    // Vuruþu YOK SAY (Return)
                    return;
                }
            }

            // Yukarýdaki engele takýlmadýysak saldýrýyý tetikle
            _animator.SetTrigger("attack");

            // Ekstra Önlem: Trigger birikmesini önlemek için önceki triggerlarý resetle.
            // Bu, spam yapýldýðýnda animasyon kuyruðunun þiþmesini engeller.
            /*_animator.ResetTrigger("attack");
            _animator.SetTrigger("attack");*/
        }
    }
}