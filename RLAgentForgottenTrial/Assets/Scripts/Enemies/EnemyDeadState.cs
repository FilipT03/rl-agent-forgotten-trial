
using UnityEngine;

public class EnemyDeadState : IEnemyState
{
    public void Enter(EnemyAI enemy)
    {
        //enemy.agent.enabled = false;  No need to disable the agent if the game object is disabled
        Debug.Log("Enemy Died");
        enemy.gameObject.SetActive(false); // Since there are no animations, it's better to just disable enemies immediately 
    }

    public void Exit(EnemyAI enemy) {}

    public void Update(EnemyAI enemy) {}
}
