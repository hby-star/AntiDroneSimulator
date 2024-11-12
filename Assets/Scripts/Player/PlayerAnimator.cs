using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Player Player => GetComponentInParent<Player>();

    private void AnimationFinished()
    {
        Player.StateMachine.CurrentState.AnimationFinished();
    }
}
