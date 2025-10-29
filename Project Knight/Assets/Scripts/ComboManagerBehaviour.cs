using UnityEngine;

// MonoBehaviour DEÐÝL, StateMachineBehaviour
public class ComboManagerBehaviour : StateMachineBehaviour
{
    [SerializeField] private int comboStepValue;

    // Bu durum (state) girildiðinde çalýþýr
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animator'deki "comboStep" parametresini bizim belirlediðimiz deðere ayarla
        animator.SetInteger("comboStep", comboStepValue);
    }
}