using FMODUnity;
using UnityEngine;

public class FrisbeeFmodSoundDirectory : MonoBehaviour
{
    public static FrisbeeFmodSoundDirectory Instance { get; private set; }

    [field: SerializeField] public EventReference ThrowFrisbee { get; private set; }
    [field: SerializeField] public EventReference ChargeThrowPower { get; private set; }
    [field: SerializeField] public EventReference Flying { get; private set; }
    [field: SerializeField] public EventReference MenuOpen { get; private set; }
    [field: SerializeField] public EventReference MenuClose { get; private set; }
    [field: SerializeField] public EventReference MenuHover { get; private set; }
    [field: SerializeField] public EventReference MenuClick { get; private set; }
    [field: SerializeField] public EventReference TargetBreak { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Found more than one FrisbeeFmodSoundDirectory in the scene.", this);
            return;
        }

        Instance = this;
    }
}
