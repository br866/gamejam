using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class FormalCheckpointLevelRegistrationTests
{
    [Test]
    public void CheckpointRegistersOnlyOnceForItsOwningScene()
    {
        var checkpointObject = new GameObject("Checkpoint");
        var checkpoint = checkpointObject.AddComponent<FormalCheckpoint>();
        string sceneName = SceneManager.GetActiveScene().name;

        try
        {
            typeof(FormalCheckpoint)
                .GetField("owningLevelScene", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(checkpoint, sceneName);

            Assert.IsTrue(checkpoint.RegisterWithOwningLevel());
            Assert.IsFalse(checkpoint.RegisterWithOwningLevel());
            Assert.IsTrue(checkpoint.IsRegisteredWithOwningLevel);
        }
        finally
        {
            Object.DestroyImmediate(checkpointObject);
        }
    }

    [Test]
    public void CheckpointRejectsRegistrationForAnotherScene()
    {
        var checkpointObject = new GameObject("Checkpoint");
        var checkpoint = checkpointObject.AddComponent<FormalCheckpoint>();

        try
        {
            typeof(FormalCheckpoint)
                .GetField("owningLevelScene", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(checkpoint, "FormalLevelMissing");

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("belongs to 'FormalLevelMissing'"));
            Assert.IsFalse(checkpoint.RegisterWithOwningLevel());
            Assert.IsFalse(checkpoint.IsRegisteredWithOwningLevel);
        }
        finally
        {
            Object.DestroyImmediate(checkpointObject);
        }
    }
}
