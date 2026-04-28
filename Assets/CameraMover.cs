using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMover : MonoBehaviour
{
    public bool moveNow = false;
    public CameraPosController controller;

    void Update()
    {
        // 1. Priority check: If moveNow is toggled, move immediately and reset
        if (moveNow)
        {
            moveNow = false;
            TriggerMovement();
            return; // Exit update early so we don't also check for clicks this frame
        }

        // 2. Mouse click check: Only happens if moveNow was false
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if we hit this object or any of its children
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    TriggerMovement();
                }
            }
        }
    }

    private void TriggerMovement()
    {
        // Safety check: ensure the controller exists and isn't already moving
        if (controller != null && !controller.isLerping)
        {
            controller.currentIndex = (controller.currentIndex + 1) % controller.positions.Count;
            StartCoroutine(controller.LerpToPosition(controller.positions[controller.currentIndex]));
        }
    }
}