using UnityEngine;
using System.Collections;
using UnityEngine.Events;
public class EatInteractionVisual : MonoBehaviour
{
    private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    [SerializeField] private float dissolveDuration = 1.2f;
    [SerializeField] private Renderer meshRenderer;
    private Material currMaterial;
    private Coroutine dissolveCoroutine;
    public UnityEvent OnDissolvedFinished;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(meshRenderer == null) meshRenderer = GetComponent<Renderer>();
        if(meshRenderer == null) meshRenderer = GetComponentInChildren<Renderer>();

        if (meshRenderer != null) currMaterial = meshRenderer.material;
    }
    void Start()
    {
    }
    public void PlayDissolve()
    {
        if (dissolveCoroutine == null) dissolveCoroutine = StartCoroutine(DissolveCoroutine());
    }
    public void ResetMaterial()
    {
        if (dissolveCoroutine != null)
        {
            StopCoroutine(dissolveCoroutine);
            dissolveCoroutine = null;
        }
        if (currMaterial != null) currMaterial.SetFloat(DissolveAmountID, 0f);
    }
    private IEnumerator DissolveCoroutine()
    {
        float elapsed = 0f;
        while(elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float dissolveValue = Mathf.Clamp01(elapsed / dissolveDuration);
            if(currMaterial != null)
            {
                currMaterial.SetFloat(DissolveAmountID, dissolveValue);
            }
            yield return null;
        }
        if (currMaterial != null) currMaterial.SetFloat(DissolveAmountID, 1f);
        OnDissolvedFinished?.Invoke();
        dissolveCoroutine = null;
    }
}
