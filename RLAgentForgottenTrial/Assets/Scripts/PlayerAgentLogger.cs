using Unity.MLAgents;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAgentLogger : MonoBehaviour
{
    private int totalShotsFired;
    private int totalTargetHits;
    private int totalSuccessfulHits;
    private int totalDetections;
    private int episodeCount;
    private int totalKills;

    private bool hitAllTargets;

    private float cumulativeReward;
    private int stepCount;

    private StatsRecorder stats;
    private string scenePrefix;

    private void Awake()
    {
        stats = Academy.Instance.StatsRecorder;
        scenePrefix = SceneManager.GetActiveScene().name;
    }

    public void OnEpisodeBegin()
    {
        totalShotsFired = 0;
        totalTargetHits = 0;
        totalSuccessfulHits = 0;
        totalKills = 0;
        hitAllTargets = false;
        totalDetections = 0;
        cumulativeReward = 0;
        stepCount = 0;
        episodeCount++;
    }

    public void OnAction(float reward)
    {
        cumulativeReward += reward;
    }

    public void OnStep() => stepCount++;

    public void OnShotFired() => totalShotsFired++;

    public void OnHitTarget()
    {
        totalTargetHits++;
        totalSuccessfulHits++;
    }

    public void OnEnemyKilled() => totalKills++;

    public void OnEnemyHit() => totalSuccessfulHits++;

    public void OnHitAllTargets() => hitAllTargets = true;

    public void OnDetected() => totalDetections++;

    public void OnEpisodeEnd(bool success)
    {
        float precision = totalShotsFired > 0 ? (float)totalSuccessfulHits / totalShotsFired : 0f;
        float successRate = success ? 1f : 0f;

        string prefix = $"{scenePrefix}/";

        stats.Add($"{prefix}Main/Average Cumulative Reward", cumulativeReward);
        stats.Add($"{prefix}Main/Episode Length", stepCount);
        stats.Add($"{prefix}Main/Success Rate", successRate);

        stats.Add($"{prefix}Task/Precision", precision);
        stats.Add($"{prefix}Task/Shots Fired", totalShotsFired);
        stats.Add($"{prefix}Task/Targets Hit", totalTargetHits);
        stats.Add($"{prefix}Task/Target Completions", hitAllTargets ? 1f : 0f);
        stats.Add($"{prefix}Task/Times Detected", totalDetections);
        stats.Add($"{prefix}Task/Enemies Killed", totalKills);

        stats.Add($"{prefix}Debug/Episode Count", episodeCount);
    }
}
