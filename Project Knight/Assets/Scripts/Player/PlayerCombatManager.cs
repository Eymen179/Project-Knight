using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatManager : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private InputActionReference attack;
    [SerializeField] private InputActionReference block;

    public bool isWeaponEquipped = false;

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

        bool isBlocking = block.action.IsPressed();
        _animator.SetBool("isBlocking", isBlocking);

        // --- KORUMA (INTERRUPT) ---
        if (isBlocking)
        {
            // Gard alýrsak saldýrý emrini sil ve hareketi durdur
            _animator.ResetTrigger("attack");
            GetComponent<PlayerMovement>().enabled = false;
            return;
        }
        else
        {
            GetComponent<PlayerMovement>().enabled = true;
        }

        // --- TIKLAMA (SPAM) KONTROLÜ ---
        if (attack.action.WasPressedThisFrame())
        {
            int attackLayer = 1; // Saldýrý animasyonlarýnýn olduðu Layer numarasý

            // 1. O anki durumu VE eðer bir geçiþ varsa "bir sonraki" durumu al
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(attackLayer);
            AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(attackLayer);

            // 2. Kontrol et: Þuan Attack1/Attack2'de miyiz VEYA onlara geçiþ mi yapýyoruz?
            bool inAttack1 = currentState.IsName("Attack1") || nextState.IsName("Attack1");
            bool inAttack2 = currentState.IsName("Attack2") || nextState.IsName("Attack2");

            // 3. Eðer Attack1 veya Attack2 içindeysek (ya da girmek üzereysek) týklamayý ÇÖPE AT!
            if (inAttack1 || inAttack2)
            {
                return;
            }

            // 4. Yukarýdaki engele takýlmadýysak (Yani Boþtaysak veya Attack3'teysek) tetiði çek
            _animator.ResetTrigger("attack");
            _animator.SetTrigger("attack");
        }
    }
}