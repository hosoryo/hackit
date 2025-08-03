using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SizeManipulator : MonoBehaviour
{
    public XRDirectInteractor leftHandInteractor;
    public InputActionReference xButtonAction; // "XRI LeftHand Interaction/X Button"
    public InputActionReference yButtonAction; // "XRI LeftHand Interaction/Y Button"
    public float scaleStep = 0.1f;
    public float minScale = 0.1f;
    public float maxScale = 3f;

    void OnEnable()
    {
        xButtonAction.action.performed += HandleXButton;
        yButtonAction.action.performed += HandleYButton;
    }

    void OnDisable()
    {
        xButtonAction.action.performed -= HandleXButton;
        yButtonAction.action.performed -= HandleYButton;
    }

    void HandleXButton(InputAction.CallbackContext ctx)
    {
        Transform target = GetHeldObject();
        if (target == null) return;

        Vector3 newScale = target.localScale + Vector3.one * scaleStep;
        target.localScale = ClampScale(newScale);
    }

    void HandleYButton(InputAction.CallbackContext ctx)
    {
        Transform target = GetHeldObject();
        if (target == null) return;

        Vector3 newScale = target.localScale - Vector3.one * scaleStep;
        target.localScale = ClampScale(newScale);
    }

    Transform GetHeldObject()
    {
        if (leftHandInteractor.selectTarget != null)
            return leftHandInteractor.selectTarget.transform;

        return null;
    }

    Vector3 ClampScale(Vector3 scale)
    {
        float clampedX = Mathf.Clamp(scale.x, minScale, maxScale);
        float clampedY = Mathf.Clamp(scale.y, minScale, maxScale);
        float clampedZ = Mathf.Clamp(scale.z, minScale, maxScale);
        return new Vector3(clampedX, clampedY, clampedZ);
    }
}