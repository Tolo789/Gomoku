using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour {
    public void PlayClick() {
        SceneManager.LoadScene("Main");
    }
    public void ExitClick() {
        Application.Quit();
    }
}
