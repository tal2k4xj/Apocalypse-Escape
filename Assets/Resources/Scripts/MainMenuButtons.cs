using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour {

    public Button PlayGameButton;
    public Button HighScoresButton;
    public Button InstructionButton;
    public Button DownloadButton;
    public Text EnterWithOldNick;
    public GameObject EnterNickPanel;
    public InputField InputField;
    public GameObject AlertPanel;
    public AudioClip MenuSound;
    private AudioSource source;
    private DBManager DB;

    void Start()
    {
        source = GetComponent<AudioSource>();
        source.PlayOneShot(MenuSound);
        DB = new DBManager();
        InputField.keyboardType = TouchScreenKeyboardType.ASCIICapable;
    }

    public void MoveScene(string scene){
        SceneManager.LoadScene(scene,LoadSceneMode.Single);
    }

    public void DownloadGameBoard(){
        Application.OpenURL("https://www.dropbox.com/s/8drijt539xa83ul/ApoTargetImage.jpg?dl=0");
    }

    public void EnterPlayerName()
    {
        PlayGameButton.gameObject.SetActive(false);
        InstructionButton.gameObject.SetActive(false);
        DownloadButton.gameObject.SetActive(false);
        HighScoresButton.gameObject.SetActive(false);
        EnterNickPanel.SetActive(true);
        EnterWithOldNick.text = "Enter As: " + getLastPlayerName();
    }

    public void StartGame()
    {
        if (InputField.text.Equals(""))
        {
            InvokeRepeating("OpenAlertMsg", 0f, 0.5f);
            StartCoroutine(StopAlertMsg("OpenAlertMsg", 1.5f));
        }
        else
        {
            DB.insertPlayer(InputField.text);
            SceneManager.LoadScene("LevelsManager", LoadSceneMode.Single);
        }
    }

    public void StartGameWithOldNick()
    {
        DB.insertPlayer(getLastPlayerName());
        SceneManager.LoadScene("LevelsManager", LoadSceneMode.Single);
    }

    void OpenAlertMsg()
    {
        if (AlertPanel.activeSelf)
        {
            AlertPanel.SetActive(false);
        }
        else
        {
            AlertPanel.SetActive(true);
        }
    }

    IEnumerator StopAlertMsg(string func, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        CancelInvoke(func);
    }

    string getLastPlayerName()
    {
        string name = DB.getPlayerName();
        if (name != null)
            return name;
        else
            return "Player";
    }
}
