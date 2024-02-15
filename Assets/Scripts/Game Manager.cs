using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Ball ball;
    public LevelTimer levelTimer;
    public GameObject levelCompleteButtons;
    public Canvas Canvas;
    public EventSystem EventSystem;

    #region Variables
    public int currentLevel = 0;
    #endregion

    #region Methods/Functions
    private void LoadLevel(int level)
    {
        this.currentLevel = level;
        SceneManager.LoadScene($"Level {level}");
    }

    public void LoadNextLevel()
    {
        LoadLevel(currentLevel + 1);
    }

    private void NewGame()
    {
        currentLevel = 1;
        LoadLevel(currentLevel);
    }

    [ContextMenu("ResetLevel()")]
    public void ResetLevel()
    {
        player.ResetPlayer();
        levelTimer.ResetClock();
        ball.ResetBall();
        levelCompleteButtons.SetActive(false);
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        player = FindAnyObjectByType<Player>();
        ball = FindAnyObjectByType<Ball>();
        levelTimer = FindAnyObjectByType<LevelTimer>();
    }
    #endregion


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);  // we want this object to stay across all scenes so don't destroy when loading other scenes
        DontDestroyOnLoad(Canvas);
        DontDestroyOnLoad(EventSystem);
        SceneManager.sceneLoaded += OnLevelLoaded;
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene($"Main Menu");
    }

    // Update is called once per frame
    void Update()
    {
        if (ball.hitFloor)
            ResetLevel();
        if (ball.hitGoal)
        {
            levelCompleteButtons.SetActive(true);
            levelTimer.levelTimerActive = false;
        }
    }
}
