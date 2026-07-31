using TMPro;
using UnityEngine;

public class SoldierWalkState : SoldierState
{
    private Vector3 targetPosition;

    public SoldierWalkState(Soldier soldier, SoldierStateMachine soldierStateMachine, string animName) : base(soldier, soldierStateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        targetPosition = soldier.GetRandomNavMeshPosition();
        soldier.Agent.SetDestination(targetPosition);
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        if (soldier.target != null)
        {
            return;
        }

        if (soldier.Agent.velocity.sqrMagnitude > 0.1f)
        {
            soldier.HandleRotation(soldier.Agent.velocity.normalized);
        }

        if (HasReachedDestination())
        {
            soldierStateMachine.ChangeState(soldier.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Exit()
    {
        base.Exit();

        soldier.Agent.ResetPath();
    }

    private bool HasReachedDestination()
    {
        if (!soldier.Agent.pathPending)
        {
            if (soldier.Agent.remainingDistance <= soldier.Agent.stoppingDistance)
            {
                if (!soldier.Agent.hasPath || soldier.Agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
