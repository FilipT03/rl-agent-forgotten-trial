using UnityEngine;

public class EnemyChaseState : IEnemyState
{

    private float aggroTimer;

    public void Enter(EnemyAI enemy)
    {
        enemy.agent.isStopped = false;
        aggroTimer = 0f;
    }

    public void Update(EnemyAI enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if (distance <= enemy.attackRange)
        {
            enemy.StateMachine.ChangeState(new EnemyAttackState());
            return;
        }

        if (distance > enemy.sightRange)
        {
            aggroTimer += Time.deltaTime;
            if (aggroTimer >= enemy.keepAggroTime)
            {
                enemy.StateMachine.ChangeState(new EnemyPatrolState());
                return;
            }
            else
            {
                aggroTimer = 0f;
            }
        }
        enemy.agent.SetDestination(enemy.player.position);
    }

    public void Exit(EnemyAI enemy) {}
}
