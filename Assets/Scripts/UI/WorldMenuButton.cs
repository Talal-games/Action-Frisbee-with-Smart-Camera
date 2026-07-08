using UnityEngine;
using UnityEngine.Events;

public class WorldMenuButton : MonoBehaviour
{
    [SerializeField] private Transform hoveredPosition;
    [SerializeField] private float hoverPosSpeed = 10f;
    [SerializeField] private GameObject hoverEffects;
    [SerializeField] private ParticleSystem clickParticles;
    [SerializeField] private UnityEvent onClick;

    private Collider[] colliders;
    private Vector3 normalLocalPosition;
    private Vector3 hoverWorldPosition;
    private WorldMenuPanel parentPanel;
    private bool isHovered;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
        parentPanel = GetComponentInParent<WorldMenuPanel>();
        normalLocalPosition = transform.localPosition;
        SetHovered(false);
    }

    private void Update()
    {
        bool canInteract = parentPanel == null || !parentPanel.IsMoving;
        SetHovered(canInteract && IsPointerOverButton());

        if (isHovered && Input.GetMouseButtonDown(0))
        {
            Click();
        }

        UpdateHoverMovement();
    }

    public void SetHovered(bool hovered)
    {
        if (isHovered == hovered) return;

        isHovered = hovered;

        if (hoverEffects != null)
        {
            hoverEffects.SetActive(isHovered);
        }

        if (isHovered)
        {
            // Copy the marker's world position once, so child markers do not move the target as the button slides.
            if (hoveredPosition != null)
            {
                hoverWorldPosition = hoveredPosition.position;
            }

            PlayMenuSound(FrisbeeFmodSoundDirectory.Instance != null ? FrisbeeFmodSoundDirectory.Instance.MenuHover : default);
        }
    }

    public void Click()
    {
        if (parentPanel != null && parentPanel.IsMoving) return;

        if (clickParticles != null)
        {
            clickParticles.Play();
        }

        PlayMenuSound(FrisbeeFmodSoundDirectory.Instance != null ? FrisbeeFmodSoundDirectory.Instance.MenuClick : default);
        onClick.Invoke();
    }

    private void UpdateHoverMovement()
    {
        if (isHovered && hoveredPosition != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                hoverWorldPosition,
                hoverPosSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                normalLocalPosition,
                hoverPosSpeed * Time.unscaledDeltaTime);
        }

        Physics.SyncTransforms();
    }

    private bool IsPointerOverButton()
    {
        if (colliders.Length == 0 || Camera.main == null) return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return false;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (hit.collider == colliders[i])
            {
                return true;
            }
        }

        return false;
    }

    private void PlayMenuSound(FMODUnity.EventReference sound)
    {
        if (FrisbeeFmodAudioManager.Instance == null) return;

        FrisbeeFmodAudioManager.Instance.PlayOneShot(sound, transform.position);
    }
}
