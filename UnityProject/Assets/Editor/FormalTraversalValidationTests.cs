using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FormalTraversalValidationTests
{
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
}
