using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FormalPhysicalDoorTransitionTests
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [Test]
    public void PhysicalArrivalRequiresMatchingPendingTransitionAndDoesNotMovePlayers()
    {
        GameObject flowObject = new GameObject("Flow");
        FormalGameFlowController flow = flowObject.AddComponent<FormalGameFlowController>();
        GameObject playerObject = new GameObject("Player");
        playerObject.transform.position = new Vector3(12f, 3f, -8f);
        Scene activeScene = SceneManager.GetActiveScene();

        try
        {
            Assert.IsFalse(flow.ConfirmPreloadedPhysicalArrival(activeScene.name),
                "An arrival must not be accepted without a matching preload.");

            SetPrivate(flow, "currentLevelScene", "FormalLevel02");
            SetPrivate(flow, "pendingPhysicalTransitionFromScene", "FormalLevel02");
            SetPrivate(flow, "pendingPhysicalTransitionToScene", activeScene.name);

            Vector3 positionBefore = playerObject.transform.position;
            Assert.IsTrue(flow.ConfirmPreloadedPhysicalArrival(activeScene.name));
            Assert.AreEqual(activeScene.name, flow.CurrentLevelScene);
            Assert.IsFalse(flow.HasPendingPhysicalTransition);
            Assert.AreEqual(positionBefore, playerObject.transform.position,
                "Physical arrival confirmation must not reposition players.");
        }
        finally
        {
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(flowObject);
        }
    }

    [Test]
    public void DirectLoadClearsPendingPhysicalTransition()
    {
        GameObject flowObject = new GameObject("Flow");
        FormalGameFlowController flow = flowObject.AddComponent<FormalGameFlowController>();

        try
        {
            SetPrivate(flow, "pendingPhysicalTransitionFromScene", "FormalLevel02");
            SetPrivate(flow, "pendingPhysicalTransitionToScene", "FormalLevel03");

            typeof(FormalGameFlowController)
                .GetMethod("CancelPendingPhysicalTransition", PrivateInstance)
                .Invoke(flow, null);

            Assert.IsFalse(flow.HasPendingPhysicalTransition);
        }
        finally
        {
            Object.DestroyImmediate(flowObject);
        }
    }

    [Test]
    public void CooperativeTriggerCanPreloadSuccessorWithoutDirectAdvance()
    {
        GameObject flowObject = new GameObject("Flow");
        FormalGameFlowController flow = flowObject.AddComponent<FormalGameFlowController>();
        GameObject triggerObject = new GameObject("L3ExitTrigger");
        triggerObject.AddComponent<BoxCollider>().isTrigger = true;
        FormalActuatorTrigger trigger = triggerObject.AddComponent<FormalActuatorTrigger>();
        Scene activeScene = SceneManager.GetActiveScene();

        try
        {
            SetPrivate(flow, "currentLevelScene", "FormalLevel03");
            SetPrivate(flow, "routeCatalog", new[]
            {
                new FormalGameFlowController.FormalRouteEntry { sceneName = "FormalLevel03" },
                new FormalGameFlowController.FormalRouteEntry { sceneName = activeScene.name }
            });
            SetPrivate(trigger, "preloadRouteSuccessor", true);
            SetPrivate(trigger, "opensTransitionDoor", true);

            trigger.CompleteImmediately();

            Assert.IsTrue(flow.HasPendingPhysicalTransition,
                "Preload mode must establish a physical transition instead of directly advancing.");
            Assert.AreEqual("FormalLevel03", flow.CurrentLevelScene,
                "Preload mode must retain the originating level as current.");
        }
        finally
        {
            Object.DestroyImmediate(triggerObject);
            Object.DestroyImmediate(flowObject);
        }
    }

    [Test]
    public void Level03ExitSceneInstanceUsesPreloadInsteadOfDirectAdvance()
    {
        Scene scene = EditorSceneManager.OpenScene(
            "Assets/MoMing/FormalLevels/FormalLevel03.unity",
            OpenSceneMode.Additive);

        try
        {
            FormalActuatorTrigger exit = null;
            MonoBehaviour binding = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (binding == null)
                    binding = FindComponentByTypeName(root, "FormalPhysicalDoorExitBinding");

                foreach (FormalActuatorTrigger candidate in root.GetComponentsInChildren<FormalActuatorTrigger>(true))
                {
                    if ((string)GetPrivate(candidate, "successorScene") == "FormalLevel04")
                    {
                        exit = candidate;
                        break;
                    }
                }

                if (exit != null)
                    break;
            }

            Assert.IsNotNull(exit, "L3 must contain its cooperative exit trigger.");
            Assert.IsNotNull(binding, "L3 must contain a scene-owned physical door exit binding.");
            SetPrivate(exit, "preloadRouteSuccessor", false);
            binding.GetType().GetMethod("Awake", PrivateInstance).Invoke(binding, null);
            Assert.IsTrue((bool)GetPrivate(exit, "preloadRouteSuccessor"));
            Assert.IsFalse((bool)GetPrivate(exit, "opensTransitionDoor"));
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void FinalRouteScenesHavePhysicalExitBindingsForEveryRouteExit()
    {
        Scene level04 = EditorSceneManager.OpenScene(
            "Assets/MoMing/FormalLevels/FormalLevel04.unity",
            OpenSceneMode.Additive);
        Scene level045 = EditorSceneManager.OpenScene(
            "Assets/MoMing/FormalLevels/FormalLevel045.unity",
            OpenSceneMode.Additive);

        try
        {
            FormalPhysicalDoorExitBinding level04Binding = FindBinding(level04, "FormalLevel045");
            FormalPhysicalDoorExitBinding level045Binding = FindBinding(level045, "FormalLevel05");
            Assert.IsNotNull(level04Binding, "L4 must own a physical exit binding for L4.5.");
            Assert.IsNotNull(level045Binding, "L4.5 must own a physical exit binding for L5.");

            level04Binding.SendMessage("Awake");
            level045Binding.SendMessage("Awake");

            Assert.IsTrue(HasPreloadActuator(level04, "FormalLevel045"),
                "The L4 cooperative exit must be switched to preload mode.");
            Assert.IsTrue(HasPreloadActuator(level045, "FormalLevel05"),
                "The L4.5 cooperative exit must be switched to preload mode.");

            FormalCrateDoorTrigger crateExit = FindInScene<FormalCrateDoorTrigger>(level045);
            Assert.IsNotNull(crateExit, "L4.5 must retain its crate-door route exit.");
            Assert.IsTrue((bool)GetPrivate(crateExit, "preloadRouteSuccessor"),
                "The L4.5 crate-door route exit must be switched to preload mode.");
        }
        finally
        {
            EditorSceneManager.CloseScene(level045, true);
            EditorSceneManager.CloseScene(level04, true);
        }
    }

    [Test]
    public void Level045ArrivalSealAndRetainedPredecessorCompleteWithoutUnload()
    {
        GameObject flowObject = new GameObject("Flow");
        FormalGameFlowController flow = flowObject.AddComponent<FormalGameFlowController>();
        Scene level045 = EditorSceneManager.OpenScene(
            "Assets/MoMing/FormalLevels/FormalLevel045.unity",
            OpenSceneMode.Additive);
        Scene level05 = EditorSceneManager.OpenScene(
            "Assets/MoMing/FormalLevels/FormalLevel05.unity",
            OpenSceneMode.Additive);

        try
        {
            Assert.IsNotNull(FindInScene<FormalLevelEntrySeal>(level045),
                "L4.5 must own a two-player physical arrival seal.");
            Assert.IsNotNull(FindInScene<FormalLevelEntrySeal>(level05),
                "L5 must retain its two-player physical arrival seal.");

            SetPrivate(flow, "currentLevelScene", "FormalLevel045");
            SetPrivate(flow, "pendingUnloadScene", "FormalLevel04");
            SetPrivate(flow, "routeCatalog", new[]
            {
                new FormalGameFlowController.FormalRouteEntry { sceneName = "FormalLevel04", levelId = "Level04" },
                new FormalGameFlowController.FormalRouteEntry { sceneName = "FormalLevel045", levelId = "Level04.5" }
            });

            Assert.IsTrue(flow.SealPredecessorLevel(),
                "A retained predecessor must count as handled so the arrival seal can finish.");
            Assert.AreEqual("FormalLevel04", (string)GetPrivate(flow, "pendingUnloadScene"),
                "L4 must remain retained for the Level 4.5 pursuit sequence.");
        }
        finally
        {
            EditorSceneManager.CloseScene(level05, true);
            EditorSceneManager.CloseScene(level045, true);
            Object.DestroyImmediate(flowObject);
        }
    }

    [Test]
    public void RetainedLevel04StaysTrackedUntilLevel05Arrival()
    {
        GameObject flowObject = new GameObject("Flow");
        FormalGameFlowController flow = flowObject.AddComponent<FormalGameFlowController>();
        Scene level045 = SceneManager.CreateScene("FormalLevel045");
        Scene level05 = SceneManager.CreateScene("FormalLevel05");

        try
        {
            SetPrivate(flow, "routeCatalog", new[]
            {
                new FormalGameFlowController.FormalRouteEntry { sceneName = "FormalLevel04", levelId = "Level04" },
                new FormalGameFlowController.FormalRouteEntry { sceneName = "FormalLevel045", levelId = "Level04.5" },
                new FormalGameFlowController.FormalRouteEntry { sceneName = "FormalLevel05", levelId = "Level05" }
            });
            SetPrivate(flow, "currentLevelScene", "FormalLevel04");
            SetPrivate(flow, "pendingPhysicalTransitionFromScene", "FormalLevel04");
            SetPrivate(flow, "pendingPhysicalTransitionToScene", level045.name);

            Assert.IsTrue(flow.ConfirmPreloadedPhysicalArrival(level045.name));
            Assert.AreEqual("FormalLevel04", (string)GetPrivate(flow, "retainedPhysicalPredecessorScene"));
            Assert.IsNotNull((Coroutine)GetPrivate(flow, "level045PursuitRoutine"),
                "Confirming physical arrival in L4.5 must start its retained-Level-4 pursuit sequence.");

            SetPrivate(flow, "pendingPhysicalTransitionFromScene", level045.name);
            SetPrivate(flow, "pendingPhysicalTransitionToScene", level05.name);
            Assert.IsTrue(flow.ConfirmPreloadedPhysicalArrival(level05.name));
            Assert.AreEqual("FormalLevel04", (string)GetPrivate(flow, "retainedPhysicalPredecessorScene"),
                "Level 4 must stay tracked until Level 5 cleanup can unload it with Level 4.5.");
        }
        finally
        {
            Object.DestroyImmediate(flowObject);
        }
    }

    [Test]
    public void Level05CheckpointReleasesOnlyRetainedLevel04()
    {
        GameObject flowObject = new GameObject("Flow");
        FormalGameFlowController flow = flowObject.AddComponent<FormalGameFlowController>();
        Scene level05 = SceneManager.CreateScene("FormalLevel05");
        GameObject controlObject = new GameObject("PlayerControl");
        FormalPlayerControl control = controlObject.AddComponent<FormalPlayerControl>();

        try
        {
            SetPrivate(control, "humanOnly", true);
            SetPrivate(flow, "currentLevelScene", "FormalLevel045");
            SetPrivate(flow, "pendingPhysicalTransitionFromScene", "FormalLevel045");
            SetPrivate(flow, "pendingPhysicalTransitionToScene", "FormalLevel05");
            SetPrivate(flow, "pendingUnloadScene", "FormalLevel04");
            SetPrivate(flow, "retainedPhysicalPredecessorScene", "FormalLevel04");

            flow.NotifyCheckpointActivated("FormalLevel05");

            Assert.IsNull(GetPrivate(flow, "retainedPhysicalPredecessorScene"),
                "L05_Checkpoint must release the retained L4 scene reference.");
            Assert.IsTrue((bool)GetPrivate(flow, "retainedPredecessorReleasedAtLevel05Checkpoint"),
                "L05_Checkpoint must mark the retained-L4 cleanup path as complete.");
            Assert.AreEqual(level05.name, flow.CurrentLevelScene,
                "L05_Checkpoint must commit Level 5 as the recovery level without moving players.");
            Assert.IsFalse((bool)GetPrivate(control, "humanOnly"),
                "L05_Checkpoint must restore normal dog switching.");
            Assert.AreEqual("FormalLevel045", (string)GetPrivate(flow, "pendingUnloadScene"),
                "L05_Checkpoint must retain Level 4.5 as the predecessor scene.");
            SetPrivate(flow, "operationInProgress", false);
            Assert.IsTrue(flow.SealPredecessorLevel(),
                "The L5 arrival seal must finish without unloading Level 4.5 after checkpoint cleanup.");
            Assert.AreEqual("FormalLevel045", (string)GetPrivate(flow, "pendingUnloadScene"),
                "The L5 arrival seal must preserve Level 4.5 on the checkpoint-cleanup path.");
        }
        finally
        {
            Object.DestroyImmediate(controlObject);
            Object.DestroyImmediate(flowObject);
        }
    }

    [Test]
    public void ForcedPursuitSelectsAndRetargetsNearestFormalActor()
    {
        GameObject monsterObject = new GameObject("Monster");
        MonsterPatrol monster = monsterObject.AddComponent<MonsterPatrol>();
        GameObject human = new GameObject("Human");
        GameObject dog = new GameObject("Dog");

        try
        {
            monsterObject.transform.position = Vector3.zero;
            human.transform.position = new Vector3(8f, 0f, 0f);
            dog.transform.position = new Vector3(2f, 0f, 0f);

            monster.BeginForcedChase(human.transform, dog.transform);
            Assert.AreSame(dog.transform, GetPrivate(monster, "chaseTarget"),
                "Forced pursuit must initially select the nearer dog.");

            human.transform.position = new Vector3(1f, 0f, 0f);
            typeof(MonsterPatrol).GetMethod("ForcedChase", PrivateInstance).Invoke(monster, null);
            Assert.AreSame(human.transform, GetPrivate(monster, "chaseTarget"),
                "Forced pursuit must retarget the human when it becomes nearer.");
        }
        finally
        {
            Object.DestroyImmediate(dog);
            Object.DestroyImmediate(human);
            Object.DestroyImmediate(monsterObject);
        }
    }

    private static FormalPhysicalDoorExitBinding FindBinding(Scene scene, string successorScene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (FormalPhysicalDoorExitBinding binding in root.GetComponentsInChildren<FormalPhysicalDoorExitBinding>(true))
                if ((string)GetPrivate(binding, "successorScene") == successorScene)
                    return binding;
        }

        return null;
    }

    private static bool HasPreloadActuator(Scene scene, string successorScene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (FormalActuatorTrigger trigger in root.GetComponentsInChildren<FormalActuatorTrigger>(true))
                if ((string)GetPrivate(trigger, "successorScene") == successorScene &&
                    (bool)GetPrivate(trigger, "preloadRouteSuccessor"))
                    return true;
        }

        return false;
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T result = root.GetComponentInChildren<T>(true);
            if (result != null)
                return result;
        }

        return null;
    }

    private static void SetPrivate(object target, string fieldName, object value)
    {
        target.GetType().GetField(fieldName, PrivateInstance).SetValue(target, value);
    }

    private static object GetPrivate(object target, string fieldName)
    {
        return target.GetType().GetField(fieldName, PrivateInstance).GetValue(target);
    }

    private static MonoBehaviour FindComponentByTypeName(GameObject root, string typeName)
    {
        foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
            if (component != null && component.GetType().Name == typeName)
                return component;

        return null;
    }
}
