using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.IO;  // 用于 Path.Combine

public class NoteSpawner : MonoBehaviour
{
    [Header("谱面文件")]
    public TextAsset beatmapFile;

    [Header("预制体")]
    public GameObject yellowNotePrefab;
    public GameObject greenNotePrefab;
    public GameObject bluePurpleNotePrefab;

    [Header("生成区域")]
    public float spawnDistanceZ = 25f;

    [Header("世界中心")]
    public Transform noteWorldCenter;

    [Header("移动")]
    public float moveSpeed = 5f;
    public float leadTime = 3f;

    private List<NoteData> notes = new List<NoteData>();
    private int nextNoteIndex = 0;

    private AudioSource musicSource;

    private bool hasStarted = false;
    private bool hasEnded = false;

    private float gameTime = 0f;

    public System.Action<NoteData> OnNoteCaptured;
    public System.Action<NoteData> OnNoteMissed;

    void Start()
    {
        // ★ 协程加载谱面
        StartCoroutine(LoadBeatmapCoroutine());

        musicSource = GetComponent<AudioSource>();
        hasStarted = true;

        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play();
            Debug.Log($"🎵 Music Start. Notes={notes.Count}");
            StartCoroutine(WaitForMusicEnd());
        }
    }

    // ★★★ 协程加载谱面 ★★★
    IEnumerator LoadBeatmapCoroutine()
    {
        bool loaded = false;

        // 1. 优先使用拖入的 TextAsset
        if (beatmapFile != null)
        {
            BeatmapData data = JsonUtility.FromJson<BeatmapData>(beatmapFile.text);
            if (data != null && data.notes != null && data.notes.Count > 0)
            {
                notes = data.notes;
                loaded = true;
                Debug.Log($"✅ 从 TextAsset 加载谱面成功！共 {notes.Count} 个音符");
                yield break;
            }
        }

        // 2. ★★★ 从 StreamingAssets 加载 ★★★
        string filePath = Path.Combine(Application.streamingAssetsPath, "beatmap.json");
        Debug.Log($"📁 尝试从 StreamingAssets 加载: {filePath}");

        using (UnityWebRequest request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"📄 JSON 前200字符: {json.Substring(0, Mathf.Min(200, json.Length))}...");

                BeatmapData data = JsonUtility.FromJson<BeatmapData>(json);
                if (data != null && data.notes != null && data.notes.Count > 0)
                {
                    notes = data.notes;
                    loaded = true;
                    Debug.Log($"✅ 从 StreamingAssets 加载谱面成功！共 {notes.Count} 个音符");
                }
                else
                {
                    Debug.LogWarning("⚠️ JSON 解析失败或音符为空");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to load beatmap: {request.error}");
            }
        }

        // 3. 从 Resources 加载
        if (!loaded)
        {
            TextAsset file = Resources.Load<TextAsset>("beatmap");
            if (file != null)
            {
                BeatmapData data = JsonUtility.FromJson<BeatmapData>(file.text);
                if (data != null && data.notes != null && data.notes.Count > 0)
                {
                    notes = data.notes;
                    loaded = true;
                    Debug.Log($"✅ 从 Resources 加载谱面成功！共 {notes.Count} 个音符");
                }
            }
        }

        // 4. 生成测试数据
        if (!loaded)
        {
            Debug.LogWarning("⚠️ No valid beatmap found. Generating test beatmap.");
            CreateTestBeatmap();
        }

        notes.Sort((a, b) => a.beatTime.CompareTo(b.beatTime));

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetTotalCounts(CountNotesByColor());
        }

        Debug.Log($"📊 最终谱面: {notes.Count} 个音符");
    }

    void Update()
    {
        if (!hasStarted) return;

        float currentTime = 0f;

        if (musicSource != null && musicSource.isPlaying)
        {
            currentTime = musicSource.time;
            gameTime = currentTime;
        }
        else
        {
            gameTime += Time.deltaTime;
            currentTime = gameTime;
        }

        if (Time.frameCount % 300 == 0)
        {
            Debug.Log($"🎵 Current time: {currentTime:F2}s, spawned: {nextNoteIndex}/{notes.Count}");
        }

        while (nextNoteIndex < notes.Count)
        {
            NoteData note = notes[nextNoteIndex];
            float timeToArrival = note.beatTime - currentTime;

            if (timeToArrival <= leadTime)
            {
                SpawnNote(note);
                nextNoteIndex++;
            }
            else
            {
                break;
            }
        }
    }

    Dictionary<string, int> CountNotesByColor()
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        result["Yellow"] = 0;
        result["Green"] = 0;
        result["BluePurple"] = 0;

        foreach (var n in notes)
        {
            if (result.ContainsKey(n.color))
                result[n.color]++;
        }
        return result;
    }

    void CreateTestBeatmap()
    {
        string[] colors = { "Yellow", "Green", "BluePurple" };

        for (int i = 0; i < 60; i++)
        {
            NoteData n = new NoteData();
            n.color = colors[Random.Range(0, 3)];
            n.beatTime = 2 + i * 0.8f;
            n.xOffset = Random.Range(-2.5f, 2.5f);
            n.yOffset = Random.Range(-1.5f, 1.5f);
            notes.Add(n);
        }

        Debug.Log("Generated test beatmap");
    }

    void SpawnNote(NoteData data)
    {
        GameObject prefab = GetPrefabByColor(data.color);
        if (prefab == null) return;

        Vector3 target = GetJudgeLinePosition(data.xOffset, data.yOffset);
        Vector3 spawn = target + Vector3.forward * spawnDistanceZ;

        GameObject obj = Instantiate(prefab, spawn, Quaternion.identity);

        Note note = obj.GetComponent<Note>();
        if (note == null) note = obj.AddComponent<Note>();

        float duration = spawnDistanceZ / moveSpeed;

        note.Initialize(data, target, duration);

        note.OnCaptured += HandleNoteCaptured;
        note.OnMissed += HandleNoteMissed;

        SetColor(obj, data.color);

        if (obj.GetComponent<Collider>() == null)
        {
            SphereCollider c = obj.AddComponent<SphereCollider>();
            c.isTrigger = true;
        }
    }

    GameObject GetPrefabByColor(string c)
    {
        switch (c)
        {
            case "Yellow": return yellowNotePrefab;
            case "Green": return greenNotePrefab;
            case "BluePurple": return bluePurpleNotePrefab;
        }
        return yellowNotePrefab;
    }

    void SetColor(GameObject obj, string c)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r == null) return;

        Color targetColor = Color.white;
        switch (c)
        {
            case "Yellow": targetColor = Color.yellow; break;
            case "Green": targetColor = Color.green; break;
            case "BluePurple": targetColor = new Color(0.5f, 0.2f, 1f); break;
        }

        Material mat = r.material;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", targetColor);
        else if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", targetColor);
        else
            mat.color = targetColor;
    }

    Vector3 GetJudgeLinePosition(float x, float y)
    {
        if (noteWorldCenter != null)
            return noteWorldCenter.position + new Vector3(x, y, 0);
        return new Vector3(x, y, 0);
    }

    IEnumerator WaitForMusicEnd()
    {
        if (musicSource == null || musicSource.clip == null)
            yield break;

        yield return new WaitForSeconds(musicSource.clip.length);

        if (!hasEnded)
        {
            hasEnded = true;
            SceneManager.LoadScene("EndingScene");
        }
    }

    void HandleNoteCaptured(NoteData n)
    {
        OnNoteCaptured?.Invoke(n);
    }

    void HandleNoteMissed(NoteData n)
    {
        OnNoteMissed?.Invoke(n);
    }

    public int GetTotalNotes()
    {
        return notes.Count;
    }
}