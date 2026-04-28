using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class CameraPosController : MonoBehaviour
{
    public Camera pCam;
    public List<Transform> positions;
    public bool changePos = false;
    public float lerpDuration = 1f;

    public int currentIndex = 0;
    public bool isLerping = false;

    void Start()
    {
        if (positions.Count > 0)
        {
            pCam.transform.position = positions[0].position;
            pCam.transform.rotation = positions[0].rotation;
        }
    }

    void FixedUpdate()
    {
        
    }
    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        if (hit.transform == transform || hit.transform.IsChildOf(transform))
        {
            changePos=true;
            if (changePos && !isLerping)
            {
                currentIndex = (currentIndex + 1) % positions.Count;
                StartCoroutine(LerpToPosition(positions[currentIndex]));
                changePos = false;
            }
        }
    }
    public System.Collections.IEnumerator LerpToPosition(Transform toPos){
        isLerping = true;

        Vector3 startPos = pCam.transform.position;
        Quaternion startRot = pCam.transform.rotation;
        float elapsed = 0f;

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / lerpDuration);
            pCam.transform.position = Vector3.Lerp(startPos, toPos.position, t);
            pCam.transform.rotation = Quaternion.Slerp(startRot, toPos.rotation, t);
            yield return null;
        }

        pCam.transform.position = toPos.position;
        pCam.transform.rotation = toPos.rotation;
        isLerping = false;
    }
}