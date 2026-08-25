using UnityEngine;

/// <summary>
/// 人形角色捡的钥匙。
///
/// 原本的写法是「捡到就直接 RequestRouteAdvance()」——钥匙一到手关卡就推进了，
/// 玩家根本没有「拿钥匙 → 走去开门 → 穿过门」这个过程。
/// 现在改成：捡到钥匙只负责把对应的门解锁，推进关卡交给门后面的
/// FormalRouteAdvanceTrigger。
/// </summary>
public class FormalHumanKey : MonoBehaviour, IFormalLevelTemporaryState
{
    [Header("这把钥匙开哪扇门")]
    [Tooltip("捡到钥匙时打开这些门。在 Inspector 里把出口门拖进来。")]
    [SerializeField] private FormalDoor[] doorsToUnlock;

    [Header("怎么捡")]
    [Tooltip("勾上=走到钥匙旁边按 E 才捡起来（和玩法介绍图一致）。\n取消=碰到就自动捡。")]
    [SerializeField] private bool requireInteractToCollect = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string promptPickup = "按 E 捡起钥匙";

    [Header("怎么开门")]
    [Tooltip("勾上=捡到钥匙那一刻门直接开。\n" +
             "取消=只是把钥匙记为“已获得”，玩家还得走到门口按 E 才开门（需要门上挂 FormalDoorInteraction）。")]
    [SerializeField] private bool openDoorsImmediately;

    [Header("兼容旧行为")]
    [Tooltip("勾上=捡到钥匙直接进下一关（旧的偷懒做法）。\n" +
             "默认关闭：拿到钥匙不等于通关，过关要靠走过门。")]
    [SerializeField] private bool advanceRouteOnPickup;

    [Header("Wwise Audio")]
    [Tooltip("Play_Key_Pickup. Using an AK.Wwise.Event reference also loads its AutoBank.")]
    [SerializeField] private AK.Wwise.Event pickupEvent = new AK.Wwise.Event();

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool collected;
    private bool warnedMissingPickupEvent;

    /// <summary>钥匙是否已被捡起。门上的 FormalDoorInteraction 读这个来决定能不能开。</summary>
    public bool IsCollected => collected;

    void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    private bool humanInRange;
    private GameObject pickupEmitter;

    void OnTriggerEnter(Collider other)
    {
        if (collected || !FormalLevelActors.IsHuman(other))
            return;

        FormalPlayerActor human = FormalLevelActors.ResolvePlayer(other);
        pickupEmitter = human != null ? human.gameObject : other.gameObject;

        if (requireInteractToCollect)
        {
            humanInRange = true;
            if (!string.IsNullOrEmpty(promptPickup) && FormalHUDController.Instance != null)
                FormalHUDController.Instance.ShowHint(promptPickup);
            return;
        }

        Collect();
    }

    void OnTriggerExit(Collider other)
    {
        if (FormalLevelActors.IsHuman(other))
        {
            humanInRange = false;
            pickupEmitter = null;
        }
    }

    void Update()
    {
        if (collected || !requireInteractToCollect || !humanInRange)
            return;

        if (Input.GetKeyDown(interactKey))
            Collect();
    }

    void Collect()
    {
        if (collected)
            return;

        collected = true;
        humanInRange = false;

        // Post on the player rather than the key: the key is hidden immediately below,
        // while the player's Wwise GameObject remains active for the whole one-shot.
        GameObject emitter = pickupEmitter != null ? pickupEmitter : gameObject;
        if (pickupEvent != null && pickupEvent.IsValid())
        {
            pickupEvent.Post(emitter);
        }
        else if (!warnedMissingPickupEvent)
        {
            Debug.LogWarning("[FormalHumanKey] Play_Key_Pickup is not assigned.", this);
            warnedMissingPickupEvent = true;
        }
        pickupEmitter = null;
        gameObject.SetActive(false);

        bool hasDoors = false;
        if (doorsToUnlock != null)
        {
            foreach (FormalDoor door in doorsToUnlock)
            {
                if (door == null)
                    continue;

                hasDoors = true;

                // 默认不直接开门：钥匙只是“解锁”，真正开门要玩家走到门口按 E
                if (openDoorsImmediately)
                    door.OpenPermanently();
            }
        }

        // Doors To Unlock 只在 openDoorsImmediately 模式下才需要填。
        // 正常流程里钥匙只是个「我拿到了」的标记，开门是门前的
        // FormalDoorInteraction 读 IsCollected 来判断的，这里空着完全正常。
        if (openDoorsImmediately && !hasDoors)
            Debug.LogWarning("[FormalHumanKey] 勾了 Open Doors Immediately 但 Doors To Unlock 是空的，" +
                             "捡到钥匙不会开任何门。", this);

        if (!openDoorsImmediately && FormalHUDController.Instance != null)
            FormalHUDController.Instance.ShowHint("拿到钥匙了。去门那边按 E 开门。");

        // 捡钥匙【绝对不】推进关卡。过关只能由门后面的
        // FormalRouteAdvanceTrigger 触发——拿到钥匙不等于通关。
        if (!advanceRouteOnPickup)
            return;

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.RequestRouteAdvance(this);
    }

    public void ResetTemporaryState()
    {
        collected = false;
        humanInRange = false;
        pickupEmitter = null;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        gameObject.SetActive(true);
    }
}
