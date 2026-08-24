using System.Collections.Generic;
using UnityEngine;

public class FormalCheckpoint : MonoBehaviour, IFormalLevelPermanentState
{
    [SerializeField] private FormalLevelController level;
    [SerializeField] private string owningLevelScene;
    [SerializeField] private FormalMechanismState[] prerequisites;

    private static readonly HashSet<FormalCheckpoint> RegisteredCheckpoints =
        new HashSet<FormalCheckpoint>();
    private bool registeredWithOwningLevel;

    public bool IsComplete { get; private set; }
    public string OwningLevelScene => owningLevelScene;
    public bool IsRegisteredWithOwningLevel => registeredWithOwningLevel;

    void Awake()
    {
        if (!string.IsNullOrEmpty(owningLevelScene))
            RegisterWithOwningLevel();
    }

    void Start()
    {
        if (!registeredWithOwningLevel)
            RegisterWithOwningLevel();
    }

    void OnDestroy()
    {
        RegisteredCheckpoints.Remove(this);
        registeredWithOwningLevel = false;
    }

    /// <summary>Returns true only for this checkpoint's first registration in its loaded level.</summary>
    public bool RegisterWithOwningLevel()
    {
        if (registeredWithOwningLevel)
            return false;

        if (string.IsNullOrEmpty(owningLevelScene))
        {
            Debug.LogError($"[FormalCheckpoint] {name} has no owning level scene configured.", this);
            return false;
        }

        if (owningLevelScene != gameObject.scene.name)
        {
            Debug.LogError(
                $"[FormalCheckpoint] {name} belongs to '{owningLevelScene}' but is placed in '{gameObject.scene.name}'.",
                this);
            return false;
        }

        if (!RegisteredCheckpoints.Add(this))
            return false;

        registeredWithOwningLevel = true;
        return true;
    }

    public void ActivateCheckpoint()
    {
        if (level == null)
            level = FormalLevelActors.FindLevelController(gameObject.scene);

        if (level == null || !PrerequisitesComplete())
            return;

        IsComplete = true;
        level.SetCheckpoint(null, null);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsComplete || !FormalLevelActors.IsPlayer(other) || !PrerequisitesComplete())
            return;

        ActivateCheckpoint();

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.NotifyCheckpointActivated(gameObject.scene.name);
    }

    bool PrerequisitesComplete()
    {
        if (prerequisites == null)
            return true;

        foreach (FormalMechanismState prerequisite in prerequisites)
        {
            if (prerequisite == null || !prerequisite.IsComplete)
                return false;
        }

        return true;
    }
}
