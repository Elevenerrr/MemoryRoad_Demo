using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [Header("SetingPanel")]
    
    //暂停菜单面板
    public GameObject MenuPanel;
    //标记当前是否处于暂停状态
    public bool isPaused = false;
    
    public void Start()
    {      
        //确保游戏开始时处于非暂停状态
        isPaused = false;
        MenuPanel.SetActive(false); //隐藏暂停菜单
    }
    private void Update()
    {
        //监听玩家按下Esc键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        void TogglePause()
        {
            //切换暂停状态
            isPaused = !isPaused;
            //根据当前状态显示或隐藏UI
            if (isPaused)
            {
                MenuPanel.SetActive(true);  //显示暂停菜单
                Time.timeScale = 0f;        //暂停游戏时间
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                MenuPanel.SetActive(false); //隐藏暂停菜单
                Time.timeScale = 1f;        //恢复游戏时间
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    //继续游戏 按钮点击事件
    public void OnClickContinue()
    {
        isPaused = false;
        MenuPanel.SetActive(false); //隐藏暂停菜单
        Time.timeScale = 1f;        //恢复游戏时间
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void OnClickQuit()
    {
        isPaused = false;
        MenuPanel.SetActive(false); //隐藏暂停菜单
        Time.timeScale = 1f;        //恢复游戏时间
    }
}
