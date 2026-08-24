using NUnit.Framework;
using UnityEngine;

public class FormalLevel02GmGateToolsTests
{
    [Test]
    public void DogRuntimeSpeedMultiplierDefaultsToOneAndCanBeRestored()
    {
        GameObject dogObject = new GameObject("Dog");
        dogObject.AddComponent<Rigidbody>();
        FormalPlayerActor dog = dogObject.AddComponent<FormalPlayerActor>();

        try
        {
            Assert.AreEqual(1f, dog.RuntimeMovementSpeedMultiplier);
            dog.SetRuntimeMovementSpeedMultiplier(5f);
            Assert.AreEqual(5f, dog.RuntimeMovementSpeedMultiplier);
            dog.SetRuntimeMovementSpeedMultiplier(1f);
            Assert.AreEqual(1f, dog.RuntimeMovementSpeedMultiplier);
        }
        finally
        {
            Object.DestroyImmediate(dogObject);
        }
    }
}
