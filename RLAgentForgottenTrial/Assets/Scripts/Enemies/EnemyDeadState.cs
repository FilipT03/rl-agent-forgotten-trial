
using UnityEngine;

public class EnemyDeadState : IEnemyState
{
    public void Enter(EnemyAI enemy)
    {
        enemy.agent.enabled = false;
        Debug.Log("Enemy Died");
        enemy.Invoke(nameof(DestroyEnemy), 180f);
    }

    public void Exit(EnemyAI enemy) {}

    public void Update(EnemyAI enemy) {}

    private void DestroyEnemy(EnemyAI enemy)
    {
        Object.Destroy(enemy.gameObject);
    }
}
