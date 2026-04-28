using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    public float flyDuration = 0.8f;
    public float shakeDuration = 0.5f;
    public float shakeAmount = 0.05f;
    public ParticleSystem burstParticles;

    [Header("Line Renderer")]
    public Material lineMaterial;
    public float lineWidth = 0.02f;
    public Color lineColor = Color.cyan;

    private List<LineRenderer> activeLines = new();
    private GameObject spawnedResult;

    public GameObject GetSpawnedResult() => spawnedResult;

    public IEnumerator PlayCraftSequence(
        List<GameObject> itemObjects,
        Vector3 center,
        GameObject resultPrefab)
    {
        spawnedResult = null;

        List<Coroutine> shakes = new();
        foreach (var item in itemObjects)
        {
            LineRenderer lr = CreateLine(item.transform.position, center);
            activeLines.Add(lr);
            shakes.Add(StartCoroutine(ShakeObject(item.transform, shakeDuration, shakeAmount)));
        }

        float elapsed = 0f;
        Vector3[] origins = new Vector3[itemObjects.Count];
        for (int i = 0; i < itemObjects.Count; i++)
            origins[i] = itemObjects[i].transform.position;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < itemObjects.Count; i++)
                if (activeLines[i] != null)
                {
                    activeLines[i].SetPosition(0, itemObjects[i].transform.position);
                    activeLines[i].SetPosition(1, center);
                }
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / flyDuration, 3f);

            for (int i = 0; i < itemObjects.Count; i++)
            {
                if (itemObjects[i] == null) continue;
                itemObjects[i].transform.position = Vector3.Lerp(origins[i], center, t);
                itemObjects[i].transform.Rotate(Vector3.up, 360f * Time.deltaTime / flyDuration);

                if (activeLines[i] != null)
                {
                    activeLines[i].SetPosition(0, itemObjects[i].transform.position);
                    activeLines[i].SetPosition(1, center);
                }
            }
            yield return null;
        }

        foreach (var item in itemObjects)
            if (item != null) item.SetActive(false);

        foreach (var lr in activeLines)
            if (lr != null) Destroy(lr.gameObject);
        activeLines.Clear();

        if (burstParticles != null)
        {
            var burst = Instantiate(burstParticles, center, Quaternion.identity);
            burst.Play();
            Destroy(burst.gameObject, burst.main.duration + 0.5f);
        }

        yield return new WaitForSeconds(0.2f);

        if (resultPrefab != null)
        {
            spawnedResult = Instantiate(resultPrefab, center, Quaternion.identity);
            yield return StartCoroutine(ShakeObject(spawnedResult.transform, 0.3f, 0.08f));
        }

        foreach (var item in itemObjects)
            if (item != null) Destroy(item);
    }

    LineRenderer CreateLine(Vector3 from, Vector3 to)
    {
        GameObject go = new GameObject("CraftLine");
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.material = lineMaterial != null
            ? lineMaterial
            : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lineColor;
        lr.endColor = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth * 0.3f;
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.useWorldSpace = true;
        return lr;
    }

    IEnumerator ShakeObject(Transform t, float duration, float amount)
    {
        Vector3 origin = t.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            t.localPosition = origin + (Vector3)Random.insideUnitCircle * amount;
            yield return null;
        }
        t.localPosition = origin;
    }
}