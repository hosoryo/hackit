using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class SizeManipulator : MonoBehaviour
{
    [Header("Interactors")]
    public XRDirectInteractor leftHandInteractor;
    public XRDirectInteractor rightHandInteractor;

    [Header("Button Actions")]
    public InputActionReference leftExpandAction;
    public InputActionReference leftShrinkAction;
    public InputActionReference rightExpandAction;
    public InputActionReference rightShrinkAction;

    [Header("Scale Settings")]
    public float buttonStep = 0.1f;
    public float minScale = 0.1f;
    public float maxScale = 3f;

    // �s���`����p
    private bool isPinching = false;
    private float initialDistance;
    private Vector3 initialScale;

    private void OnEnable()
    {
        leftExpandAction.action.performed += _ => ScaleByButton(buttonStep);
        leftShrinkAction.action.performed += _ => ScaleByButton(-buttonStep);
        rightExpandAction.action.performed += _ => ScaleByButton(buttonStep);
        rightShrinkAction.action.performed += _ => ScaleByButton(-buttonStep);
    }

    private void OnDisable()
    {
        leftExpandAction.action.performed -= _ => ScaleByButton(buttonStep);
        leftShrinkAction.action.performed -= _ => ScaleByButton(-buttonStep);
        rightExpandAction.action.performed -= _ => ScaleByButton(buttonStep);
        rightShrinkAction.action.performed -= _ => ScaleByButton(-buttonStep);
    }

    private void Update()
    {
        HandlePinchScaling();
    }

    // �{�^���ɂ��X�P�[���ύX
    private void ScaleByButton(float delta)
    {
        if (GetActiveInteractor() == null) return;

        Vector3 newScale = transform.localScale + Vector3.one * delta;
        transform.localScale = ClampScale(newScale);
    }

    // ����s���`�ŃX�P�[���ύX
    private void HandlePinchScaling()
    {
        bool leftGrabbing = IsInteractorGrabbing(leftHandInteractor);
        bool rightGrabbing = IsInteractorGrabbing(rightHandInteractor);

        if (leftGrabbing && rightGrabbing)
        {
            float currentDistance = Vector3.Distance(
                leftHandInteractor.transform.position,
                rightHandInteractor.transform.position
            );

            if (!isPinching)
            {
                isPinching = true;
                initialDistance = currentDistance;
                initialScale = transform.localScale;
            }
            else if (initialDistance > Mathf.Epsilon)
            {
                float factor = currentDistance / initialDistance;
                Vector3 targetScale = initialScale * factor;
                transform.localScale = ClampScale(targetScale);
            }
        }
        else
        {
            isPinching = false;
        }
    }

    // �C���^���N�^�[�����̃I�u�W�F�N�g��͂�ł��邩����
    private bool IsInteractorGrabbing(XRBaseInteractor interactor)
    {
        foreach (var interactable in interactor.interactablesSelected)
        {
            if (interactable.transform == transform)
                return true;
        }
        return false;
    }

    // �͂�ł���C���^���N�^�[���擾�i���݂��Ȃ���� null�j
    private XRBaseInteractor GetActiveInteractor()
    {
        if (IsInteractorGrabbing(leftHandInteractor)) return leftHandInteractor;
        if (IsInteractorGrabbing(rightHandInteractor)) return rightHandInteractor;
        return null;
    }

    // �X�P�[���� minScale�`maxScale �͈̔͂ɐ���
    private Vector3 ClampScale(Vector3 scale)
    {
        scale.x = Mathf.Clamp(scale.x, minScale, maxScale);
        scale.y = Mathf.Clamp(scale.y, minScale, maxScale);
        scale.z = Mathf.Clamp(scale.z, minScale, maxScale);
        return scale;
    }
}