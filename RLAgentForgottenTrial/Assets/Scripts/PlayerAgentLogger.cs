using Unity.MLAgents;
using UnityEngine;

public class PlayerAgentLogger : MonoBehaviour
{
    private int totalShotsFired;
    private int totalHits;
    private int totalDetections;
    private int episodeCount;

    private float cumulativeReward;
    private int stepCount;

    private StatsRecorder stats;

    private void Awake()
    {
        stats = Academy.Instance.StatsRecorder;
    }

    public void OnEpisodeBegin()
    {
        totalShotsFired = 0;
        totalHits = 0;
        totalDetections = 0;
        cumulativeReward = 0;
        stepCount = 0;
        episodeCount++;
    }

    public void OnAction(float reward)
    {
        cumulativeReward += reward;
        stepCount++;
    }

    public void OnShotFired()
    {
        totalShotsFired++;
    }

    public void OnHitTarget()
    {
        totalHits++;
    }

    public void OnDetected()
    {
        totalDetections++;
    }

    public void OnEpisodeEnd(bool success)
    {
        float precision = totalShotsFired > 0 ? (float)totalHits / totalShotsFired : 0f;
        float successRate = success ? 1f : 0f;
        float avgSteps = stepCount;

        stats.Add("Main/Average Cumulative Reward", cumulativeReward);
        stats.Add("Main/Episode Length", avgSteps);
        stats.Add("Main/Success Rate", successRate);

        stats.Add("Task/Precision", precision);
        stats.Add("Task/Shots Fired", totalShotsFired);
        stats.Add("Task/Times Detected", totalDetections);

        stats.Add("Debug/Episode Count", episodeCount);
    }
}
