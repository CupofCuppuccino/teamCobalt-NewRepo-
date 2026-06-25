using UnityEngine;

public enum NoteColor
{
    Yellow,
    Green,
    BluePurple
}

public class Note : MonoBehaviour
{
    [Header("颜色")]
    public NoteColor noteColor;

    [Header("分数")]
    public int scoreValue = 100;

    [Header("大小")]
    public float minScale = 0.15f;
    public float maxScale = 1.0f;

    [Header("判定范围发光")]
    public float normalGlow = 0.2f;
    public float readyGlow = 2f;

    [Header("特效")]
    public GameObject hitEffectPrefab;
    public GameObject missEffectPrefab;


    private NoteData data;

    private Vector3 targetPosition;
    private Vector3 passPosition;

    private float travelDuration;
    private float spawnTime;

    private Vector3 startPosition;


    private bool isActive = true;
    private bool isCaptured = false;


    private Renderer[] renderers;
    private Material[] materials;

    private Color[] originalBaseColors;

    private bool glowing = false;


    public System.Action<NoteData> OnCaptured;
    public System.Action<NoteData> OnMissed;



    public void Initialize(NoteData noteData, Vector3 target, float duration)
    {
        data = noteData;

        targetPosition = target;

        passPosition =
            targetPosition -
            Vector3.forward * 5f;


        travelDuration = duration;

        spawnTime = Time.time;

        startPosition = transform.position;


        renderers =
            GetComponentsInChildren<Renderer>();


        materials =
            new Material[renderers.Length];


        originalBaseColors =
            new Color[renderers.Length];


        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] =
                renderers[i].material;


            Material mat = materials[i];


            // ★ Shader Graph 用 BaseColor
            if (mat.HasProperty("_BaseColor"))
            {
                originalBaseColors[i] =
                    mat.GetColor("_BaseColor");
            }
            else if (mat.HasProperty("_Color"))
            {
                originalBaseColors[i] =
                    mat.GetColor("_Color");
            }
            else
            {
                originalBaseColors[i] = Color.white;
            }
        }


        transform.localScale =
            Vector3.one * minScale;


        SetGlow(normalGlow);


        HitManager.Register(this);
    }



    void Update()
    {
        if (!isActive || isCaptured)
            return;


        float progress =
            (Time.time - spawnTime) / travelDuration;


        progress = Mathf.Clamp01(progress);


        transform.position =
            Vector3.Lerp(
                startPosition,
                passPosition,
                progress
            );


        float scale =
            Mathf.Lerp(
                minScale,
                maxScale,
                progress
            );


        transform.localScale =
            Vector3.one * scale;


        transform.Rotate(
            Vector3.up,
            30f * Time.deltaTime
        );


        transform.Rotate(
            Vector3.right,
            20f * Time.deltaTime
        );


        UpdateReadyGlow();


        if (progress >= 1f)
        {
            MissAndDestroy();
        }
    }



    void UpdateReadyGlow()
    {
        if (IsInHitRange() && !glowing)
        {
            glowing = true;
            SetGlow(readyGlow);
        }
        else if (!IsInHitRange() && glowing)
        {
            glowing = false;
            SetGlow(normalGlow);
        }
    }



    public void Capture()
    {
        if (!isActive)
            return;


        isActive = false;
        isCaptured = true;


        if (data != null)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(
                    data.color,
                    scoreValue
                );
            }
        }


        OnCaptured?.Invoke(data);


        PlayEffect(hitEffectPrefab);


        Destroy(gameObject);
    }



    public bool IsActive()
    {
        return isActive && !isCaptured;
    }



    public float GetDistanceToJudgeLine()
    {
        return Mathf.Abs(
            transform.position.z - 15f
        );
    }



    public bool IsInHitRange()
    {
        return GetDistanceToJudgeLine()
            <= HitManager.hitRange;
    }



    void MissAndDestroy()
    {
        if (!isActive)
            return;


        isActive = false;


        OnMissed?.Invoke(data);


        PlayEffect(missEffectPrefab);


        Destroy(gameObject);
    }



    void PlayEffect(GameObject prefab)
    {
        if (prefab == null)
            return;


        GameObject obj =
            Instantiate(
                prefab,
                transform.position,
                Quaternion.identity
            );


        ParticleSystem[] systems =
            obj.GetComponentsInChildren<ParticleSystem>();


        foreach (ParticleSystem ps in systems)
        {
            var main = ps.main;
            main.simulationSpeed = 0.25f;
        }


        Destroy(obj, 4f);
    }

    void SetGlow(float intensity)
    {
        if (materials == null)
            return;


        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];


            // 保留原颜色
            Color baseColor =
                originalBaseColors[i];


            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor(
                    "_BaseColor",
                    baseColor
                );
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor(
                    "_Color",
                    baseColor
                );
            }


            // Shader Graph:
            // SG_BaseColor_Lite 使用 EmissionIntensity
            if (mat.HasProperty("_EmissionIntensity"))
            {
                mat.SetFloat(
                    "_EmissionIntensity",
                    intensity
                );
            }


            // 兼容旧Shader
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");

                mat.SetColor(
                    "_EmissionColor",
                    Color.white * intensity
                );
            }
        }
    }

    void OnDestroy()
    {
        HitManager.Unregister(this);
    }
}