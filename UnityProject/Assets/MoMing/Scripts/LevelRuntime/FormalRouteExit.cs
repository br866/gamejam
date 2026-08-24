using UnityEngine;

public class FormalRouteExit : MonoBehaviour, IFormalLevelActuator
{
    public void Open()
    {
        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.CompleteRoute();
    }

    public void Close()
    {
    }
}