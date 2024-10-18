using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : EntityState
{
    protected Player Player;



    public PlayerState(EntityStateMachine entityStateMachine, Entity entity, string animationName, Player player) : base(entityStateMachine, entity, animationName)
    {
        Player = player;
    }
}
