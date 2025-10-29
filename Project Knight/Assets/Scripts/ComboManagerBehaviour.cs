using UnityEngine;

// MonoBehaviour DE��L, StateMachineBehaviour
public class ComboManagerBehaviour : StateMachineBehaviour
{
    [SerializeField] private int comboStepValue;

    // Bu durum (state) girildi�inde �al���r
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animator'deki "comboStep" parametresini bizim belirledi�imiz de�ere ayarla
        animator.SetInteger("comboStep", comboStepValue);
    }
}