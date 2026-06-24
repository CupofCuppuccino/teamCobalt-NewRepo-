using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameIntroFlowController : MonoBehaviour
{
    public string nextScene = "MainScene";

    public AudioSource dialogueBGM;

    public RawImage introVideoImage;

    public VideoPlayer videoPlayer;

    public DialogueManager dialogueManager;

    private enum State
    {
        Video,
        Dialogue,
        End
    }

    private State state;

    private bool initialized;

    private bool inputLocked = true;

    void OnEnable()
    {
        GuitarInputManager.OnStringPlayed -= OnGuitarInput;
        GuitarInputManager.OnStringPlayed += OnGuitarInput;

        Debug.Log("Intro 已监听吉他输入");
    }


    void Start()
    {
        StartCoroutine(DelayedInitialize());
    }

    IEnumerator DelayedInitialize()
    {
        yield return null;
        yield return null;

        Initialize();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TitleScreen")
        {
            initialized = false;

            StartCoroutine(DelayedInitialize());
        }
    }

    void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        dialogueManager = FindObjectOfType<DialogueManager>(true);

        if (dialogueManager == null)
        {
            Debug.LogError("没有找到 DialogueManager");
            return;
        }

        dialogueManager.ResetState();

        dialogueManager.panel?.SetActive(false);

        dialogueManager.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }

        if (introVideoImage != null)
            introVideoImage.enabled = false;

        state = State.Video;

        inputLocked = false;

        StartCoroutine(Run());

        Debug.Log("Intro 初始化完成");
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(1);

        StartVideo();
    }

    void StartVideo()
    {
        state = State.Video;

        inputLocked = false;

        if (introVideoImage != null)
            introVideoImage.enabled = true;

        dialogueManager.panel?.SetActive(false);

        videoPlayer?.Play();

        StartCoroutine(VideoAutoEnd());

        Debug.Log("视频开始");
    }

    IEnumerator VideoAutoEnd()
    {
        // ★ 先等待视频真正开始播放
        yield return new WaitForSeconds(0.5f);
        
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            // ★ 如果视频没有播放，尝试播放
            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
                yield return new WaitForSeconds(1f);
            }
            
            // 等待视频播放完成
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }
        }

        Debug.Log("🎬 视频播放结束，切换到对话");
        EndVideoToDialogue();
    }

    void OnGuitarInput(int id)
    {
        Debug.Log(
            "Intro收到弦:{id} 状态:{state}"
        );


        if (state == State.End)
            return;


        if (id == 1)
        {
            if (state == State.Video)
            {
                Debug.Log("吉他跳过视频");
                EndVideoToDialogue();
            }
            else if (state == State.Dialogue)
            {
                Debug.Log("吉他跳过对话");
                EndFlow();
            }
        }


        if (id == 2 && state == State.Dialogue)
        {
            dialogueManager?.NextManual();
        }
    }

    void EndVideoToDialogue()
    {
        if (state != State.Video)
            return;

        state = State.Dialogue;

        StartCoroutine(SwitchToDialogue());
    }

    IEnumerator SwitchToDialogue()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }

        yield return new WaitForEndOfFrame();

        if (introVideoImage != null)
            introVideoImage.enabled = false;

        StartDialogue();
    }

    void StartDialogue()
    {
        state = State.Dialogue;

        if (dialogueBGM != null)
        {
            dialogueBGM.volume = 0;
            dialogueBGM.Play();
        }

        dialogueManager?.Begin(this);
    }

    public void OnDialogueFinished()
    {
        EndFlow();
    }

    void EndFlow()
    {
        state = State.End;

        dialogueBGM?.Stop();

        dialogueManager?.gameObject.SetActive(false);

        SceneManager.LoadScene(nextScene);
    }

    void OnDestroy()
    {
        GuitarInputManager.OnStringPlayed -= OnGuitarInput;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}