using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FormalTraversalValidationTests
{
    private struct FormalRouteScene
    {
        public readonly string Id;
        public readonly string Path;
        public readonly string ContentRoot;
        public readonly string CollisionRoot;

        public FormalRouteScene(string id, string path, string contentRoot, string collisionRoot)
        {
            Id = id;
            Path = path;
            ContentRoot = contentRoot;
            CollisionRoot = collisionRoot;
        }
    }

    private static readonly FormalRouteScene[] FormalRoute =
    {
        new FormalRouteScene("Level01", "Assets/MoMing/FormalLevels/FormalLevel01.unity", "Level01ContentRoot", "Level01CollisionRoot"),
        new FormalRouteScene("Level02", "Assets/MoMing/FormalLevels/FormalLevel02.unity", "Level02ContentRoot", "L02_CollisionRoot"),
        new FormalRouteScene("Level03", "Assets/MoMing/FormalLevels/FormalLevel03.unity", "Level03ContentRoot", "L03_CollisionRoot"),
        new FormalRouteScene("Level04", "Assets/MoMing/FormalLevels/FormalLevel04.unity", "Level04ContentRoot", "L04_CollisionRoot"),
        new FormalRouteScene("Level04.5", "Assets/MoMing/FormalLevels/FormalLevel045.unity", "Level045ContentRoot", "L045_CollisionRoot"),
        new FormalRouteScene("Level05", "Assets/MoMing/FormalLevels/FormalLevel05.unity", "Level05ContentRoot", "L05_CollisionRoot")
    };

    [Test]
    public void EveryFormalRouteSceneHasTheRequiredBaseContract()
    {
        var routeIds = new HashSet<string>();

        foreach (FormalRouteScene routeScene in FormalRoute)
        {
            Assert.IsTrue(routeIds.Add(routeScene.Id), $"Duplicate formal route id {routeScene.Id}.");
            Assert.IsTrue(EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == routeScene.Path),
                $"{routeScene.Path} is not enabled in Build Settings.");

            Scene scene = EditorSceneManager.OpenScene(routeScene.Path, OpenSceneMode.Additive);
            try
            {
                FormalLevelController[] controllers = FindInScene<FormalLevelController>(scene);
                Assert.AreEqual(1, controllers.Length, $"{scene.name} must have exactly one level controller.");
                Assert.AreEqual(routeScene.Id, controllers[0].LevelId, $"{scene.name} has an unexpected route id.");
                Assert.IsNotNull(GetControllerTransform(controllers[0], "humanSpawn"), $"{scene.name} has no human spawn reference.");
                Assert.IsNotNull(GetControllerTransform(controllers[0], "dogSpawn"), $"{scene.name} has no dog spawn reference.");
                Assert.IsNotNull(FindTransform(scene, routeScene.ContentRoot), $"{scene.name} has no content root.");
                Assert.IsNotNull(FindTransform(scene, routeScene.CollisionRoot), $"{scene.name} has no collision root.");
                Assert.AreEqual(0, FindInScene<FormalPlayerActors>(scene).Length,
                    $"{scene.name} must not contain a persistent player pair.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, false);
            }
        }
    }

    [Test]
    public void CheckpointStoresSeparateRespawnAnchors()
    {
        var levelObject = new GameObject("Level");
        var level = levelObject.AddComponent<FormalLevelController>();
        var humanAnchor = new GameObject("Human").transform;
        var dogAnchor = new GameObject("Dog").transform;
        humanAnchor.position = new Vector3(1f, 2f, 3f);
        dogAnchor.position = new Vector3(4f, 5f, 6f);

        level.SetCheckpoint(humanAnchor, dogAnchor);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var human = (Vector3)typeof(FormalLevelController).GetField("checkpointHuman", flags).GetValue(level);
        var dog = (Vector3)typeof(FormalLevelController).GetField("checkpointDog", flags).GetValue(level);

        Assert.AreEqual(humanAnchor.position, human);
        Assert.AreEqual(dogAnchor.position, dog);

        Object.DestroyImmediate(dogAnchor.gameObject);
        Object.DestroyImmediate(humanAnchor.gameObject);
        Object.DestroyImmediate(levelObject);
    }

    [Test]
    public void BothPlayersRequirementRequiresOneHumanAndOneDog()
    {
        var humanObject = new GameObject("Human");
        var dogObject = new GameObject("Dog");
        var human = humanObject.AddComponent<FormalPlayerActor>();
        var dog = dogObject.AddComponent<FormalPlayerActor>();
        var roleField = typeof(FormalPlayerActor).GetField("role", BindingFlags.Instance | BindingFlags.NonPublic);
        roleField.SetValue(human, FormalPlayerActor.ActorRole.Human);
        roleField.SetValue(dog, FormalPlayerActor.ActorRole.Dog);

        Assert.IsTrue(FormalTriggerEligibility.Accepts(humanObject.AddComponent<BoxCollider>(), FormalTriggerRequirement.BothPlayers));
        Assert.IsTrue(FormalTriggerEligibility.Accepts(dogObject.AddComponent<BoxCollider>(), FormalTriggerRequirement.BothPlayers));

        Object.DestroyImmediate(dogObject);
        Object.DestroyImmediate(humanObject);
    }

    [Test]
    public void PrerequisiteActuatorOpensOnlyAfterEveryMechanismCompletes()
    {
        var prerequisiteObject = new GameObject("Prerequisites");
        var first = prerequisiteObject.AddComponent<FormalMechanismState>();
        var second = prerequisiteObject.AddComponent<FormalMechanismState>();
        var actuator = prerequisiteObject.AddComponent<FormalPrerequisiteActuator>();
        var door = new GameObject("Door").AddComponent<FormalDoor>();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;

        typeof(FormalPrerequisiteActuator).GetField("prerequisites", flags)
            .SetValue(actuator, new[] { first, second });
        typeof(FormalPrerequisiteActuator).GetField("actuators", flags)
            .SetValue(actuator, new MonoBehaviour[] { door });

        typeof(FormalPrerequisiteActuator).GetMethod("Update", flags).Invoke(actuator, null);
        Assert.IsFalse(door.IsOpen);

        first.Complete();
        typeof(FormalPrerequisiteActuator).GetMethod("Update", flags).Invoke(actuator, null);
        Assert.IsFalse(door.IsOpen);

        second.Complete();
        typeof(FormalPrerequisiteActuator).GetMethod("Update", flags).Invoke(actuator, null);
        Assert.IsTrue(door.IsOpen);

        Object.DestroyImmediate(door.gameObject);
        Object.DestroyImmediate(prerequisiteObject);
    }

    [Test]
    public void OrderedMechanismRejectsOutOfOrderCompletion()
    {
        var objectWithStates = new GameObject("OrderedMechanism");
        var first = objectWithStates.AddComponent<FormalMechanismState>();
        var second = objectWithStates.AddComponent<FormalMechanismState>();
        var completion = objectWithStates.AddComponent<FormalMechanismState>();
        var ordered = objectWithStates.AddComponent<FormalOrderedMechanism>();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;

        typeof(FormalOrderedMechanism).GetField("orderedStates", flags)
            .SetValue(ordered, new[] { first, second });
        typeof(FormalOrderedMechanism).GetField("completionState", flags)
            .SetValue(ordered, completion);

        ordered.CompleteNext(second);
        Assert.IsFalse(second.IsComplete);

        ordered.CompleteNext(first);
        ordered.CompleteNext(second);
        Assert.IsTrue(first.IsComplete);
        Assert.IsTrue(second.IsComplete);
        Assert.IsTrue(completion.IsComplete);

        Object.DestroyImmediate(objectWithStates);
    }

    [Test]
    public void MonsterSafeZoneSuppressesCapture()
    {
        var monsterObject = new GameObject("Monster");
        var monster = monsterObject.AddComponent<MonsterPatrol>();
        var playerObject = new GameObject("Player");
        playerObject.transform.position = Vector3.zero;
        monsterObject.transform.position = Vector3.zero;
        var safeZone = new GameObject("SafeZone").AddComponent<BoxCollider>();
        safeZone.transform.position = Vector3.zero;
        safeZone.size = Vector3.one * 4f;

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(MonsterPatrol).GetField("safeZones", flags).SetValue(monster, new[] { safeZone });
        var tryCatch = (bool)typeof(MonsterPatrol).GetMethod("TryCatch", flags).Invoke(monster, new object[] { playerObject.transform });

        Assert.IsFalse(tryCatch);

        Object.DestroyImmediate(safeZone.gameObject);
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(monsterObject);
    }

    [Test]
    public void GateImplementsFormalActuatorContract()
    {
        var gate = new GameObject("Gate").AddComponent<GateController>();

        Assert.IsInstanceOf<IFormalLevelActuator>(gate);
        gate.Open();
        Assert.IsTrue(gate.IsOpen);
        gate.Close();
        Assert.IsFalse(gate.IsOpen);

        Object.DestroyImmediate(gate.gameObject);
    }

    [Test]
    public void Level02DogPlateAndSafeZoneGateTheLevel03Exit()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/MoMing/FormalLevels/FormalLevel02.unity", OpenSceneMode.Additive);
        try
        {
            var dogPlate = FindTransform(scene, "L02_DogPlateTrigger");
            var safeZone = FindTransform(scene, "L02_CooperativeSafeZoneTrigger");
            var exitDoor = FindTransform(scene, "L02_ExitDoor_ToLevel03");
            var checkpoint = FindTransform(scene, "SuccessorCheckpoint");

            Assert.IsNotNull(dogPlate);
            Assert.IsNotNull(safeZone);
            Assert.IsNotNull(exitDoor);
            Assert.IsNotNull(checkpoint);

            var dogTrigger = dogPlate.GetComponent<FormalActuatorTrigger>();
            var safeZoneTrigger = safeZone.GetComponent<FormalActuatorTrigger>();
            var safeZoneCollider = safeZone.GetComponent<BoxCollider>();
            var formalCheckpoint = checkpoint.GetComponent<FormalCheckpoint>();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            Assert.AreEqual(FormalTriggerRequirement.DogOnly,
                typeof(FormalActuatorTrigger).GetField("requirement", flags).GetValue(dogTrigger));
            Assert.AreEqual(FormalTriggerRequirement.BothPlayers,
                typeof(FormalActuatorTrigger).GetField("requirement", flags).GetValue(safeZoneTrigger));
            Assert.IsTrue(safeZoneCollider.isTrigger);
            var safeZonePrerequisites = (FormalMechanismState[])typeof(FormalActuatorTrigger)
                .GetField("prerequisites", flags).GetValue(safeZoneTrigger);
            Assert.AreEqual(0, safeZonePrerequisites.Length);
            var dogPlateActuators = (MonoBehaviour[])typeof(FormalActuatorTrigger)
                .GetField("actuators", flags).GetValue(dogTrigger);
            Assert.AreEqual(1, dogPlateActuators.Length);
            Assert.AreEqual(exitDoor.GetComponent<FormalDoor>(), dogPlateActuators[0]);

            var safeZoneActuators = (MonoBehaviour[])typeof(FormalActuatorTrigger)
                .GetField("actuators", flags).GetValue(safeZoneTrigger);
            Assert.AreEqual(0, safeZoneActuators.Length);

            var checkpointPrerequisites = (FormalMechanismState[])typeof(FormalCheckpoint)
                .GetField("prerequisites", flags).GetValue(formalCheckpoint);
            Assert.IsNotNull(checkpointPrerequisites);
            Assert.AreEqual(0, checkpointPrerequisites.Length);

            Assert.IsTrue((bool)typeof(FormalCheckpoint)
                .GetField("successorRegistrationPoint", flags).GetValue(formalCheckpoint));
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, false);
        }
    }

    [Test]
    public void CooperativeRailMoverAllowsHumanOnlyUnlimitedTravel()
    {
        float originalFixedDeltaTime = Time.fixedDeltaTime;
        Time.fixedDeltaTime = 0.02f;
        var moverObject = new GameObject("Mover");
        moverObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        moverObject.AddComponent<BoxCollider>();
        moverObject.AddComponent<Rigidbody>();
        var mover = moverObject.AddComponent<FormalCooperativeRailMover>();
        var rightHumanPoint = CreatePoint(moverObject.transform, "RightHumanPoint", Vector3.right);
        var leftHumanPoint = CreatePoint(moverObject.transform, "LeftHumanPoint", Vector3.left);
        var frontHumanPoint = CreatePoint(moverObject.transform, "FrontHumanPoint", Vector3.forward);
        var backHumanPoint = CreatePoint(moverObject.transform, "BackHumanPoint", Vector3.back);

        var human = CreateFormalActor("Human", FormalPlayerActor.ActorRole.Human, leftHumanPoint.position);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var groups = new[]
        {
            new FormalCooperativeRailMover.DirectionPointGroup { humanPoint = rightHumanPoint },
            new FormalCooperativeRailMover.DirectionPointGroup { humanPoint = leftHumanPoint },
            new FormalCooperativeRailMover.DirectionPointGroup { humanPoint = frontHumanPoint },
            new FormalCooperativeRailMover.DirectionPointGroup { humanPoint = backHumanPoint }
        };
        typeof(FormalCooperativeRailMover).GetField("directionGroups", flags).SetValue(mover, groups);
        typeof(FormalCooperativeRailMover).GetMethod("Awake", flags).Invoke(mover, null);
        Assert.IsTrue(mover.TryEngage(human));
        mover.Move(Vector3.right);
        var travelField = typeof(FormalCooperativeRailMover).GetField("travel", flags);
        Assert.Greater((float)travelField.GetValue(mover), 0f);

        for (int i = 0; i < 100; i++)
            mover.Move(Vector3.right);
        Assert.Greater((float)travelField.GetValue(mover), 1.001f);

        mover.SetAttachedPullAnimation();
        Assert.AreEqual(FormalPlayerActor.ActorState.Idle, human.State);

        float travelBeforeBackwardMove = (float)travelField.GetValue(mover);
        mover.Move(Vector3.left, false);
        Assert.Less((float)travelField.GetValue(mover), travelBeforeBackwardMove);
        Assert.AreEqual(FormalPlayerActor.ActorState.Idle, human.State);

        mover.Cancel();
        Assert.IsFalse(mover.IsEngaged);
        mover.ResetTemporaryState();

        human.SetPosition(frontHumanPoint.position);
        Physics.SyncTransforms();
        Assert.IsTrue(mover.TryEngage(human));
        var originField = typeof(FormalCooperativeRailMover).GetField("movementOrigin", flags);
        Assert.AreEqual(0f, (float)travelField.GetValue(mover), 0.001f);
        Assert.AreEqual(moverObject.transform.position, (Vector3)originField.GetValue(mover));
        mover.Move(Vector3.forward);
        Assert.Greater((float)travelField.GetValue(mover), 0f);
        mover.Cancel();

        Object.DestroyImmediate(rightHumanPoint.gameObject);
        Object.DestroyImmediate(leftHumanPoint.gameObject);
        Object.DestroyImmediate(frontHumanPoint.gameObject);
        Object.DestroyImmediate(backHumanPoint.gameObject);
        Object.DestroyImmediate(human.gameObject);
        Object.DestroyImmediate(moverObject);
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }

    [TestCase("Assets/MoMing/FormalLevels/FormalLevel01.unity")]
    [TestCase("Assets/MoMing/FormalLevels/FormalLevel02.unity")]
    public void EntranceAndCheckpointAnchorsAreSupported(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        Physics.SyncTransforms();

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var anchor in root.GetComponentsInChildren<Transform>(true))
            {
                if (anchor.name != "HumanSpawn" && anchor.name != "DogSpawn" &&
                    anchor.name != "HumanRespawnAnchor" && anchor.name != "DogRespawnAnchor")
                    continue;

                Assert.IsTrue(
                    Physics.Raycast(anchor.position + Vector3.up * 0.1f, Vector3.down, 2f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore),
                    $"{scene.name}/{anchor.name} has no supporting collider.");

                var overlaps = Physics.OverlapCapsule(
                    anchor.position + Vector3.up * 0.51f,
                    anchor.position + Vector3.up * 1.49f,
                    0.49f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);

                foreach (var overlap in overlaps)
                    Assert.IsTrue(overlap.isTrigger, $"{scene.name}/{anchor.name} overlaps {overlap.name}.");
            }
        }

        EditorSceneManager.CloseScene(scene, false);
    }

    [TestCase("Assets/MoMing/FormalLevels/FormalLevel01.unity")]
    [TestCase("Assets/MoMing/FormalLevels/FormalLevel02.unity")]
    public void EntranceToCheckpointSegmentIsClear(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        Physics.SyncTransforms();

        Transform humanSpawn = null;
        Transform humanCheckpoint = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == "HumanSpawn")
                    humanSpawn = transform;
                else if (transform.name == "HumanRespawnAnchor")
                    humanCheckpoint = transform;
            }
        }

        Assert.IsNotNull(humanSpawn, $"{scene.name} has no human entrance anchor.");
        Assert.IsNotNull(humanCheckpoint, $"{scene.name} has no human checkpoint anchor.");

        var direction = humanCheckpoint.position - humanSpawn.position;
        Assert.IsFalse(
            Physics.CapsuleCast(
                humanSpawn.position + Vector3.up * 0.51f,
                humanSpawn.position + Vector3.up * 1.49f,
                0.49f,
                direction.normalized,
                direction.magnitude,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore),
            $"{scene.name} entrance-to-checkpoint route is blocked.");

        EditorSceneManager.CloseScene(scene, false);
    }

    [Test]
    public void L01MechanismPedalUsesTriggerPrefabAndDoesNotBlock()
    {
        const string contentPath = "Assets/MoMing/FormalLevels/Prefabs/L01_Content.prefab";
        const string pedalPath = "Assets/MoMing/FormalLevels/Prefabs/L01_Mechanism_Pedal.prefab";

        var content = AssetDatabase.LoadAssetAtPath<GameObject>(contentPath);
        var pedalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pedalPath);
        Assert.IsNotNull(content, "L01 content prefab is missing.");
        Assert.IsNotNull(pedalPrefab, "L01 mechanism pedal prefab is missing.");

        var contentPedal = FindChild(content.transform, "L01_Mechanism_Pedal");
        var prefabPedal = FindChild(pedalPrefab.transform, "L01_Mechanism_Pedal");
        Assert.IsNotNull(contentPedal, "L01 content does not contain the mechanism pedal instance.");
        Assert.IsNotNull(prefabPedal, "L01 mechanism prefab has no root pedal object.");

        var contentCollider = contentPedal.GetComponent<Collider>();
        var prefabCollider = prefabPedal.GetComponent<Collider>();
        Assert.IsNotNull(contentCollider, "L01 content pedal has no detection collider.");
        Assert.IsNotNull(prefabCollider, "L01 pedal prefab has no detection collider.");
        Assert.IsTrue(contentCollider.isTrigger, "L01 content pedal detection collider must be a trigger.");
        Assert.IsTrue(prefabCollider.isTrigger, "L01 pedal prefab detection collider must be a trigger.");
        Assert.IsNotNull(contentPedal.GetComponent<FormalMechanismPedal>(), "L01 content pedal has no mechanism behavior.");
        Assert.IsNotNull(prefabPedal.GetComponent<FormalMechanismPedal>(), "L01 pedal prefab has no mechanism behavior.");
    }

    [Test]
    public void FormalMechanismPedalPrefabHasNoSolidChildCollider()
    {
        var pedalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/MoMing/FormalLevels/Prefabs/L01_Mechanism_Pedal.prefab");
        Assert.IsNotNull(pedalPrefab, "L01 mechanism pedal prefab is missing.");

        foreach (var collider in pedalPrefab.GetComponentsInChildren<Collider>(true))
            Assert.IsTrue(collider.isTrigger, $"Mechanism pedal collider {collider.name} must be trigger-only.");
    }

    [Test]
    public void L01HumanKeyLoadsFormalLevel02Directly()
    {
        var keyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/MoMing/FormalLevels/Prefabs/L01_HumanKey.prefab");
        Assert.IsNotNull(keyPrefab, "L01 human key prefab is missing.");

        var key = keyPrefab.GetComponent<FormalHumanKey>();
        var trigger = keyPrefab.GetComponent<Collider>();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var successorScene = (string)typeof(FormalHumanKey)
            .GetField("successorScene", flags)
            .GetValue(key);

        Assert.IsNotNull(key, "L01 human key has no FormalHumanKey behavior.");
        Assert.IsNotNull(trigger, "L01 human key has no trigger collider.");
        Assert.IsTrue(trigger.isTrigger, "L01 human key collider must be a trigger.");
        Assert.AreEqual("FormalLevel02", successorScene,
            "L01 human key must load FormalLevel02 directly.");
    }

    [Test]
    public void FormalFlowKeepsPredecessorPendingUntilRestart()
    {
        var flowObject = new GameObject("Flow");
        var flow = flowObject.AddComponent<FormalGameFlowController>();
        var pendingField = typeof(FormalGameFlowController)
            .GetField("pendingUnloadScene", BindingFlags.Instance | BindingFlags.NonPublic);
        var confirmedField = typeof(FormalGameFlowController)
            .GetField("successorArrivalConfirmed", BindingFlags.Instance | BindingFlags.NonPublic);
        var currentField = typeof(FormalGameFlowController)
            .GetField("currentLevelScene", BindingFlags.Instance | BindingFlags.NonPublic);

        pendingField.SetValue(flow, "FormalLevel01");
        currentField.SetValue(flow, "FormalLevel02");
        typeof(FormalGameFlowController)
            .GetMethod("NotifySuccessorCheckpointActivated", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(flow, new object[] { "FormalLevel02" });

        Assert.AreEqual("FormalLevel01", pendingField.GetValue(flow));
        Assert.IsTrue((bool)confirmedField.GetValue(flow));

        Object.DestroyImmediate(flowObject);
    }

    [Test]
    public void KeypadSixShortcutExposesCurrentDoorScopeOperation()
    {
        Assert.IsNotNull(typeof(FormalGameFlowController).GetMethod(
            "OpenAllDoorsInCurrentLevelScope", BindingFlags.Instance | BindingFlags.Public));
    }

    static T[] FindInScene<T>(Scene scene) where T : Component
    {
        var results = new List<T>();
        foreach (GameObject root in scene.GetRootGameObjects())
            results.AddRange(root.GetComponentsInChildren<T>(true));
        return results.ToArray();
    }

    static Transform CreatePoint(Transform parent, string name, Vector3 position)
    {
        var point = new GameObject(name).transform;
        point.SetParent(parent);
        point.localPosition = position;
        return point;
    }


    static Transform FindTransform(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == name)
                    return transform;
            }
        }

        return null;
    }

    static Transform FindChild(Transform root, string name)
    {
        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == name)
                return transform;
        }

        return null;
    }

    static Transform GetControllerTransform(FormalLevelController controller, string fieldName)
    {
        var field = typeof(FormalLevelController).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return field != null ? field.GetValue(controller) as Transform : null;
    }

    static FormalPlayerActor CreateFormalActor(string name, FormalPlayerActor.ActorRole role, Vector3 position)
    {
        var actor = new GameObject(name).AddComponent<FormalPlayerActor>();
        actor.transform.position = position;
        typeof(FormalPlayerActor).GetField("role", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(actor, role);
        typeof(FormalPlayerActor).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(actor, null);
        return actor;
    }
}
