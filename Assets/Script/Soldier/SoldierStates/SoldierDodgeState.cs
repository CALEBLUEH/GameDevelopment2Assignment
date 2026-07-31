using UnityEngine;
using UnityEngine.AI;

public class SoldierDodgeState : SoldierState
{
    private const float DODGE_TIMEOUT = 1.0f;
    private const float ARRIVAL_THRESHOLD = 0.3f;

    private float enterTime;

    public SoldierDodgeState(Soldier soldier, SoldierStateMachine soldierStateMachine, string animName)
        : base(soldier, soldierStateMachine, animName)
    {
    }

    public override void Enter()
    {
        // Reset the firing trigger so the animator can leave the Firing state
        soldier.Anim.ResetTrigger("Firing");

        base.Enter();

        enterTime = Time.time;

        Vector3 dodgeTarget = CalculateDodgePosition();
        soldier.Agent.isStopped = false;
        soldier.Agent.speed = soldier.DodgeSpeed;
        soldier.Agent.SetDestination(dodgeTarget);
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        // Rotate toward player while dodging so we keep shooting
        if (soldier.target != null)
        {
            Vector3 lookDir = (soldier.target.position - soldier.transform.position).normalized;
            lookDir.y = 0f;
            soldier.HandleRotation(lookDir);
        }

        bool arrived = !soldier.Agent.pathPending
                       && soldier.Agent.remainingDistance <= ARRIVAL_THRESHOLD;
        bool timedOut = Time.time - enterTime >= DODGE_TIMEOUT;

        if (arrived || timedOut)
        {
            soldierStateMachine.ChangeState(soldier.FiringState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        soldier.Agent.ResetPath();
        soldier.Agent.isStopped = false;
        soldier.Agent.speed = soldier.DefaultMoveSpeed;
    }

    private Vector3 CalculateDodgePosition()
    {
        // Dodge perpendicular to the direction the bullet is coming from
        Vector3 dodgeRight = soldier.transform.right;

        // Randomly pick left or right
        float side = Random.value > 0.5f ? 1f : -1f;
        Vector3 dodgeDirection = dodgeRight * side;

        Vector3 desiredPos = soldier.transform.position + dodgeDirection * soldier.DodgeDistance;

        // Make sure the dodge target is on the NavMesh
        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, soldier.DodgeDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // If that side is blocked, try the other side
        desiredPos = soldier.transform.position - dodgeDirection * soldier.DodgeDistance;
        if (NavMesh.SamplePosition(desiredPos, out hit, soldier.DodgeDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // Fallback: stay put
        return soldier.transform.position;
    }
}
