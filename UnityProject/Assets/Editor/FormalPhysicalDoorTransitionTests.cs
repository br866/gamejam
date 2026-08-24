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
