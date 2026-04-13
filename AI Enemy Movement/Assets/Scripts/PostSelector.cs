using System.Collections.Generic;
using UnityEngine;

public class PostSelector
{
    public string Name;
    public List<Criterion> Criteria = new List<Criterion>();

    public struct ScoredPost
    {
        public Vector3 Position;
        public Vector3 Normal;
        public float Score;
    }

    public List<ScoredPost> Score(List<(Vector3 pos, Vector3 normal)> candidates,
        Vector3 agentPos, Vector3 threatPos)
    {
        var results = new List<ScoredPost>();
        foreach (var c in candidates)
        {
            float score = 1f;
            foreach (var criterion in Criteria)
            {
                score *= criterion.Evaluate(c.pos, c.normal, agentPos, threatPos);
                if (score <= 0f) break;
            }
            results.Add(new ScoredPost { Position = c.pos, Normal = c.normal, Score = score });
        }
        return results;
    }
}