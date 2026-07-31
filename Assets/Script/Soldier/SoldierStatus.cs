using UnityEngine;

public class SoldierStatus : EnemyStatus
{
    private Soldier soldier;

    protected override void Awake()
    {
        base.Awake();
        soldier = GetComponent<Soldier>();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        // When hit, immediately aggro onto the player regardless of detection range
        if (soldier != null && currentHealth > 0)
        {
            soldier.AlertToAttacker();
        }
    }
}
