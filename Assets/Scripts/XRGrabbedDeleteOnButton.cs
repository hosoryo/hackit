using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Attach this to the same GameObject as your controller interactor (or on a parent).
/// Inspector: set deleteAction to the right-controller A button action (Button type).
/// When holding an interactable, pressing the button will release and then destroy or disable it.
/// </summary>
public class XRGrabbedDeleteOnButton : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Button action to trigger deletion. Use an InputAction that maps the controller button (Button South / A).")]
    public InputActionReference deleteAction;

    [Header("Behavior")]
    [Tooltip("If true, object will be destroyed. If false, object will be SetActive(false).")]
    public bool destroyObject = false;

    [Tooltip("Require the interactor to be holding object for at least this many seconds before allow delete (s). 0 = immediate.")]
    public float holdTimeRequired = 0f;

    XRBaseInteractor m_Interactor;
    IXRSelectInteractable m_HeldInteractable;
    float m_HoldStartTime;

    void Awake()
    {
        // Try to find a concrete interactor on this object or a parent (XRDirectInteractor / XRRayInteractor etc.)
        m_Interactor = GetComponent<XRBaseInteractor>() ?? GetComponentInParent<XRBaseInteractor>();
        if (m_Interactor == null)
        {
            Debug.LogError($"[{nameof(XRGrabbedDeleteOnButton)}] No XRBaseInteractor-derived component found on this GameObject or parents. Attach XRDirectInteractor or XRRayInteractor.", this);
        }
    }

    void OnEnable()
    {
        if (m_Interactor != null)
        {
            m_Interactor.selectEntered.AddListener(OnSelectEntered);
            m_Interactor.selectExited.AddListener(OnSelectExited);
        }

        if (deleteAction != null && deleteAction.action != null)
        {
            deleteAction.action.performed += OnDeletePerformed;
            // Ensure the action is enabled so performed can fire
            if (!deleteAction.action.enabled)
                deleteAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (m_Interactor != null)
        {
            m_Interactor.selectEntered.RemoveListener(OnSelectEntered);
            m_Interactor.selectExited.RemoveListener(OnSelectExited);
        }

        if (deleteAction != null && deleteAction.action != null)
        {
            deleteAction.action.performed -= OnDeletePerformed;
            // Do not disable here automatically; leave enabling policy to user if needed
        }
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        m_HeldInteractable = args.interactableObject;
        m_HoldStartTime = Time.time;
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        if (m_HeldInteractable == args.interactableObject)
        {
            m_HeldInteractable = null;
            m_HoldStartTime = 0f;
        }
    }

    void OnDeletePerformed(InputAction.CallbackContext ctx)
    {
        if (m_Interactor == null || m_HeldInteractable == null)
            return;

        // optional hold-time check
        if (holdTimeRequired > 0f && Time.time - m_HoldStartTime < holdTimeRequired)
            return;

        // Resolve GameObject from the interactable
        GameObject interactableGO = null;
        if (m_HeldInteractable is XRBaseInteractable baseInteractable)
            interactableGO = baseInteractable.gameObject;
        else if (m_HeldInteractable is MonoBehaviour mb)
            interactableGO = mb.gameObject;

        if (interactableGO == null)
            return;

        // Gracefully force deselect via InteractionManager if available to avoid dangling references
        var manager = m_Interactor.interactionManager;
        if (manager != null)
        {
            // Use SelectExit to instruct the interaction system to end the selection cleanly.
            manager.SelectExit(m_Interactor, m_HeldInteractable);
        }
        else
        {
            // Fallback: try to remove selection directly (best-effort)
            try
            {
                // XRBaseInteractor may expose EndManualInteraction in some versions; call if present.
                var method = typeof(XRBaseInteractor).GetMethod("EndManualInteraction", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                method?.Invoke(m_Interactor, null);
            }
            catch
            {
                // swallow exceptions - best effort fallback
            }
        }

        // Finally remove or deactivate the GameObject
        if (destroyObject)
            Destroy(interactableGO);
        else
            interactableGO.SetActive(false);

        // clear held reference
        m_HeldInteractable = null;
        m_HoldStartTime = 0f;
    }
}