using UnityEngine;
using UnityEngine.SceneManagement;

public class WindBladeShooter : MonoBehaviour
{
    public GameObject yellowBladePrefab;
    public GameObject greenBladePrefab;
    public GameObject bluePurpleBladePrefab;

    public Transform spawnPoint;

    private bool canShoot = false;


    private void OnEnable()
    {
        GuitarInputManager.OnStringPlayed += Shoot;

        SceneManager.sceneLoaded += OnSceneLoaded;

        CheckCurrentScene();
    }


    private void OnDisable()
    {
        GuitarInputManager.OnStringPlayed -= Shoot;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene();
    }


    void CheckCurrentScene()
    {
        canShoot = SceneManager.GetActiveScene().name == "MainScene";

        Debug.Log(
            $"WindBladeShooter 状态: {(canShoot ? "开启" : "关闭")}"
        );
    }


    void Shoot(int id)
    {
        // ★ 只允许 MainScene 发射
        if (!canShoot)
            return;


        switch (id)
        {
            case 1:
                Fire(yellowBladePrefab, NoteColor.Yellow);
                break;

            case 2:
                Fire(greenBladePrefab, NoteColor.Green);
                break;

            case 3:
                Fire(bluePurpleBladePrefab, NoteColor.BluePurple);
                break;
        }
    }


    void Fire(GameObject prefab, NoteColor color)
    {
        if (prefab == null)
            return;


        Note target = HitManager.FindTarget(color);


        GameObject blade =
            Instantiate(
                prefab,
                spawnPoint.position,
                spawnPoint.rotation
            );


        WindBlade wb =
            blade.GetComponent<WindBlade>();


        if (wb != null)
        {
            wb.Initialize(target);
        }
    }
}