using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景自有的实体门出口绑定。
///
/// 它在源关卡加载时，按目标关卡名把现有出口机关和箱子门切为“开门后预加载”，
/// 避免修改嵌套 Prefab 资源。目标关卡入口的 FormalLevelEntrySeal 负责双人到达确认。
/// </summary>
[AddComponentMenu("MoMing/Formal Physical Door Exit Binding")]
public class FormalPhysicalDoorExitBinding : MonoBehaviour
{
    [SerializeField] private string successorScene;

    void Awake()
    {
        if (string.IsNullOrEmpty(successorScene))
        {
            Debug.LogError("[PhysicalDoorTransition] exit-binding has no successor scene.", this);
            return;
        }

        int actuatorCount = 0;
        int crateDoorCount = 0;
        Scene scene = gameObject.scene;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (FormalActuatorTrigger trigger in root.GetComponentsInChildren<FormalActuatorTrigger>(true))
            {
                if (trigger.SuccessorScene != successorScene)
                    continue;

                trigger.SetPreloadRouteSuccessor(true);
                actuatorCount++;
            }

            foreach (FormalCrateDoorTrigger trigger in root.GetComponentsInChildren<FormalCrateDoorTrigger>(true))
            {
                trigger.SetPreloadRouteSuccessor(true);
                crateDoorCount++;
            }
        }

        if (actuatorCount == 0 && crateDoorCount == 0)
        {
            Debug.LogError(
                $"[PhysicalDoorTransition] exit-binding scene='{scene.name}' found no route-producing exit targeting '{successorScene}'.",
                this);
            return;
        }

        Debug.Log(
            $"[PhysicalDoorTransition] exit-binding scene='{scene.name}' target='{successorScene}' " +
            $"actuators={actuatorCount} crateDoors={crateDoorCount}.",
            this);
    }
}
