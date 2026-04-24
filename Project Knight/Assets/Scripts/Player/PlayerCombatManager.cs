using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatManager : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private InputActionReference attack;
    [SerializeField] private InputActionReference block;

    public bool isWeaponEquipped = false;

    // Animasyonun % kaçýnda input kabul edelim? (0.75 = %75)
    [SerializeField] private float attackInputThreshold = 0.75f;

    // YENÝ: Saldýrý animasyonlarýnýn bulunduðu Layer'ýn numarasý (AttackLayer = 1)
    private int attackLayerIndex = 1;

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
        if (!isWeaponEquipped) return;
        // Bloklama kontrolü
        bool isBlocking = block.action.IsPressed();
        _animator.SetBool("isBlocking", isBlocking);

        // Eðer silah yoksa veya blokluyorsak saldýramayýz
        //if (!isWeaponEquipped || isBlocking) return;

        // --- YENÝ EKLENEN KORUMA (INTERRUPT) ---
        if (isBlocking)
        {
            // Eðer gard alýyorsak, yarýda kesilen saldýrýnýn trigger'ýný hafýzadan SÝL
            _animator.ResetTrigger("attack");
            return; // Aþaðýdaki saldýrý/sol týk kodlarýný hiç okuma
        }
        // --------------------------------------
        // Sol týk basýldý mý?
        if (attack.action.WasPressedThisFrame())
        {
            // 1. KRÝTÝK DÜZELTME: 0 yerine 1. katmanýn (AttackLayer) geçiþini kontrol et!
            // Eðer halihazýrda Attack1'den Attack2'ye geçiþ yapýlýyorsa týký reddet.
            if (_animator.IsInTransition(attackLayerIndex)) return;

            // O anki animasyon durumunu al
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(attackLayerIndex);

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

            // 2. KRÝTÝK DÜZELTME: Önceki birikmiþ triggerlarý sil!
            // Bu sayede spam yapsan bile kuyrukta sadece tek bir tetik kalýr.
            _animator.ResetTrigger("attack");

            // Þimdi yeni saldýrýyý tetikle
            _animator.SetTrigger("attack");
        }

    }

}