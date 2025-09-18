using UnityEngine;

public interface IEnemyState
{
    void Enter(EnemyAI enemy);
    void Update(EnemyAI enemy);
    void Exit(EnemyAI enemy);
}
