using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour {

    public void PlayClick(GameObject playSettings) {
        playSettings.SetActive(true);
    }
    public void ExitClick() {
        Application.Quit();
    }
    public void GoClick(GameObject playSettings) {
        playSettings.SetActive(true);
        SceneManager.LoadScene("Main");
    }
     public void GoBack(GameObject playSettings) {
       playSettings.SetActive(false);
    }
}
