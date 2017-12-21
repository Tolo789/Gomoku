using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour {
	[HideInInspector]
	public GameManager gameManager;

    public Button playButton;
    public GameObject settingsButton;
    public GameObject quitButton;

    void Start()
    {
        Button btn = playButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }
    void TaskOnClick() {
        Application.LoadLevel("Main");
    }
}
