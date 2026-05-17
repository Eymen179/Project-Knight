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

        //Blok kontrolcusu (Sag tik)
        bool isBlocking = block.action.IsPressed();
        _animator.SetBool("isBlocking", isBlocking);

        //Blok yapiyorken saldiri yapilmayacak ve hareket duracak.
        if (isBlocking)
        {
            _animator.ResetTrigger("attack");
            GetComponent<PlayerMovement>().enabled = false;
            return;
        }
        else
        {
            GetComponent<PlayerMovement>().enabled = true;
        }

        //Saldiri kontrolcusu (Sol tik)
        if (attack.action.WasPressedThisFrame())
        {
            int attackLayer = 1;

            //Uclu saldiridan ilk ikisinde sol tiki etkisiz hale getir.
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(attackLayer);
            AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(attackLayer);

            bool inAttack1 = currentState.IsName("Attack1") || nextState.IsName("Attack1");
            bool inAttack2 = currentState.IsName("Attack2") || nextState.IsName("Attack2");

            if (inAttack1 || inAttack2)
            {
                return;
            }

            //Saldiri animasyonu
            _animator.ResetTrigger("attack");
            _animator.SetTrigger("attack");
        }
    }
}