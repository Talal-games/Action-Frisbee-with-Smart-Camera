using UnityEngine;

public class WorldMenuPanel : MonoBehaviour
{
    [SerializeField] private Transform panelRoot;
    [SerializeField] private Transform hiddenAbovePosition;
    [SerializeField] private Transform visiblePosition;
    [SerializeField] private float slideSpeed = 8f;

    private Vector3 targetLocalPosition;
    public bool IsMoving => Vector3.Distance(panelRoot.localPosition, targetLocalPosition) > 0.001f;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = transform;
        }

        targetLocalPosition = panelRoot.localPosition;
    }

    private void Update()
    {
        panelRoot.localPosition = Vector3.MoveTowards(
            panelRoot.localPosition,
            targetLocalPosition,
            slideSpeed * Time.unscaledDeltaTime);

        Physics.SyncTransforms();
    }

    public void ShowFromAbove()
    {
        if (hiddenAbovePosition != null)
        {
            panelRoot.localPosition = hiddenAbovePosition.localPosition;
            Physics.SyncTransforms();
        }

        MoveToVisible();
        PlayMenuSound(FrisbeeFmodSoundDirectory.Instance != null ? FrisbeeFmodSoundDirectory.Instance.MenuOpen : default);
    }

    public void HideDown()
    {
        targetLocalPosition = GetHiddenBelowLocalPosition();
        PlayMenuSound(FrisbeeFmodSoundDirectory.Instance != null ? FrisbeeFmodSoundDirectory.Instance.MenuClose : default);
    }

    public void HideUp()
    {
        if (hiddenAbovePosition == null) return;

        targetLocalPosition = hiddenAbovePosition.localPosition;
        PlayMenuSound(FrisbeeFmodSoundDirectory.Instance != null ? FrisbeeFmodSoundDirectory.Instance.MenuClose : default);
    }

    public void MoveToVisible()
    {
        if (visiblePosition == null) return;

        targetLocalPosition = visiblePosition.localPosition;
    }

    public void MoveToHiddenBelow()
    {
        targetLocalPosition = GetHiddenBelowLocalPosition();
    }

    public void SetInstantVisible()
    {
        if (visiblePosition == null) return;

        panelRoot.localPosition = visiblePosition.localPosition;
        targetLocalPosition = panelRoot.localPosition;
        Physics.SyncTransforms();
    }

    public void SetInstantHiddenAbove()
    {
        if (hiddenAbovePosition == null) return;

        panelRoot.localPosition = hiddenAbovePosition.localPosition;
        targetLocalPosition = panelRoot.localPosition;
        Physics.SyncTransforms();
    }

    public void SetActive(bool active)
    {
        panelRoot.gameObject.SetActive(active);
    }

    private Vector3 GetHiddenBelowLocalPosition()
    {
        if (hiddenAbovePosition == null || visiblePosition == null)
        {
            return panelRoot.localPosition;
        }

        Vector3 visibleToAbove = hiddenAbovePosition.localPosition - visiblePosition.localPosition;
        return visiblePosition.localPosition - visibleToAbove;
    }

    private void PlayMenuSound(FMODUnity.EventReference sound)
    {
        if (FrisbeeFmodAudioManager.Instance == null) return;

        FrisbeeFmodAudioManager.Instance.PlayOneShot(sound, panelRoot.position);
    }
}
