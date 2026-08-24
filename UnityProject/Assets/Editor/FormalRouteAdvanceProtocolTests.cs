using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class FormalRouteAdvanceProtocolTests
{
    private const string SharedArtL0102Path = "Assets/MoMing/FormalLevels/FormalSharedArt_L01_L02.unity";

    [SetUp]
    public void IsolateInSavedScene()
    {
        EditorSceneManager.OpenScene("Assets/MoMing/FormalLevels/FormalLevel01.unity");
    }

    [Test]
    public void DefaultRouteCatalogMatchesTheSixLevelRoute()
    {
        var flowObject = new GameObject("Flow");
        var flow = flowObject.AddComponent<FormalGameFlowController>();
        InvokeEnsureRouteCatalog(flow);

        var catalog = (IList)typeof(FormalGameFlowController)
            .GetField("routeCatalog", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(flow);

        Assert.AreEqual(6, catalog.Count);
        Assert.AreEqual("FormalLevel01", GetEntryField(catalog[0], "sceneName"));
        Assert.AreEqual("FormalLevel02", GetEntryField(catalog[1], "sceneName"));
        Assert.AreEqual("FormalLevel03", GetEntryField(catalog[2], "sceneName"));
        Assert.AreEqual("FormalLevel04", GetEntryField(catalog[3], "sceneName"));
        Assert.AreEqual("FormalLevel045", GetEntryField(catalog[4], "sceneName"));
        Assert.AreEqual("FormalLevel05", GetEntryField(catalog[5], "sceneName"));

        var l01Shared = (string[])GetEntryField(catalog[0], "sharedArtScenes");
        Assert.AreEqual(1, l01Shared.Length);
        Assert.AreEqual("FormalSharedArt_L01_L02", l01Shared[0]);

        var l045Shared = (string[])GetEntryField(catalog[4], "sharedArtScenes");
        Assert.AreEqual(2, l045Shared.Length);
        Assert.AreEqual("FormalSharedArt_L04_L045", l045Shared[0]);
        Assert.AreEqual("FormalSharedArt_L045_L05", l045Shared[1]);

        Destroy(flowObject);
    }

    [Test]
    public void TransitionDoorLookupResolvesDoorInsideSharedArtIntersection()
    {
        var flowObject = new GameObject("Flow");
        var flow = flowObject.AddComponent<FormalGameFlowController>();
        InvokeEnsureRouteCatalog(flow);

        Scene shared = EditorSceneManager.OpenScene(SharedArtL0102Path, OpenSceneMode.Additive);
        try
        {
            FormalDoor expected = FindTransitionDoorInScene(shared, "ToLevel02");
            Assert.IsNotNull(expected,
                "FormalSharedArt_L01_L02 must contain a ToLevel-named FormalDoor for this contract.");

            var byName = SceneManager.GetSceneByName("FormalSharedArt_L01_L02");
            var found = InvokeFindTransitionDoor(flow, "FormalLevel01", "FormalLevel02");

            Assert.AreEqual(expected, found,
                $"lookup mismatch. byName.isValid={byName.IsValid()} byName.isLoaded={byName.isLoaded} " +
                $"activeScene={SceneManager.GetActiveScene().name} sharedHandleMatch={(byName == shared)} " +
                $"flowOnScene={flow.gameObject.scene.name}");
        }
        finally
        {
            EditorSceneManager.CloseScene(shared, false);
            Destroy(flowObject);
        }
    }

    [Test]
    public void TransitionDoorLookupDegradesToWarningWhenNothingIsLoaded()
    {
        var flowObject = new GameObject("Flow");
        var flow = flowObject.AddComponent<FormalGameFlowController>();
        InvokeEnsureRouteCatalog(flow);

        LogAssert.Expect(LogType.Warning, "No shared transition door found from FormalLevel01 to FormalLevel02.");

        typeof(FormalGameFlowController)
            .GetMethod("OpenTransitionDoor", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(flow, new object[] { "FormalLevel01", "FormalLevel02" });

        Assert.IsNull(InvokeFindTransitionDoor(flow, "FormalLevel01", "FormalLevel02"),
            "Without any loaded shared-art scene the lookup must resolve to null.");

        Destroy(flowObject);
    }

    [Test]
    public void RestartRoutineClosesTransitionDoorImmediatelyBeforeUnloading()
    {
        var flowObject = new GameObject("Flow");
        var flow = flowObject.AddComponent<FormalGameFlowController>();
        InvokeEnsureRouteCatalog(flow);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(FormalGameFlowController).GetField("currentLevelScene", flags).SetValue(flow, "FormalLevel02");
        typeof(FormalGameFlowController).GetField("pendingUnloadScene", flags).SetValue(flow, "FormalLevel01");

        Scene shared = EditorSceneManager.OpenScene(SharedArtL0102Path, OpenSceneMode.Additive);
        try
        {
            FormalDoor door = FindTransitionDoorInScene(shared, "ToLevel02");
            Assert.IsNotNull(door, "Shared art must expose the transition door for this contract.");
            door.SetOpenImmediate();
            Assert.IsTrue(door.IsOpen);

            // The restart routine closes the registered transition door
            // synchronously before its first async step. Depending on editor
            // coroutine scheduling the subsequent unload may surface as a
            // thrown or logged play-mode-only error; both are irrelevant here.
            bool previousIgnore = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                var routine = (IEnumerator)typeof(FormalGameFlowController)
                    .GetMethod("RestartCurrentLevelRoutine", flags)
                    .Invoke(flow, null);
                flow.StartCoroutine(routine);
            }
            catch (Exception)
            {
                // surfaced through the coroutine scheduler in some editor versions
            }
            finally
            {
                LogAssert.ignoreFailingMessages = previousIgnore;

                Assert.IsFalse(door.IsOpen,
                    "Restarting with a pending predecessor must close the shared transition door immediately.");
                Assert.IsTrue(door.BlockingCollider != null && door.BlockingCollider.enabled,
                    "A closed transition door must re-enable its blocking collider immediately.");
            }
        }
        finally
        {
            EditorSceneManager.CloseScene(shared, false);
            Destroy(flowObject);
        }
    }

    [Test]
    public void RequestRouteAdvanceDefersWhenBusyAndDrainsExactlyOnce()
    {
        var flowObject = new GameObject("Flow");
        var flow = flowObject.AddComponent<FormalGameFlowController>();
        InvokeEnsureRouteCatalog(flow);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(FormalGameFlowController).GetField("currentLevelScene", flags).SetValue(flow, "FormalLevel02");
        typeof(FormalGameFlowController).GetField("pendingUnloadScene", flags).SetValue(flow, "FormalLevel01");
        var operationField = typeof(FormalGameFlowController).GetField("operationInProgress", flags);
        operationField.SetValue(flow, true);

        // While busy the request must be recorded instead of executed or dropped.
        flow.RequestRouteAdvance();
        flow.RequestRouteAdvance();
        flow.RequestRouteAdvance();

        Assert.AreEqual("FormalLevel02", (string)typeof(FormalGameFlowController)
            .GetField("pendingAdvanceFromScene", flags).GetValue(flow),
            "Busy-window requests must collapse into a single retained slot.");

        operationField.SetValue(flow, false);
        LogAssert.Expect(LogType.Exception,
            "InvalidOperationException: This can only be used during play mode, please use EditorSceneManager.OpenScene() instead.");
        typeof(FormalGameFlowController)
            .GetMethod("DrainPendingAdvance", flags)
            .Invoke(flow, null);

        Assert.IsNull((string)typeof(FormalGameFlowController)
            .GetField("pendingAdvanceFromScene", flags).GetValue(flow),
            "The drained request must clear the slot exactly once.");

        Destroy(flowObject);
    }

    [Test]
    public void StaleDeferredAdvanceIsDiscardedWithoutLoadingAnything()
    {
        var flowObject = new GameObject("Flow");
        var flow = flowObject.AddComponent<FormalGameFlowController>();
        InvokeEnsureRouteCatalog(flow);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(FormalGameFlowController).GetField("currentLevelScene", flags).SetValue(flow, "FormalLevel03");
        typeof(FormalGameFlowController).GetField("pendingAdvanceFromScene", flags).SetValue(flow, "FormalLevel01");

        typeof(FormalGameFlowController)
            .GetMethod("DrainPendingAdvance", flags)
            .Invoke(flow, null);

        Assert.IsNull((string)typeof(FormalGameFlowController)
            .GetField("pendingAdvanceFromScene", flags).GetValue(flow),
            "A stale-origin deferred request must be dropped.");

        Destroy(flowObject);
    }

    [Test]
    public void RegisteredDoorTokenOverridesGenericSubstringOrder()
    {
        var flowObject = new GameObject("Flow");
        var flow = flowObject.AddComponent<FormalGameFlowController>();
        InvokeEnsureRouteCatalog(flow);

        Scene shared = EditorSceneManager.OpenScene(SharedArtL0102Path, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(shared);
        var extraHolder = new GameObject("SpecialGate_A");
        extraHolder.AddComponent<FormalDoor>();
        try
        {
            var catalog = (IList)typeof(FormalGameFlowController)
                .GetField("routeCatalog", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(flow);
            typeof(FormalGameFlowController.FormalRouteEntry)
                .GetField("arrivalTransitionDoorName")
                .SetValue(catalog[1], "SpecialGate");

            var found = InvokeFindTransitionDoor(flow, "FormalLevel01", "FormalLevel02");

            Assert.AreEqual(extraHolder.GetComponent<FormalDoor>(), found,
                "The registered token must win over the generic ToLevel substring.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(extraHolder);
            EditorSceneManager.CloseScene(shared, false);
            Destroy(flowObject);
        }
    }

    [Test]
    public void NotifyCheckpointActivatedStartsSequenceOnlyForPolicyLevels()
    {
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var plainObject = new GameObject("FlowPlain");
        var plain = plainObject.AddComponent<FormalGameFlowController>();
        InvokeEnsureRouteCatalog(plain);
        typeof(FormalGameFlowController).GetField("currentLevelScene", flags).SetValue(plain, "FormalLevel03");

        plain.NotifyCheckpointActivated("FormalLevel03");
        Assert.IsNull((Coroutine)typeof(FormalGameFlowController)
                .GetField("level045PursuitRoutine", flags).GetValue(plain),
            "Checkpoints in levels without the arrival policy must not start any sequence.");
        Destroy(plainObject);

        var policyObject = new GameObject("FlowPolicy");
        var policy = policyObject.AddComponent<FormalGameFlowController>();
        InvokeEnsureRouteCatalog(policy);
        typeof(FormalGameFlowController).GetField("currentLevelScene", flags).SetValue(policy, "FormalLevel045");
        typeof(FormalGameFlowController).GetField("pendingUnloadScene", flags).SetValue(policy, "FormalLevel04");

        policy.NotifyCheckpointActivated("FormalLevel045");
        Assert.IsNotNull((Coroutine)typeof(FormalGameFlowController)
                .GetField("level045PursuitRoutine", flags).GetValue(policy),
            "Activating the Level04.5 checkpoint must start the pursuit sequence.");

        Destroy(policyObject);
    }

    private static void Destroy(UnityEngine.Object target)
    {
        UnityEngine.Object.DestroyImmediate(target);
    }

    private static FormalDoor FindTransitionDoorInScene(Scene scene, string token)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (FormalDoor door in root.GetComponentsInChildren<FormalDoor>(true))
                if (door.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    return door;

        return null;
    }

    private static FormalDoor InvokeFindTransitionDoor(FormalGameFlowController flow, string fromScene, string toScene)
    {
        return (FormalDoor)typeof(FormalGameFlowController)
            .GetMethod("FindTransitionDoor", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(flow, new object[] { fromScene, toScene });
    }

    private static void InvokeEnsureRouteCatalog(FormalGameFlowController flow)
    {
        typeof(FormalGameFlowController)
            .GetMethod("EnsureRouteCatalog", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(flow, null);
    }

    private static object GetEntryField(object entry, string fieldName)
    {
        return typeof(FormalGameFlowController.FormalRouteEntry)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .GetValue(entry);
    }
}
