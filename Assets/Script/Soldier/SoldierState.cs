using UnityEngine;

public class SoldierState
{
    protected Soldier soldier;
    protected SoldierStateMachine soldierStateMachine;
    protected string animName;

    public SoldierState(Soldier soldier, SoldierStateMachine soldierStateMachine, string animName)
    {
        this.soldier = soldier;
        this.soldierStateMachine = soldierStateMachine;
        this.animName = animName;
    }

    public virtual void Enter()
    {
        soldier.Anim.SetBool(animName, true);
        //Debug.Log(animName + " Enter");
    }

    public virtual void LogicalUpdate()
    {
    }

    public virtual void PhysicsUpdate()
    {
    }

    public virtual void Exit()
    {
        soldier.Anim.SetBool(animName, false);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
