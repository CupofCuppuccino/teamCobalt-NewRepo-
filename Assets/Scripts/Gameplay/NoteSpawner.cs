// Assets/Scripts/Gameplay/NoteSpawner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("谱面文件")]
    public TextAsset beatmapFile;
    
    [Header("音符预制体")]
    public GameObject yellowNotePrefab;
    public GameObject greenNotePrefab;
    public GameObject bluePurpleNotePrefab;
    
    [Header("生成区域设置")]
    public float spawnDistanceZ = 30f;     // 生成距离（远处）
    public float spawnRadiusX = 4f;        // X轴散布范围
    public float spawnRadiusY = 2.5f;      // Y轴散布范围
    
    [Header("判定线设置")]
    public Transform judgeLine;            // 判定线的 Transform
    public float judgeLineZ = 0.5f;        // 判定线 Z 位置（玩家前方50cm）
    
    [Header("移动设置")]
    public float moveSpeed = 1f;           // 移动速度（米/秒）
    
    private List<NoteData> notes = new List<NoteData>();
    private int nextNoteIndex = 0;
    private AudioSource musicSource;
    private bool isPlaying = false;
    private float musicStartTime;
    
    public System.Action<NoteData> OnNoteCaptured;
    public System.Action<NoteData> OnNoteMissed;
    
    void Start()
    {
        LoadBeatmap();
        musicSource = GetComponent<AudioSource>();
        
        if (musicSource != null)
        {
            musicSource.Play();
            isPlaying = true;
            musicStartTime = Time.time;
            Debug.Log("音乐开始播放");
        }
        else
        {
            Debug.LogWarning("没有 AudioSource，使用时间模拟");
            StartCoroutine(SimulateMusicTime());
        }
    }
    
    void LoadBeatmap()
    {
        if (beatmapFile == null)
        {
            Debug.LogWarning("没有谱面文件，使用默认测试数据");
            CreateTestBeatmap();
            return;
        }
        
        BeatmapData beatmap = JsonUtility.FromJson<BeatmapData>(beatmapFile.text);
        notes = beatmap.notes;
        Debug.Log($"加载谱面完成，共 {notes.Count} 个音符");
    }
    
    void CreateTestBeatmap()
    {
        string[] colors = { "Yellow", "Green", "BluePurple" };
        for (int i = 0; i < 20; i++)
        {
            NoteData note = new NoteData();
            note.color = colors[Random.Range(0, colors.Length)];
            note.time = 4f + i * 1.2f + Random.Range(-0.2f, 0.2f);
            note.x = Random.Range(-2.5f, 2.5f);
            note.y = Random.Range(-1.5f, 1.5f);
            notes.Add(note);
        }
        notes.Sort((a, b) => a.time.CompareTo(b.time));
        Debug.Log($"创建测试谱面，共 {notes.Count} 个音符");
    }
    
    IEnumerator SimulateMusicTime()
    {
        float time = 0;
        while (time < 60f)
        {
            time += Time.deltaTime;
            yield return null;
        }
    }
    
    void Update()
    {
        if (!isPlaying) return;
        
        float currentTime = musicSource != null ? musicSource.time : Time.time - musicStartTime;
        
        // 生成音符
        while (nextNoteIndex < notes.Count)
        {
            NoteData note = notes[nextNoteIndex];
            float timeToArrival = note.time - currentTime;
            
            // 提前 3 秒生成（让音符从远处飘来）
            if (timeToArrival <= 3f && timeToArrival > -0.5f)
            {
                SpawnNote(note, currentTime);
                nextNoteIndex++;
            }
            else if (timeToArrival <= -0.5f)
            {
                // 音符已经过了时间还没生成（跳过）
                Debug.LogWarning($"音符 {nextNoteIndex} 已超时，跳过");
                nextNoteIndex++;
            }
            else
            {
                break;
            }
        }
    }
    
    void SpawnNote(NoteData data, float currentTime)
    {
        GameObject prefab = GetPrefabByColor(data.color);
        if (prefab == null) return;
        
        // 计算目标位置（判定线位置）
        Vector3 targetPosition = GetJudgeLinePosition(data.x, data.y);
        
        // 计算生成位置：在目标位置正前方 spawnDistanceZ 处
        Vector3 spawnPosition = targetPosition + Vector3.forward * spawnDistanceZ;
        
        // 添加随机偏移（让音符在扇形区域散开）
        float angleOffset = Random.Range(-20f, 20f);
        float radiusOffset = Random.Range(0f, 1.5f);
        spawnPosition.x += Mathf.Sin(angleOffset * Mathf.Deg2Rad) * radiusOffset;
        spawnPosition.y += Random.Range(-0.8f, 0.8f);
        
        // 实例化
        GameObject noteObj = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // 获取或添加 Note 组件
        Note note = noteObj.GetComponent<Note>();
        if (note == null) note = noteObj.AddComponent<Note>();
        
        // 计算移动时间：距离 ÷ 速度
        float travelDuration = spawnDistanceZ / moveSpeed;
        
        // 初始化，传入 travelDuration
        note.Initialize(data, targetPosition, travelDuration);
        
        // 设置颜色（如果预制体没有颜色）
        SetNoteColor(noteObj, data.color);
        
        // 添加碰撞体（如果预制体没有）
        if (noteObj.GetComponent<Collider>() == null)
        {
            SphereCollider collider = noteObj.AddComponent<SphereCollider>();
            collider.radius = 0.3f;
            collider.isTrigger = true;
        }
        
        // 设置标签（用于检测）
        noteObj.tag = "Note";
        
        // 订阅事件
        note.OnCaptured += HandleNoteCaptured;
        note.OnMissed += HandleNoteMissed;
        
        Debug.Log($"生成音符: {data.color} at {data.time}s, 位置: {spawnPosition}");
    }
    
    GameObject GetPrefabByColor(string color)
    {
        switch (color)
        {
            case "Yellow": return yellowNotePrefab;
            case "Green": return greenNotePrefab;
            case "BluePurple": return bluePurpleNotePrefab;
            default: return yellowNotePrefab;
        }
    }
    
    void SetNoteColor(GameObject obj, string color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;
        
        Color targetColor = Color.white;
        switch (color)
        {
            case "Yellow": targetColor = Color.yellow; break;
            case "Green": targetColor = Color.green; break;
            case "BluePurple": targetColor = new Color(0.5f, 0.2f, 1f); break;
        }
        renderer.material.color = targetColor;
    }
    
    Vector3 GetJudgeLinePosition(float x, float y)
    {
        if (judgeLine != null)
            return judgeLine.position + new Vector3(x, y, 0);
        else
            return new Vector3(x, y, judgeLineZ);
    }
    
    void HandleNoteCaptured(NoteData data)
    {
        OnNoteCaptured?.Invoke(data);
        Debug.Log($"命中: {data.color} at {data.time}s");
    }
    
    void HandleNoteMissed(NoteData data)
    {
        OnNoteMissed?.Invoke(data);
        Debug.Log($"未命中: {data.color} at {data.time}s");
    }
    
    public int GetTotalNotes()
    {
        return notes.Count;
    }
}