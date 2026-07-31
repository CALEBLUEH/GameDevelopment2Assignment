using UnityEngine;

public class SoldierFiringState : SoldierState
{
    private const float FIRING_ROTATION = 5f;
    private const float ROTATION_SPEED = 10f;
    private Quaternion targetRotation;
    private bool isChasing;

    public SoldierFiringState(Soldier soldier, SoldierStateMachine soldierStateMachine, string animName)
        : base(soldier, soldierStateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        isChasing = false;
        soldier.Agent.isStopped = true;
        soldier.Anim.SetTrigger(animName);

        if (soldier.target != null)
        {
            UpdateTargetRotation(soldier.target);
        }
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        if (soldier.target == null)
        {
            soldierStateMachine.ChangeState(soldier.IdleState);
            return;
        }

        // Check for incoming bullets and attempt to dodge
        if (soldier.TryDodge())
        {
            return;
        }

        float distanceToTarget = soldier.GetDistanceToTarget();

        if (distanceToTarget > soldier.attackRange)
        {
            // Target is too far — chase while shooting
            StartChasing();
        }
        else
        {
            // Target is within range — stop and fire
            StopChasing();
        }

        UpdateTargetRotation(soldier.target);
        MaintainFiringRotation();
        soldier.Attack();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Exit()
    {
        base.Exit();

        soldier.Agent.isStopped = false;
        soldier.Agent.ResetPath();
        soldier.Agent.speed = soldier.DefaultMoveSpeed;
        soldier.Anim.ResetTrigger(animName);
        soldier.Anim.SetBool("Walk", false);

        soldier.StartCoroutine(ResetBaseRotation());
    }

    private void StartChasing()
    {
        if (!isChasing)
        {
            isChasing = true;
            soldier.Agent.isStopped = false;
            soldier.Agent.speed = soldier.ChaseSpeed;
            soldier.Anim.SetBool("Walk", true);
        }

        // Keep updating destination as player moves
        soldier.Agent.SetDestination(soldier.target.position);
    }

    private void StopChasing()
    {
        if (isChasing)
        {
            isChasing = false;
            soldier.Agent.isStopped = true;
            soldier.Agent.ResetPath();
            soldier.Agent.speed = soldier.DefaultMoveSpeed;
            soldier.Anim.SetBool("Walk", false);
        }
    }

    private void UpdateTargetRotation(Transform target)
    {
        Vector3 direction = (target.position - soldier.transform.position).normalized;
        float baseAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        targetRotation = Quaternion.Euler(0, baseAngle + FIRING_ROTATION, 0);
    }

    private void MaintainFiringRotation()
    {
        soldier.transform.rotation = Quaternion.Slerp(
            soldier.transform.rotation,
            targetRotation,
            ROTATION_SPEED * Time.deltaTime
        );
    }

    private System.Collections.IEnumerator ResetBaseRotation()
    {
        float duration = 0.3f;
        Quaternion startRot = soldier.transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, startRot.eulerAngles.y - FIRING_ROTATION, 0);

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            soldier.transform.rotation = Quaternion.Slerp(
                startRot,
                endRot,
                t / duration
            );
            yield return null;
        }
        soldier.transform.rotation = endRot;
    }
}
