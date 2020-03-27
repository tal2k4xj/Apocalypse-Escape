using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vuforia;
using System.Threading;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour , ITrackableEventHandler {

    public AudioClip startGameSound;
    private AudioSource source;

    private int MAX_ZOMBIES;
	private int MAX_CITIZENS;

	private TrackableBehaviour imageTrackEventHandler;
	private List<Zombie> zombies;
	private Player player;
	private int zombiesLeft;
    private int citizensLeft;
    private int citizensDied;
    private int citizensSurvived;
    private List<Citizen> citizens;
	private bool gameStarted;
    private bool gameEnd;
    private DBManager DB;
    private int score;

    public int gameLevel;
    public Zombie Zombie1Prefab;
    public Zombie Zombie2Prefab;
    public Zombie Zombie3Prefab;
    public Citizen CitizenPrefab;
    public Text ZombiesLeftText;
    public Text CitizensLeftText;
    public Text Level;
    public UnityEngine.UI.Image YouWin;
    public UnityEngine.UI.Image GameOver;


    public Text PlayerName;
	void Start () {
		zombies = new List<Zombie>();
		citizens = new List<Citizen>();
		player = Camera.main.GetComponent<Player> ();

        score = 0;

        zombiesLeft = 0;
        citizensLeft = 0;
        citizensDied = 0;
        citizensSurvived = 0;

        gameStarted = false;
        gameEnd = false;

        DB = new DBManager();
        PlayerName.text = DB.getPlayerName();
        player.name = PlayerName.text;
        gameLevel = DB.getPlayerLevel();
        Level.text = "LEVEL " + gameLevel;

        if(gameLevel == 1)
        {
            MAX_ZOMBIES = 3;
            MAX_CITIZENS = 2;
        }
        else
        {
            MAX_ZOMBIES = 4 * gameLevel / 2;
            MAX_CITIZENS = 3 * gameLevel / 2;
        }

        imageTrackEventHandler = GetComponent<TrackableBehaviour>();
		if (imageTrackEventHandler) {
			imageTrackEventHandler.RegisterTrackableEventHandler (this); //register handler to image delegate
		}

        StartCoroutine(stopLevelShow(4f));

        source = GetComponent<AudioSource>();
        source.PlayOneShot(startGameSound);
    }

    void Update()
    {
        if (!gameEnd)
        {
            CheckGameStatus();
            if (Input.GetKeyDown(KeyCode.Space) || (Input.touches.Length > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Began)))
            {
                if (!player.isShooting && !player.isReloading && gameStarted)
                {
                    StartCoroutine(player.Shoot());
                    Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 1500f))
                    {
                        foreach (Zombie z in zombies)
                        {
                            if (hit.transform == z.transform)
                            {
                                z.GotShot();
                            }
                        }
                    }
                }
            }
        }
    }

    void CheckGameStatus(){
		if (gameStarted) {//check if game started
            if((citizensDied > 0 || citizensSurvived > 0) && citizensLeft == 0 && (citizensSurvived+ citizensDied) >= MAX_CITIZENS)//game ends
            {
                CancelInvoke("SamplingPlayerLocation");
                gameEnd = true;
                if(citizensDied >= citizensSurvived) //loose
                {
                    GameOver.gameObject.SetActive(true);
                    ExitToHighScores();
                }
                else //win
                {
                    YouWin.gameObject.SetActive(true);
                    GoNextLevel();
                }
            }
		}
	}

    void GoNextLevel()
    {
        DB.updatePlayerLevel(player.name);
        DB.updatePlayerScore(player.name, score);
        StartCoroutine(NextLevel(4f));
    }

    void ExitToHighScores()
    {
        DB.updatePlayerScore(player.name, score);
        StartCoroutine(ExitGame(4f));
    }

    IEnumerator NextLevel(float timeToWait)//open next lvl in few seconds
    {
        yield return new WaitForSeconds(timeToWait);
        SceneManager.LoadScene("LevelsManager",LoadSceneMode.Single);
    }

    IEnumerator ExitGame(float timeToWait)//open next lvl in few seconds
    {
        yield return new WaitForSeconds(timeToWait);
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    IEnumerator stopLevelShow(float timeToWait)//stop the level text after few seconds
    {
        yield return new WaitForSeconds(timeToWait);
        Level.gameObject.SetActive(false);
    }

    public Vector3 GetPlayerPosition()//get specific position
    {
        return transform.InverseTransformPoint(Camera.main.transform.position);
    }

    public Vector3 GetVectorInWorldSpace(Vector3 v3)//return the position of object in world space
    {
        return transform.TransformPoint(v3);
    }

    void SamplingPlayerLocation()//get sample of player position to DB
    {
        Vector3 playerLocation = GetPlayerPosition();
        DB.updatePlayerLocation(player.name, playerLocation.x, playerLocation.y, playerLocation.z);
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus,TrackableBehaviour.Status newStatus)//invoke when trackable state changed
	{
		if (previousStatus == TrackableBehaviour.Status.NOT_FOUND && newStatus == TrackableBehaviour.Status.TRACKED) {
			OnTrackingFound (); //if image found - start all game objects
		}

		if (previousStatus == TrackableBehaviour.Status.TRACKED && newStatus == TrackableBehaviour.Status.NOT_FOUND) {
			OnTrackingLost (); //if image lost - stop all game obejcts
		}
	}

    private void OnTrackingFound()//if found (create / resume game)
	{
		if (citizens.Count == 0 && zombies.Count == 0 && !gameStarted) {//if game just started
            InvokeRepeating("CreateZombie", 5f, 5f);//create all zombies with time diff
            StartCoroutine(StopZombiesSpawn("CreateZombie", (5f + 5f * MAX_ZOMBIES - 1f)));
            InvokeRepeating("CreateCitizen", 6f, 6f);//create all citizens with time diff
            StartCoroutine(StopCitizensSpawn("CreateCitizen", (6f + 6f * MAX_CITIZENS - 1f)));
            InvokeRepeating("SamplingPlayerLocation", 0f, 5f);
            gameStarted = true;
        }

		if (zombies.Count > 0) {
			for (int i = 0; i < zombies.Count; i++) {
                if (zombies[i] != null)
                {
                    zombies[i].gameObject.SetActive(true);//activate zombie
                }
			}
		}

		if (citizens.Count > 0) {
			for (int i = 0; i < citizens.Count; i++) {
                if (citizens[i] != null)
                {
                    citizens[i].gameObject.SetActive(true);//activate citizen
                }
			}
		}
	}

	private void OnTrackingLost()//if lost (pause game)
	{
		if (zombies.Count > 0) {
			for (int i = 0; i < zombies.Count; i++) {
                if (zombies[i] != null)
                {
                    zombies[i].gameObject.SetActive(false);//disable zombie
                }
			}
		}

		if (citizens.Count > 0) {
			for (int i = 0; i < citizens.Count; i++) {
                if(citizens[i] != null)
                {
                    citizens[i].gameObject.SetActive(false);//disable citizen
                }
			}
		}
	}

    void CreateCitizen(){
        citizens.Add (Instantiate (CitizenPrefab, imageTrackEventHandler.transform));//create new citizen
        foreach (Zombie zomb in zombies)
        {
            zomb.GetCitizen(citizens[citizensLeft]);
        }
        UpdateCitizensLeft(1);
	}

    void CreateZombie(){
        int r = Mathf.FloorToInt(Random.Range(1f, 3.99f));
        switch (r)
        {
            case 1:
                zombies.Add(Instantiate(Zombie1Prefab, imageTrackEventHandler.transform));//create new zombie
                break;
            case 2:
                zombies.Add(Instantiate(Zombie2Prefab, imageTrackEventHandler.transform));//create new zombie
                break;
            case 3:
                zombies.Add(Instantiate(Zombie3Prefab, imageTrackEventHandler.transform));//create new zombie
                break;
        }
        
        zombies[zombiesLeft].GetCitizens(citizens);
        UpdateZombiesLeft(1);
	}

	IEnumerator StopZombiesSpawn(string func , float timeToWait){
		yield return new WaitForSeconds (timeToWait);
		CancelInvoke (func);
	}

	IEnumerator StopCitizensSpawn(string func , float timeToWait){
		yield return new WaitForSeconds (timeToWait);
		CancelInvoke (func);
	}

    public void CitizenSurvived(Citizen citizen){
		// todo : update player poinst and everything
		citizensSurvived++;
        UpdateCitizensLeft(-1);
        score = score + 50;
        citizens.Remove (citizen);
	}

	public void CitizenDied(Citizen citizen){
		// todo : update player poinst and everything
		citizensDied++;
        UpdateCitizensLeft(-1);
        score = score - 50;
        citizens.Remove (citizen);
	}

	public void KillZombie(Zombie zombie){
        UpdateZombiesLeft(-1);
        score = score + 100;
        zombies.Remove (zombie);// remove from the zombie list
	}

    void UpdateZombiesLeft(int num)
    {
        zombiesLeft += num;
        ZombiesLeftText.text = "Zombies : " + zombiesLeft;
    }

    void UpdateCitizensLeft(int num)
    {
        citizensLeft += num;
        CitizensLeftText.text = "Citizens : " + citizensLeft;
    }
}
