using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;
    private void Start(){
        if(instance == null){
            instance = this;
        }else{
            instance.gameObject.SetActive(true);
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    public void StartSimulation()
    {
        SceneManager.LoadScene("Boids");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
