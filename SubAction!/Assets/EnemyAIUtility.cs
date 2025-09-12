using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIUtility
{
    public static class AngleToleranceCluster
    {
        struct Item 
        { 
            public float angle; 
            public int index; 
        }

        public static List<List<int>> ClusterByMaxGap(Vector2 root, List<ScoreInputData> targets, float maxGapDeg = 20f)
        {
            var list = new List<Item>(targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                Vector2 v = targets[i].position - root;
                if (v.sqrMagnitude < 1e-8f) continue;
                float a = Mathf.Repeat(Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg, 360f);
                list.Add(new Item { angle = a, index = i });
            }
            if (list.Count == 0) return new List<List<int>>();

            list.Sort((a, b) => a.angle.CompareTo(b.angle));

            var clusters = new List<List<int>>();
            var current = new List<int> { list[0].index };
            for (int i = 1; i < list.Count; i++)
            {
                float gap = Mathf.DeltaAngle(list[i - 1].angle, list[i].angle);
                gap = Mathf.Abs(gap); 
                if (gap > maxGapDeg)
                {
                    clusters.Add(current);
                    current = new List<int>();
                }
                current.Add(list[i].index);
            }
            clusters.Add(current);

            float endGap = Mathf.Abs(Mathf.DeltaAngle(list[^1].angle, list[0].angle));
            if (endGap <= maxGapDeg && clusters.Count > 1)
            {
                var first = clusters[0];
                var last = clusters[^1];
                first.AddRange(last);
                clusters.RemoveAt(clusters.Count - 1);
            }

            return clusters;
        }
    }

    public static float CalculateChasePriority(int targetRank, float targetDistance)
    {
        float prio = targetRank / (float)targetDistance;
        return prio;
    }

    public static Vector2 GetOscillationOverDirection(Vector2 baseDir, float time, float freq = 0.5f, float amp = 0.2f)
    {
        baseDir.Normalize();
        Vector2 perp = new Vector2(-baseDir.y, baseDir.x);

        float theta = Mathf.Sin(time * Mathf.PI * 2f * freq) * amp;

        Vector2 dir = baseDir * Mathf.Cos(theta) + perp * Mathf.Sin(theta);
        return dir.normalized;
    }

    public static Vector2 GetOscillationInPlace(float time, float freq = 0.5f, float amp = 0.2f, float phaseShift = 0.0f)
    {
        float x = Mathf.Sin(time * Mathf.PI * 2f * freq + phaseShift) * amp;
        float y = Mathf.Cos(time * Mathf.PI * 2f * freq * 0.7f) * amp * 0.5f;
        return new Vector2(x, y);
    }
}
