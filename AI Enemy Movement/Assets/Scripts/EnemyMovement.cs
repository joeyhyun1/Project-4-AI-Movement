using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [HideInInspector]
    public Transform Player;
    public LayerMask HidableLayers;
    public EnemyLineOfSightChecker LineOfSightChecker;
    public NavMeshAgent Agent;

    [Range(0.01f, 1f)]
    public float UpdateFrequency = 0.25f;

    private Coroutine MovementCoroutine;
    private Collider[] Colliders = new Collider[10];

    private PostSelector HideSelector;
    private PostSelector AdvanceSelector;
    public PostSelector ActiveSelector;

    [HideInInspector] public List<PostSelector.ScoredPost> LastScored = new List<PostSelector.ScoredPost>();

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();

        LineOfSightChecker.OnGainSight += HandleGainSight;
        LineOfSightChecker.OnLoseSight += HandleLoseSight;

        HideSelector = new PostSelector
        {
            Name = "Hide",
            Criteria = new List<Criterion>
            {
                new FacingAwayCriterion(),
                new DistanceFromThreatCriterion { MinDistance = 3f, PreferredDistance = 8f },
                new CloseToAgentCriterion { MaxDistance = 20f },
            }
        };

        AdvanceSelector = new PostSelector
        {
            Name = "Advance",
            Criteria = new List<Criterion>
            {
                new FacingAwayCriterion(),
                new CloseToThreatCriterion { MaxDistance = 15f },
                new NotBehindPlayerCriterion(),
            }
        };

        ActiveSelector = HideSelector;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ActiveSelector = (ActiveSelector == HideSelector) ? AdvanceSelector : HideSelector;
            Debug.Log("Active selector: " + ActiveSelector.Name);
        }
    }

    private void HandleGainSight(Transform Target)
    {
        if (MovementCoroutine != null) StopCoroutine(MovementCoroutine);
        Player = Target;
        MovementCoroutine = StartCoroutine(Hide(Target));
    }

    private void HandleLoseSight(Transform Target)
    {
        if (MovementCoroutine != null) StopCoroutine(MovementCoroutine);
        Player = null;
    }

    private IEnumerator Hide(Transform Target)
    {
        WaitForSeconds Wait = new WaitForSeconds(UpdateFrequency);
        while (true)
        {
            for (int i = 0; i < Colliders.Length; i++) Colliders[i] = null;
            int hits = Physics.OverlapSphereNonAlloc(
                Agent.transform.position,
                LineOfSightChecker.Collider.radius,
                Colliders,
                HidableLayers);

            var candidates = new List<(Vector3 pos, Vector3 normal)>();
            for (int i = 0; i < hits; i++)
            {
                if (Colliders[i] == null) continue;

                if (NavMesh.SamplePosition(Colliders[i].transform.position,
                                           out NavMeshHit sampleHit, 2f, Agent.areaMask))
                {
                    if (NavMesh.FindClosestEdge(sampleHit.position,
                                                out NavMeshHit edgeHit, Agent.areaMask))
                    {
                        candidates.Add((edgeHit.position, edgeHit.normal));

                        Vector3 awayDir = (Target.position - edgeHit.position).normalized;
                        Vector3 probePos = Colliders[i].transform.position - awayDir * 2f;
                        if (NavMesh.SamplePosition(probePos, out NavMeshHit sampleHit2, 2f, Agent.areaMask))
                        {
                            if (NavMesh.FindClosestEdge(sampleHit2.position,
                                                        out NavMeshHit edgeHit2, Agent.areaMask))
                            {
                                candidates.Add((edgeHit2.position, edgeHit2.normal));
                            }
                        }
                    }
                }
            }

            LastScored = ActiveSelector.Score(candidates, Agent.transform.position, Target.position);
            LastScored.Sort((a, b) => b.Score.CompareTo(a.Score));

            if (LastScored.Count > 0 && LastScored[0].Score > 0f)
            {
                Agent.SetDestination(LastScored[0].Position);
            }

            yield return Wait;
        }
    }

    private void OnDrawGizmos()
    {
        if (LastScored == null) return;
        foreach (var p in LastScored)
        {
            Gizmos.color = Color.Lerp(Color.red, Color.green, p.Score);
            Gizmos.DrawSphere(p.Position + Vector3.up * 0.2f, 0.25f);
        }
        if (LastScored.Count > 0 && LastScored[0].Score > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(LastScored[0].Position + Vector3.up * 0.2f, 0.5f);
        }
    }
}