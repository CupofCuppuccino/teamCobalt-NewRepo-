using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        LoadBeatmap();

        musicSource = GetComponent<AudioSource>();

        hasStarted = true;


        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play();

            Debug.Log(
            "Music Start. Notes=" + notes.Count);


            StartCoroutine(WaitForMusicEnd());
        }
    }



    void Update()
    {
        if (!hasStarted) return;

        // ★ 优先用 musicSource.time，失败则用 gameTime
        float currentTime = 0f;

        if (musicSource != null && musicSource.isPlaying)
        {
            currentTime = musicSource.time;
            // ★ 同步 gameTime
            gameTime = currentTime;
        }
        else
        {
            // ★ 如果音乐没在播放，用累加的时间
            gameTime += Time.deltaTime;
            currentTime = gameTime;
        }

        // ★ 调试日志：每 5 秒打印一次
        if (Time.frameCount % 300 == 0)
        {
            Debug.Log($"🎵 当前时间: {currentTime:F2}s, 已生成: {nextNoteIndex}/{notes.Count}");
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




    void LoadBeatmap()
    {
        bool loaded = false;


        if (beatmapFile != null)
        {
            BeatmapData data =
            JsonUtility.FromJson<BeatmapData>(
                beatmapFile.text
            );


            if (data != null &&
               data.notes != null)
            {
                notes = data.notes;
                loaded = true;

                Debug.Log(
                $"Beatmap loaded:{notes.Count}"
                );
            }
        }



        if (!loaded)
        {
            TextAsset file =
            Resources.Load<TextAsset>("beatmap");


            if (file != null)
            {
                BeatmapData data =
                JsonUtility.FromJson<BeatmapData>(
                    file.text
                );


                if (data != null)
                {
                    notes = data.notes;
                    loaded = true;
                }
            }
        }



        if (!loaded)
        {
            CreateTestBeatmap();
        }


        notes.Sort(
            (a, b) =>
            a.beatTime.CompareTo(b.beatTime)
        );


        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance
            .SetTotalCounts(
                CountNotesByColor()
            );
        }
    }




    Dictionary<string, int> CountNotesByColor()
    {
        Dictionary<string, int> result =
        new Dictionary<string, int>();

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
        string[] colors =
        {
            "Yellow",
            "Green",
            "BluePurple"
        };


        for (int i = 0; i < 60; i++)
        {
            NoteData n = new NoteData();

            n.color =
            colors[Random.Range(0, 3)];

            n.beatTime =
            2 + i * 0.8f;

            n.xOffset =
            Random.Range(-2.5f, 2.5f);

            n.yOffset =
            Random.Range(-1.5f, 1.5f);


            notes.Add(n);
        }


        Debug.Log(
        "Generated test beatmap"
        );
    }





    void SpawnNote(NoteData data)
    {

        GameObject prefab =
        GetPrefabByColor(data.color);


        if (prefab == null)
            return;



        Vector3 target =
        GetJudgeLinePosition(
            data.xOffset,
            data.yOffset
        );


        Vector3 spawn =
        target +
        Vector3.forward *
        spawnDistanceZ;



        GameObject obj =
        Instantiate(
            prefab,
            spawn,
            Quaternion.identity
        );



        Note note =
        obj.GetComponent<Note>();


        if (note == null)
            note = obj.AddComponent<Note>();



        float duration =
        spawnDistanceZ / moveSpeed;



        note.Initialize(
            data,
            target,
            duration
        );


        note.OnCaptured +=
        HandleNoteCaptured;


        note.OnMissed +=
        HandleNoteMissed;



        SetColor(obj, data.color);



        if (obj.GetComponent<Collider>() == null)
        {
            SphereCollider c =
            obj.AddComponent<SphereCollider>();

            c.isTrigger = true;
        }
    }





    GameObject GetPrefabByColor(string c)
    {
        switch (c)
        {
            case "Yellow":
                return yellowNotePrefab;

            case "Green":
                return greenNotePrefab;

            case "BluePurple":
                return bluePurpleNotePrefab;
        }


        return yellowNotePrefab;
    }





    void SetColor(GameObject obj, string c)
    {
        Renderer r =
        obj.GetComponent<Renderer>();

        if (r == null)
            return;


        if (c == "Yellow")
            r.material.color = Color.yellow;

        if (c == "Green")
            r.material.color = Color.green;

        if (c == "BluePurple")
            r.material.color =
            new Color(.5f, .2f, 1);
    }





    Vector3 GetJudgeLinePosition(float x, float y)
    {
        if (noteWorldCenter != null)
            return noteWorldCenter.position +
            new Vector3(x, y, 0);


        return new Vector3(x, y, 0);
    }





    IEnumerator WaitForMusicEnd()
    {
        if (musicSource == null ||
           musicSource.clip == null)
            yield break;


        yield return new WaitForSeconds(
            musicSource.clip.length
        );


        if (!hasEnded)
        {
            hasEnded = true;
            SceneManager.LoadScene(
                "EndingScene"
            );
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