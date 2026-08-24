using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class FormalLevel02TransitionDiagnosticsTests
{
    [Test]
    public void Level02TransitionDiagnosticsAreDisabledByDefaultAndScopedToLevel02()
    {
        var flowObject = new GameObject("Flow");
        var flow = flowObject.AddComponent<FormalGameFlowController>();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;

        try
        {
            typeof(FormalGameFlowController).GetField("currentLevelScene", flags)
                .SetValue(flow, "FormalLevel02");
            var isDiagnosing = typeof(FormalGameFlowController)
                .GetMethod("IsDiagnosingLevel02Transition", flags);

            Assert.IsFalse((bool)isDiagnosing.Invoke(flow, null),
                "Diagnostics must remain off unless the FormalPersistent setting enables them.");

            typeof(FormalGameFlowController).GetField("level02TransitionDiagnostics", flags)
                .SetValue(flow, true);
            Assert.IsTrue((bool)isDiagnosing.Invoke(flow, null));

            typeof(FormalGameFlowController).GetField("currentLevelScene", flags)
                .SetValue(flow, "FormalLevel03");
            Assert.IsFalse((bool)isDiagnosing.Invoke(flow, null),
                "The Level 2 diagnostic must not produce unrelated Level 3 output.");
        }
        finally
        {
            Object.DestroyImmediate(flowObject);
        }
    }
}
