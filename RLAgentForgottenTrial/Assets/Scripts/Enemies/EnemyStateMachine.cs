using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class EnemyStateMachine 
{
    private IEnemyState currentState;
    private readonly EnemyAI enemy;

    public EnemyStateMachine(EnemyAI enemy)
    {
        this.enemy = enemy;
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.Exit(enemy);
        currentState = newState;
        currentState.Enter(enemy);
    }

    public void UpdateState()
    {
        if(enemy == null)
        {
            Debug.LogWarning("Enemy is null");
            return;
        }
        currentState?.Update(enemy);
    }

}
