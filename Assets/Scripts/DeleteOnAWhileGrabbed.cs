using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class DeleteOnAWhileGrabbed : MonoBehaviour
{
    public InputActionReference aButtonAction;
    public bool disableInsteadOfDestroy = false;
    public float safetyDelay = 0.1f;

    XRGrabInteractable grabInteractable;
    bool isSelected = false;
    float selectTime = 0f;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
        UnsubscribeAction();
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        isSelected = true;
        selectTime = Time.time;
        SubscribeAction();
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        isSelected = false;
        UnsubscribeAction();
    }

    void SubscribeAction()
    {
        if (aButtonAction == null || aButtonAction.action == null) return;
        aButtonAction.action.performed += OnAButtonPerformed;
        if (!aButtonAction.action.enabled) aButtonAction.action.Enable();
    }

    void UnsubscribeAction()
    {
        if (aButtonAction == null || aButtonAction.action == null) return;
        aButtonAction.action.performed -= OnAButtonPerformed;
    }

    void OnAButtonPerformed(InputAction.CallbackContext ctx)
    {
        if (!isSelected) return;
        if (Time.time - selectTime < safetyDelay) return;
        DeleteTarget();
    }

    void DeleteTarget()
    {
        if (disableInsteadOfDestroy)
        {
            gameObject.SetActive(false);
        }
        else
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
            Destroy(gameObject);
        }
    }
}