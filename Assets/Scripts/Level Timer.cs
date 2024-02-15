using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{

    public TextMeshProUGUI levelTimerUI;
    private int levelTimer = 0;
    public bool levelTimerActive = false;


    string NumberToClock(int number)
    {
        int minutes = Mathf.FloorToInt(number / 60);
        int seconds = Mathf.FloorToInt(number % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ResetClock()
    {
        levelTimer = 0;
        levelTimerActive = false;
        levelTimerUI.text = "00:00";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (levelTimerActive)
        {
            levelTimer += 1;
            string levelTimerS = NumberToClock(levelTimer);
            levelTimerUI.text = levelTimerS;
        }
    }
}
