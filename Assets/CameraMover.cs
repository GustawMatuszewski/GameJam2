using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMover : MonoBehaviour
{
    public bool moveNow = false;
    public CameraPosController controller;
    public Limb limb;

    void Update()
    {
        if (moveNow)
        {
            moveNow = false;
            TriggerMovement();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    TriggerMovement();
            }
        }
    }

    private void TriggerMovement()
    {
        if (controller == null || controller.isLerping) return;

        int nextIndex = (controller.currentIndex + 1) % controller.positions.Count;

        if (limb != null && !limb.itemWasDropped)
        {
            bool isLastIndex = nextIndex == controller.positions.Count - 1;
            if (isLastIndex)
                nextIndex = (nextIndex + 1) % controller.positions.Count;
        }

        controller.currentIndex = nextIndex;
        StartCoroutine(controller.LerpToPosition(controller.positions[controller.currentIndex]));
    }
}