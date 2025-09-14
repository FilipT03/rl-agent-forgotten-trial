using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private bool alreadyAttacked;

    public void Enter(EnemyAI enemy)
    {
        enemy.agent.SetDestination(enemy.transform.position);
        alreadyAttacked = false;
    }

    public void Update(EnemyAI enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if (distance > enemy.attackRange)
        {
            enemy.StateMachine.ChangeState(new EnemyChaseState());
            return;
        }
        enemy.agent.SetDestination(enemy.transform.position);

        Vector3 targetPosition = new Vector3(enemy.player.position.x, enemy.transform.position.y, enemy.player.position.z);
        enemy.transform.LookAt(targetPosition);
        if (!alreadyAttacked)
        {
            PerformAttack(enemy);
        }
    }

    public void Exit(EnemyAI enemy)
    {
        enemy.agent.isStopped = false;
    }

    private void PerformAttack(EnemyAI enemy)
    {
        alreadyAttacked = true;
        Debug.Log("Enemy Attack!");
        enemy.player.GetComponent<Player>().TakeDamage();

        enemy.StartAttackCooldown(enemy.timeBetweenAttacks, () => alreadyAttacked = false);
    }
}