using UnityEngine;


public class WindBladeShooter : MonoBehaviour
{

    public GameObject yellowBladePrefab;
    public GameObject greenBladePrefab;
    public GameObject bluePurpleBladePrefab;


    public Transform spawnPoint;



    void OnEnable()
    {
        GuitarInputManager.OnStringPlayed += Shoot;
    }


    void OnDisable()
    {
        GuitarInputManager.OnStringPlayed -= Shoot;
    }




    void Shoot(int id)
    {

        if (id == 1)
            Fire(yellowBladePrefab, NoteColor.Yellow);


        if (id == 2)
            Fire(greenBladePrefab, NoteColor.Green);


        if (id == 3)
            Fire(bluePurpleBladePrefab, NoteColor.BluePurple);

    }




    void Fire(GameObject prefab, NoteColor color)
    {

        if (prefab == null)
            return;


        GameObject blade =
        Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );


        WindBlade wb =
        blade.GetComponent<WindBlade>();


        if (wb != null)
            wb.Initialize(
                HitManager.FindTarget(color)
            );
    }
}