using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CameraMover : MonoBehaviour
{
    public bool moveNow = false;
    public CameraPosController controller;
    public List<Limb> limbs;
    public PlayerController player;

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

    bool AnyLimbDropped()
    {
        foreach (Limb limb in limbs)
            if (limb != null && limb.itemWasDropped) return true;
        return false;
    }

    void CollectDroppedItems()
    {
        foreach (Limb limb in limbs)
        {
            if (limb == null || !limb.itemWasDropped) continue;
            if (limb.droppedItemData != null)
                player.AddToInventory(limb.droppedItemData);
            if (limb.droppedItemGO != null)
                Destroy(limb.droppedItemGO);
            limb.itemWasDropped = false;
            limb.droppedItemGO = null;
            limb.droppedItemData = null;
        }
    }

    private void TriggerMovement()
    {
        if (controller == null || controller.isLerping) return;

        bool wasAtLastIndex = controller.currentIndex == controller.positions.Count - 1;
        if (wasAtLastIndex && AnyLimbDropped())
            CollectDroppedItems();

        int nextIndex;

        if (controller.currentIndex == 2 && !AnyLimbDropped())
        {
            nextIndex = 0;
        }
        else
        {
            nextIndex = (controller.currentIndex + 1) % controller.positions.Count;
            if (!AnyLimbDropped())
            {
                bool isLastIndex = nextIndex == controller.positions.Count - 1;
                if (isLastIndex)
                    nextIndex = (nextIndex + 1) % controller.positions.Count;
            }
        }

        controller.currentIndex = nextIndex;
        StartCoroutine(controller.LerpToPosition(controller.positions[controller.currentIndex]));
    }
}