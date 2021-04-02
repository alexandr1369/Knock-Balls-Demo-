using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager instance;
    private void Awake()
    {
        if (instance) 
            Destroy(instance.gameObject);
        instance = this;
    }

    #endregion

    [SerializeField]
    private Transform cannonTransform; // cannon transform
    [SerializeField]
    private Animator cannonAnimator; // cannon animator

    // current bullets amount
    [SerializeField] 
    private Text bulletsAmountText;
    private int bulletsAmount;

    // shooting cool down
    [SerializeField]
    private float coolDown = 1f;
    private float currentCoolDown;

    [SerializeField] 
    private List<GameObject> bulletPrefabs; // bullet prefabs
    [SerializeField] 
    private GameObject bulletSpawn; // bullet spawn point

    [SerializeField]
    private float zCameraDistance = 22.5f; // distance from camera on the OZ (shooting utility)

    // cannon rotation utils
    [SerializeField]
    private float yCannonMinOffset = 55f;
    [SerializeField]
    private float yCannonMaxOffset = 125f;

    private List<GameObject> enemies; // list of all boxes on the game scene (active)

    private int currentGameLevel; // current game level (number)

    private void Start()
    {
        Init();
    }
    private void Update()
    {
        // shoot (PC demo)
        if (Input.GetMouseButtonDown(0) && currentCoolDown <= 0)
            Shoot(Input.mousePosition);

        // shoot (mobile)
        if(Input.touchCount > 0 && currentCoolDown <= 0)
            Shoot(Input.touches[0].position);

        // continue waiting
        if (currentCoolDown > 0)
            currentCoolDown -= Time.deltaTime;

        // check for victory
        if(enemies.Count == 0)
        {
            // play next level (without effects or transitions)
            // DEMO
            if(currentGameLevel == 6)
                SceneManager.LoadScene(0);
            else
            {
                FacebookManager.instance.AchievedLevel(currentGameLevel);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }

        // check for loose 
        if(bulletsAmount == 0 && enemies.Count > 0 && GameObject.FindGameObjectsWithTag("Bullet").Length == 0)
        {
            // check for still moving enemies (they may still falling -> wait)
            if(enemies.FindAll(t => t.transform.GetComponent<Rigidbody>().velocity.magnitude > 0).Count == 0)
            {
                // just reload current level
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
    private void Init()
    {
        // target is 60 FPS
        Application.targetFrameRate = 60;

        // get current game level
        // (возьмем на заметку для ДЕМО, что 1 уровень начинается с buildIndex == 0)
        currentGameLevel = SceneManager.GetActiveScene().buildIndex + 1;

        // get bullets amount
        bulletsAmount = currentGameLevel * 3;
        bulletsAmountText.text = (bulletsAmount < 10 ? "0" : "") + bulletsAmount.ToString();

        // get enemies
        enemies = new List<GameObject>();
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            enemies.Add(enemy);

        // set start shooting cool down
        currentCoolDown = 0f;

        // change cannon animations's speed according to cool down time
        float shootingDuration = 0, reloadingDuration = 0;
        foreach (AnimationClip clip in cannonAnimator.runtimeAnimatorController.animationClips)
        {
            // get 'shoot' duration
            if (shootingDuration == 0 && clip.name == "Cannon|Shoot")
                shootingDuration = clip.length;

            // get 'reload' duration
            if (reloadingDuration == 0 && clip.name == "Cannon|Reload")
                reloadingDuration = clip.length;
        }

        float multiplier = (reloadingDuration + shootingDuration) / coolDown;
        cannonAnimator.SetFloat("ShootSpeed", shootingDuration * multiplier);
        cannonAnimator.SetFloat("ReloadSpeed", reloadingDuration * multiplier);
    }

    // remove enemy from list
    public void RemoveEnemy(GameObject enemy) => enemies.Remove(enemy);

    // shoot the cannon ball
    private void Shoot(Vector3 touchPosition)
    {
        if (bulletsAmount <= 0)
            return;

        // decrease bullets amount
        bulletsAmount--;
        bulletsAmountText.text = (bulletsAmount < 10 ? "0" : "") + bulletsAmount.ToString();

        // set cannon rotation
        float xScreenTouch = touchPosition.x;
        Vector3 cannonEulerAngles = new Vector3(cannonTransform.eulerAngles.x,
            Mathf.Lerp(yCannonMinOffset, yCannonMaxOffset, xScreenTouch / Screen.width),
            cannonTransform.eulerAngles.z);
        cannonTransform.eulerAngles = cannonEulerAngles;

        // play cannon 'shoot' animation
        cannonAnimator.SetTrigger("Shoot");

        // get bullet
        GameObject cannonBall = Instantiate(bulletPrefabs[Random.Range(0, bulletPrefabs.Count)]);
        cannonBall.transform.position = bulletSpawn.transform.position;

        // shoot action
        Vector3 shootPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, zCameraDistance));
        cannonBall.transform.LookAt(shootPosition);
        cannonBall.GetComponent<Rigidbody>().AddForce(cannonBall.transform.forward * 50, ForceMode.Impulse);

        // set bullet life time
        Destroy(cannonBall, 1.25f);

        // reset current cool down
        currentCoolDown = coolDown;
    }
}
