using Unity.MLAgents;
using UnityEngine;

public class PlayerAgentLogger : MonoBehaviour
{
    private int totalShotsFired;
    private int totalHits;
    private int totalDetections;
    private int episodeCount;

    private bool hitAllTargets;

    private float cumulativeReward;
    private int stepCount, rewardsCount;

    private StatsRecorder stats;

    private void Awake()
    {
        stats = Academy.Instance.StatsRecorder;
    }

    public void OnEpisodeBegin()
    {
        totalShotsFired = 0;
        totalHits = 0;
        hitAllTargets = false;
        totalDetections = 0;
        cumulativeReward = 0;
        stepCount = 0;
        //rewardsCount = 0;
        episodeCount++;
    }

    public void OnAction(float reward)
    {
        cumulativeReward += reward;
        //rewardsCount++;
    }

    public void OnStep() => stepCount++;

    public void OnShotFired() => totalShotsFired++;

    public void OnHitTarget() => totalHits++;

    public void OnHitAllTargets() => hitAllTargets = true;

    public void OnDetected() => totalDetections++;

    public void OnEpisodeEnd(bool success)
    {
        float precision = totalShotsFired > 0 ? (float)totalHits / totalShotsFired : 0f;
        float successRate = success ? 1f : 0f;
        //float avgActions = stepCount > 0 ? (float)rewardsCount / stepCount : 0f;

        stats.Add("Main/Average Cumulative Reward", cumulativeReward);
        stats.Add("Main/Episode Length", stepCount);
        stats.Add("Main/Success Rate", successRate);

        stats.Add("Task/Precision", precision);
        stats.Add("Task/Shots Fired", totalShotsFired);
        stats.Add("Task/Target Completions", hitAllTargets ? 1f : 0f);
        stats.Add("Task/Times Detected", totalDetections);

        stats.Add("Debug/Episode Count", episodeCount);
    }
}
