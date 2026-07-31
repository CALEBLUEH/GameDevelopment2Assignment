using UnityEngine;

public class SoldierIdleState : SoldierState
{
    private float idleDuration;
    private float enterTime;

    public SoldierIdleState(Soldier soldier, SoldierStateMachine soldierStateMachine, string animName) : base(soldier, soldierStateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        enterTime = Time.time;
        idleDuration = Random.Range(5f, 12f);
        soldier.Agent.isStopped = true;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        if (Time.time >= enterTime + idleDuration)
        {
            soldierStateMachine.ChangeState(soldier.WalkState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Exit()
    {
        base.Exit();

        soldier.Agent.isStopped = false;
    }
}
