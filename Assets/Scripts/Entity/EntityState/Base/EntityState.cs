using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityState
{
    protected EntityStateMachine EntityStateMachine;
    protected Entity Entity;

    private string _animationName;
    private bool _isAnimationFinished;

    public EntityState(EntityStateMachine entityStateMachine, Entity entity, string animationName)
    {
        EntityStateMachine = entityStateMachine;
        Entity = entity;
        _animationName = animationName;
    }

    public virtual void Enter()
    {
        Entity.Animator.SetBool(_animationName, true);
        _isAnimationFinished = false;
    }

    public virtual void Update()
    {
        if (_isAnimationFinished)
        {
            //EntityStateMachine.ChangeState(Entity.idleState);
        }
    }

    public virtual void Exit()
    {
        Entity.Animator.SetBool(_animationName, false);
    }

    public void AnimationFinished()
    {
        _isAnimationFinished = true;
    }

}