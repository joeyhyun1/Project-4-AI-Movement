using UnityEngine;

public abstract class Criterion
{
    public abstract float Evaluate(Vector3 postPos, Vector3 postNormal,
                                   Vector3 agentPos, Vector3 threatPos);
}

public class FacingAwayCriterion : Criterion
{
    public override float Evaluate(Vector3 postPos, Vector3 postNormal,
                                   Vector3 agentPos, Vector3 threatPos)
    {
        Vector3 toThreat = (threatPos - postPos).normalized;
        float dot = Vector3.Dot(postNormal, toThreat); 
        return Mathf.InverseLerp(1f, -1f, dot);
    }
}

public class DistanceFromThreatCriterion : Criterion
{
    public float MinDistance = 3f;
    public float PreferredDistance = 8f;

    public override float Evaluate(Vector3 postPos, Vector3 postNormal,
                                   Vector3 agentPos, Vector3 threatPos)
    {
        float d = Vector3.Distance(postPos, threatPos);
        return Mathf.Clamp01(Mathf.InverseLerp(MinDistance, PreferredDistance, d));
    }
}

public class CloseToAgentCriterion : Criterion
{
    public float MaxDistance = 20f;

    public override float Evaluate(Vector3 postPos, Vector3 postNormal,
                                   Vector3 agentPos, Vector3 threatPos)
    {
        float d = Vector3.Distance(postPos, agentPos);
        return 1f - Mathf.Clamp01(d / MaxDistance);
    }
}

public class CloseToThreatCriterion : Criterion
{
    public float MaxDistance = 15f;

    public override float Evaluate(Vector3 postPos, Vector3 postNormal,
                                   Vector3 agentPos, Vector3 threatPos)
    {
        float d = Vector3.Distance(postPos, threatPos);
        return 1f - Mathf.Clamp01(d / MaxDistance);
    }
}

public class NotBehindPlayerCriterion : Criterion
{
    public override float Evaluate(Vector3 postPos, Vector3 postNormal,
                                   Vector3 agentPos, Vector3 threatPos)
    {
        Vector3 agentToThreat = (threatPos - agentPos).normalized;
        Vector3 agentToPost = (postPos - agentPos).normalized;
        return Vector3.Dot(agentToThreat, agentToPost) < 0.5f ? 1f : 0f;
    }
}