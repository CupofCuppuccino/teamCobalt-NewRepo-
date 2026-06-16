// Assets/Scripts/Gameplay/Note.cs
using UnityEngine;
using System.Collections;

public class Note : MonoBehaviour
{
    [Header("视觉设置")]
    public float minScale = 0.15f;
    public float maxScale = 1.0f;
    public float minGlow = 0.05f;
    public float maxGlow = 0.8f;
    
    [Header("消失特效")]
    public GameObject hitEffectPrefab;
    public GameObject missEffectPrefab;
    
    private NoteData data;
    private Vector3 targetPosition;
    private float travelDuration;
    private float spawnTime;
    private Vector3 startPosition;
    
    private bool isActive = true;
    private bool isCaptured = false;
    private bool hasReachedJudgeLine = false;
    
    private Material material;
    private Color originalColor;
    private Renderer noteRenderer;
    
    public System.Action<NoteData> OnCaptured;
    public System.Action<NoteData> OnMissed;
    
    public void Initialize(NoteData noteData, Vector3 target, float duration)
    {
        data = noteData;
        targetPosition = target;
        travelDuration = duration;
        spawnTime = Time.time;
        startPosition = transform.position;
        
        noteRenderer = GetComponent<Renderer>();
        if (noteRenderer != null)
        {
            material = noteRenderer.material;
            originalColor = material.color;
        }
        
        transform.localScale = Vector3.one * minScale;
        SetGlow(minGlow);
    }
    
    void Update()
    {
        if (!isActive || isCaptured || hasReachedJudgeLine) return;
        
        float progress = (Time.time - spawnTime) / travelDuration;
        progress = Mathf.Clamp01(progress);
        
        // 1. 移动
        transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
        
        // 2. 缩放：逐渐变大
        float scaleCurve = Mathf.Pow(progress, 1.2f);
        float currentScale = Mathf.Lerp(minScale, maxScale, scaleCurve);
        transform.localScale = Vector3.one * currentScale;
        
        // 3. 发光：逐渐变亮
        float glowCurve = Mathf.Pow(progress, 1.5f);
        float currentGlow = Mathf.Lerp(minGlow, maxGlow, glowCurve);
        SetGlow(currentGlow);
        
        // 4. 自转
        transform.Rotate(Vector3.up, 20f * Time.deltaTime);
        transform.Rotate(Vector3.right, 15f * Time.deltaTime);
        
        // 5. 到达判定线
        if (progress >= 1f)
        {
            OnReachJudgeLine();
        }
    }
    
    void OnReachJudgeLine()
    {
        if (hasReachedJudgeLine) return;
        hasReachedJudgeLine = true;
        isActive = false;
        
        if (isCaptured)
        {
            OnCaptured?.Invoke(data);
            PlayEffect(hitEffectPrefab);
        }
        else
        {
            OnMissed?.Invoke(data);
            PlayEffect(missEffectPrefab);
        }
        
        StartCoroutine(FadeOutAndDestroy());
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCaptured || !isActive || hasReachedJudgeLine) return;
        
        if (other.CompareTag("WindBlade"))
        {
            Capture();
        }
    }
    
    public void Capture()
    {
        if (isCaptured || !isActive || hasReachedJudgeLine) return;
        
        isCaptured = true;
        isActive = false;
        StartCoroutine(PlayCaptureFlash());
    }
    
    IEnumerator PlayCaptureFlash()
    {
        if (noteRenderer != null && material != null)
        {
            material.color = Color.white;
            material.SetColor("_EmissionColor", Color.white * 2f);
        }
        yield return new WaitForSeconds(0.06f);
        
        if (noteRenderer != null && material != null)
        {
            material.color = originalColor;
        }
    }
    
    void PlayEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            
            // 强制设置粒子速度
            ParticleSystem[] systems = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in systems)
            {
                var main = ps.main;
                main.simulationSpeed = 0.25f;
            }
            
            Destroy(effect, 4f);
        }
    }
    
    IEnumerator FadeOutAndDestroy()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;
        Color originalColor = material != null ? material.color : Color.white;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // 向内收缩
            float scale = Mathf.Lerp(originalScale.magnitude, 0f, progress);
            transform.localScale = Vector3.one * scale;
            
            if (noteRenderer != null && material != null)
            {
                Color fadeColor = originalColor;
                fadeColor.a = Mathf.Lerp(1f, 0.2f, progress);
                material.color = fadeColor;
                material.SetColor("_EmissionColor", originalColor * Mathf.Lerp(1f, 0f, progress));
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void SetGlow(float intensity)
    {
        if (material == null) return;
        
        material.EnableKeyword("_EMISSION");
        Color glowColor = originalColor * intensity;
        material.SetColor("_EmissionColor", glowColor);
        material.color = originalColor * (0.3f + 0.7f * intensity);
    }
    
    void OnDestroy()
    {
        if (material != null && material != noteRenderer?.sharedMaterial)
            Destroy(material);
    }
}