using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : IEnemyState
{
    private Vector3 walkPoint;
    private bool walkPointSet;

    public void Enter(EnemyAI enemy)
    {
        enemy.agent.speed = enemy.normalSpeed;
        FindWalkPoint(enemy);
    }

    public void Update(EnemyAI enemy)
    {
        if (enemy == null || enemy.player == null) return;
        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if (distance <= enemy.attackRange)
        {
            enemy.StateMachine.ChangeState(new EnemyAttackState());
            return;
        }
        else if (distance <= enemy.sightRange)
        {
            enemy.StateMachine.ChangeState(new EnemyChaseState());
            return;
        }

        Patrol(enemy);
    }

    public void Exit(EnemyAI enemy) { }

    private void Patrol(EnemyAI enemy)
    {
        if (!walkPointSet)
            FindWalkPoint(enemy);

        enemy.agent.SetDestination(walkPoint);

        if (Vector3.Distance(enemy.transform.position, walkPoint) < 2f)
            walkPointSet = false;
    }

    private void FindWalkPoint(EnemyAI enemy)
    {
        float randomX = Random.Range(-enemy.walkPointRange.x, enemy.walkPointRange.x);
        float randomZ = Random.Range(-enemy.walkPointRange.y, enemy.walkPointRange.y);
        walkPoint = new Vector3(enemy.transform.position.x + randomX, enemy.transform.position.y, enemy.transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, enemy.groundMask))
        {
            walkPoint = hit.point;
            walkPointSet = true;
        }
    }

}
