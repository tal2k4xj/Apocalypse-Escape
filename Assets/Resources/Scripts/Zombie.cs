using UnityEngine;
using System.Collections;
using Vuforia;
using System.Collections.Generic;
using System;
using System.Threading;

public class Zombie : MonoBehaviour{

    public AudioClip attack;
    public AudioClip dying;
    public AudioClip gotShot;
    public AudioClip spawn;
    private AudioSource source;
    /// <summary>
    /// genes and stats
    /// </summary>
    private float MOVE_SPEED;
	private float RUN_SPEED;
    public int POWER;
    private float WISDOM;
    private int HEALTH_MAX;
    private int HEALTH;
    private float COURAGE;
    /// <summary>
    /// weights for AI
    /// </summary>
    private float GetDistanceFromCitizenWeight;
    private float GetCitizenHealthBasedOnZombiePowerWeight;
    private float GetCitizenDistanceFromExitWeight;
    private float EstimatedLastTimeGotShotWeight;
    /// <summary>
    /// other parameters
    /// </summary>
    private GameObject victim;
    private bool isAttacking;
	private bool isDead;
	private Vector3 moveVector = new Vector3 ();
	private Vector3 rotationVector = new Vector3 ();
    private Vector3[] moveablePoints;
    private Vector3[] hidePoints;
    private List<Citizen> citizens;
    private Game game;
    private Player player;
    private float LastTimeGotShot;
    private DBManager DB;

    /// <summary>
    /// start - init all parameters and location , update - call every frame in the game.
    /// </summary>
    void Start () {
        game = GameObject.Find("ImageTarget").GetComponent<Game>();
        player = GameObject.Find("Camera").GetComponent<Player>();

        GetDistanceFromCitizenWeight = 0.5f;
        GetCitizenHealthBasedOnZombiePowerWeight = 0.5f;
        GetCitizenDistanceFromExitWeight = 0.5f;
        EstimatedLastTimeGotShotWeight = 0.5f;

        if (transform.name == "zombie1(Clone)")
        {
            MOVE_SPEED = 0.06f + (game.gameLevel / 1000f);
            RUN_SPEED = 0.08f + (game.gameLevel / 1000f);
            POWER = (int)(1f * ((game.gameLevel / 10f) + 1f));
            HEALTH_MAX = (int)(1f * ((game.gameLevel / 20f) + 1f));
        }
        else if (transform.name == "zombie2(Clone)")
        {
            MOVE_SPEED = 0.045f + (game.gameLevel / 1000f);
            RUN_SPEED = 0.07f + (game.gameLevel / 1000f);
            POWER = (int)(1f * ((game.gameLevel / 10f) + 1f));
            HEALTH_MAX = (int)(1f * ((game.gameLevel / 20f) + 1f));
        }
        else if (transform.name == "zombie3(Clone)")
        {
            MOVE_SPEED = 0.03f + (game.gameLevel / 1000f);
            RUN_SPEED = 0.06f + (game.gameLevel / 1000f);
            POWER = (int)(2f * ((game.gameLevel / 10f) + 1f));
            HEALTH_MAX = (int)(2f * ((game.gameLevel / 20f) + 1f));
        }
        HEALTH = HEALTH_MAX;
        WISDOM = UnityEngine.Random.Range(0f, 1f) + (game.gameLevel / 15f);
        if(WISDOM > 1) { WISDOM = 1;  }
        COURAGE = UnityEngine.Random.Range(0f, 1f) + (game.gameLevel / 15f);
        if (COURAGE > 1) { COURAGE = 1; }
        LastTimeGotShot = 0;

        isAttacking = false;
        isDead = false;
        DB = new DBManager();
        moveVector = new Vector3();
        rotationVector = new Vector3();
        int r = (int)UnityEngine.Random.Range(1f, 4.9f);

        switch (r) {
		case 1:
			moveVector = new Vector3 (0f, 0.0107f, -0.48f);
			rotationVector = new Vector3 (0f, 0f, 0f);
			break;
		case 2:
			moveVector = new Vector3 (0f, 0.0107f, 0.48f);
			rotationVector = new Vector3 (0f, 180f, 0f);
			break;
		case 3:
			moveVector = new Vector3 (0.48f, 0.0107f, 0f);
			rotationVector = new Vector3 (0f, -90f, 0f);
			break;
		case 4:
			moveVector = new Vector3 (-0.48f, 0.0107f, 0f);
			rotationVector = new Vector3 (0f, 90f, 0f);
			break;
		}
		transform.localPosition = moveVector; // set position
		transform.localEulerAngles = rotationVector; // set rotation

        moveablePoints = new Vector3[] {new Vector3(0.15f, 0.0107f, 0f) , new Vector3(-0.15f, 0.0107f, 0f) ,
                                        new Vector3(0f, 0.0107f, -0.15f) , new Vector3(0f, 0.0107f, 0.15f) };
        hidePoints = new Vector3[] {new Vector3(-0.15f, 0.0107f, 0.3f) , new Vector3(0.15f, 0.0107f, 0.3f) ,
                                    new Vector3(0.3f, 0.0107f, 0.15f) , new Vector3(0.3f, 0.0107f, -0.15f) ,
                                    new Vector3(0.15f, 0.0107f, -0.3f) , new Vector3(-0.15f, 0.0107f, -0.3f) ,
                                    new Vector3(-0.3f, 0.0107f, -0.15f) , new Vector3(-0.3f, 0.0107f, 0.15f),
                                    new Vector3(0f, 0.0107f, -0.08f),new Vector3(0f, 0.0107f, 0.08f),
                                    new Vector3(-0.08f, 0.0107f, 0f),new Vector3(0.08f, 0.0107f, 0f)};

        source = GetComponent<AudioSource>();
        source.PlayOneShot(spawn);
    }

	void Update () {
        if (!isDead) {
			if (isAttacking && !victim) {//if is attacking but the victim died
				GetComponent<Animation> ().Stop ();
				GetComponent<Animation> ().Play ("walk");
				isAttacking = false;
			} else if (!isAttacking) {
                DecisionMaking();
            }
		}
	}

    /// <summary>
    /// handle situation zombie got shot or die
    /// </summary>
     
    public void GotShot()
    {
        LastTimeGotShot = Time.time;
        HEALTH--;
        if(HEALTH <= 0)
        {
            Dead();
        }
        else
        {
            source.Stop();
            source.PlayOneShot(gotShot);
        }
    }

    public void Dead()
    {
        isDead = true;
        source.Stop();
        source.PlayOneShot(dying);
        if (victim)
        {
            victim.GetComponent<Citizen>().CancelInvoke("GotHit");
        }
        GetComponent<Animation>().Stop();
        GetComponent<Animation>().Play("back_fall");
        GameObject.Destroy(this.gameObject, 2f);
        game.KillZombie(this);
    }

    /// <summary>
    /// handle collision with citizens , other zombies and building
    /// </summary>

    void OnCollisionEnter(Collision col){
        if (!isDead)
        {
            if (col.collider.transform.name == "citizen(Clone)")
            {
                GetComponent<Animation>().Stop();
                GetComponent<Animation>().GetClip("attack").wrapMode = WrapMode.Loop;
                GetComponent<Animation>().Play("attack");
                victim = col.collider.gameObject;
                isAttacking = true;
                source.Stop();
                source.PlayOneShot(attack);
            }
            else if (col.collider.transform.name == "building")
            {
                transform.localEulerAngles += new Vector3(0, 180, 0);
            }
        }
	}

	void OnCollisionExit(Collision col){
		if (!isDead) {
		}
	}

    /// <summary>
    /// AI algorithm - input functions
    /// </summary>

    void DecisionMaking()
    {
        float decision1;
        float decision2c1;
        float decision2c2;
        
        if(EstimatedLastTimeGotShot() != 1)
        {
            if (IsPlayerReloading())//decision 1 is about zombie and player parameters - choose between hide or attack
            {
                decision1 = (WISDOM * 2 + COURAGE * (GetZombieHp() + EstimatedLastTimeGotShot())) / 4f;
            }
            else
            {
                decision1 = (WISDOM + COURAGE * (GetZombieHp() + EstimatedLastTimeGotShot())) / 3f;
            }
        }
        else
        {
            decision1 = 1;
        }

        if (decision1 >= 0.5)//decided to attack citizen
        {
            Citizen closestCitizen = FindClosestCitizen();
            Citizen citizenToSurvive = FindClosestCitizenToSurvive();
            if(closestCitizen == null && citizenToSurvive != null)
            {
                MoveToTarget(citizenToSurvive.transform.localPosition);
            }
            else if (closestCitizen != null && citizenToSurvive == null)
            {
                MoveToTarget(closestCitizen.transform.localPosition);
            }else if (closestCitizen != null && citizenToSurvive != null)//choose between two citizens - influenced by citizen health and distance
            {
                if(Vector3.Distance(transform.localPosition, closestCitizen.transform.localPosition)*2 <= Vector3.Distance(transform.localPosition, citizenToSurvive.transform.localPosition))
                {
                    MoveToTarget(closestCitizen.transform.localPosition);
                }
                else
                {
                    decision2c1 = (WISDOM * GetDistanceFromCitizen(closestCitizen) + GetCitizenHealthBasedOnZombiePower(closestCitizen)) / 2;
                    decision2c2 = (WISDOM * (GetDistanceFromCitizen(citizenToSurvive) + GetCitizenDistanceFromExit(citizenToSurvive)) + GetCitizenHealthBasedOnZombiePower(citizenToSurvive)) / 3;
                    if (decision2c1 > decision2c2)
                    {
                        MoveToTarget(closestCitizen.transform.localPosition);
                    }
                    else
                    {
                        MoveToTarget(citizenToSurvive.transform.localPosition);
                    }
                }
            }
            else //no citizens - move around the map
            {
                transform.Translate(Vector3.forward * 5f * Time.deltaTime);
                int r = (int)UnityEngine.Random.Range(0f, 1f);
                if (r == 0)
                    transform.Rotate(Vector3.right * 2f * Time.deltaTime);
                else
                    transform.Rotate(Vector3.left * 2f * Time.deltaTime);
            }
        }
        else//decided to hide depends on player position
        {
            HideFromPlayer();
        }
    }

    public void GetCitizens(List<Citizen> citizens)
    {
        this.citizens = citizens;
    }

    public void GetCitizen(Citizen citizen)
    {
        if(citizens == null)
        {
            citizens = new List<Citizen>();
        }
        citizens.Add(citizen);
    }

    Vector3 GetPlayerPosition()
    {
        return DB.getPlayerLocation(player.name);
    }

    bool IsPlayerReloading()
    {
        return player.isReloading;
    }

    /// <summary>
    /// AI algorithm - hidden functions
    /// </summary>

    Citizen FindClosestCitizen()
    {
        float distance = 0f;
        float minDist = float.MaxValue;
        Citizen closestCitizen = null;
        if (citizens != null)
        {
            foreach (Citizen c in citizens)
            {
                if (c != null)
                {
                    distance = Vector3.Distance(transform.localPosition, c.transform.localPosition);
                    if (distance < minDist)
                    {
                        minDist = distance;
                        closestCitizen = c;
                    }
                }
            }
        }
        return closestCitizen;
    }

    Citizen FindClosestCitizenToSurvive()
    {
        float distance = 0f;
        float distanceToSurvive = float.MaxValue;
        Citizen closestCitizenToSurvive = null;
        if (citizens != null)
        {
            foreach (Citizen c in citizens)
            {
                if (c != null)
                {
                    distance = Vector3.Distance(transform.localPosition, c.transform.localPosition);
                    if (distance < 0.5f && c.GetDistanceToExit() < distanceToSurvive)
                    {
                        distanceToSurvive = c.GetDistanceToExit();
                        closestCitizenToSurvive = c;
                    }
                }
            }
        }
        return closestCitizenToSurvive;
    }

    float GetDistanceFromCitizen(Citizen c)//return value between 0 to 1 (0 is far , 1 is close)
    {
        float distance = Vector3.Distance(transform.localPosition, c.transform.localPosition);
        float numToReturn = distance / GetDistanceFromCitizenWeight;
        if(numToReturn >= 1)
        {
            if(GetDistanceFromCitizenWeight < 1)
                GetDistanceFromCitizenWeight += 0.001f;
            return 0;
        }
        else if (numToReturn == 0)
        {
            return 1;
        }
        else
        {
            if(numToReturn < 0.5 && GetDistanceFromCitizenWeight > 0)
                GetDistanceFromCitizenWeight -= 0.001f;
            return 1 - numToReturn;
        }
    }

    float GetCitizenHealthBasedOnZombiePower(Citizen c)//return value berween 0 to 1 (0 is hard to kill , 1 is easy to kill)
    {
        float numToReturn;
        if (c.hp != 0)
        {
            numToReturn = POWER / (c.hp * GetCitizenHealthBasedOnZombiePowerWeight);
        }
        else
        {
            return 0;
        }

        if (numToReturn >= 1)
        {
            if (GetCitizenHealthBasedOnZombiePowerWeight < 1)
                GetCitizenHealthBasedOnZombiePowerWeight += 0.001f;
            return 1;
        }
        else
        {
            if (numToReturn < 0.5 && GetCitizenHealthBasedOnZombiePowerWeight > 0)
                GetCitizenHealthBasedOnZombiePowerWeight -= 0.001f;
            return numToReturn;
        }
    }

    float GetCitizenDistanceFromExit(Citizen c)//return value between 0 to 1 (0 is very far from exit , 1 is very close to exit)
    {
        float numToReturn = c.GetDistanceToExit() / GetCitizenDistanceFromExitWeight;
        if(numToReturn >= 1)
        {
            if (GetCitizenDistanceFromExitWeight < 1)
                GetCitizenDistanceFromExitWeight += 0.001f;
            return 0;
        }
        else
        {
            if (numToReturn < 0.5 && GetCitizenDistanceFromExitWeight > 0)
                GetCitizenDistanceFromExitWeight -= 0.001f;
            return 1 - numToReturn;
        }
    }

    float GetZombieHp()//return value between 0 to 1 (0 is low hp , 1 is full hp)
    {
        return HEALTH / HEALTH_MAX;
    }

    float EstimatedLastTimeGotShot()//return value between 0 to 1 (0 is just got shot , 1 is long time passed from last shot)
    {
        float numToReturn = (Time.time - LastTimeGotShot) / (7f * EstimatedLastTimeGotShotWeight);
        if(numToReturn >= 1)
        {
            if (EstimatedLastTimeGotShotWeight < 1)
                EstimatedLastTimeGotShotWeight += 0.001f;
            return 1;
        }
        else
        {
            if (numToReturn < 0.5 && EstimatedLastTimeGotShotWeight > 0)
                EstimatedLastTimeGotShotWeight -= 0.001f;
            return numToReturn;
        }
    }

    /// <summary>
    /// AI algorithm - output function
    /// </summary>

    void HideFromPlayer()
    {
        Vector3 playerPoision = GetPlayerPosition();
        if (playerPoision.x > 0.5f)
        {
            if (playerPoision.z > 0.5f)
            {
                RunToTarget(FindClosetHideablePoint(hidePoints[1], hidePoints[2]));
            }
            else if (playerPoision.z < -0.5f)
            {
                RunToTarget(FindClosetHideablePoint(hidePoints[3], hidePoints[4]));
            }
            else
            {
                RunToTarget(FindClosetHideablePoint(hidePoints[1], hidePoints[4]));
            }
        }
        else if (playerPoision.x < -0.5f)
        {
            if (playerPoision.z > 0.5f)
            {
                RunToTarget(FindClosetHideablePoint(hidePoints[0], hidePoints[7]));
            }
            else if (playerPoision.z < -0.5f)
            {
                RunToTarget(FindClosetHideablePoint(hidePoints[5], hidePoints[6]));
            }
            else
            {
                RunToTarget(FindClosetHideablePoint(hidePoints[0], hidePoints[5]));
            }
        }
        else
        {
            if (playerPoision.z > 0.5f)
            {
                RunToTarget(FindClosetHideablePoint(hidePoints[2], hidePoints[7]));
            }
            else
            {
                RunToTarget(FindClosetHideablePoint(hidePoints[3], hidePoints[6]));
            }
        }
    }

    Vector3 FindClosetHideablePoint(Vector3 h1, Vector3 h2)
    {
        float distance1 = Vector3.Distance(transform.position, game.GetVectorInWorldSpace(h1));
        float distance2 = Vector3.Distance(transform.position, game.GetVectorInWorldSpace(h2));
        if (distance1 < distance2)
        {
            return h1;
        }
        else
        {
            return h2;
        }
    }

    void MoveToTarget(Vector3 target)
    {
        float step = MOVE_SPEED * Time.deltaTime;

        if (CheckObjectBetweenTwoTargets(transform.localPosition, target))
        {
            Vector3 newTarget = FindClosestMoveablePoint(target);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, newTarget, step);
            RotateToTarget(newTarget);
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, step);
            RotateToTarget(target);
        }
    }

    void RunToTarget(Vector3 target)
    {
        float step = RUN_SPEED * Time.deltaTime;

        if (CheckObjectBetweenTwoTargets(transform.localPosition, target))
        {
            Vector3 newTarget = FindClosestMoveablePoint(target);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, newTarget, step);
            RotateToTarget(newTarget);
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, step);
            RotateToTarget(target);
        }
    }

    void RotateToTarget(Vector3 target)
    {
        Quaternion rotate = Quaternion.LookRotation(target - transform.localPosition);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotate, 360);
    }

    Vector3 FindClosestMoveablePoint(Vector3 target)
    {
        float distance = 0f;
        float minDist = float.MaxValue;
        Vector3 closestPoint = new Vector3(0, 0, 0);
        foreach (Vector3 v in moveablePoints)
        {
            distance = Vector3.Distance(transform.position, game.GetVectorInWorldSpace(v));
            if (distance < minDist && !CheckObjectBetweenTwoTargets(v, target))
            {
                minDist = distance;
                closestPoint = v;
            }
        }
        if (closestPoint.x == 0 && closestPoint.y == 0 && closestPoint.z == 0)
        {
            float maxDist = 0;
            float minDistFromTarget = float.MaxValue;
            float distanceToTarget = 0f;
            foreach (Vector3 v in moveablePoints)
            {
                distanceToTarget = Vector3.Distance(game.GetVectorInWorldSpace(target), game.GetVectorInWorldSpace(v));
                distance = Vector3.Distance(transform.position, game.GetVectorInWorldSpace(v));
                if (distance > maxDist && distanceToTarget < minDistFromTarget && !CheckObjectBetweenTwoTargets(transform.localPosition, v))
                {
                    minDistFromTarget = distanceToTarget;
                    maxDist = distance;
                    closestPoint = v;
                }
            }
        }
        else
        {
            return closestPoint;
        }
        return closestPoint;
    }

    bool CheckObjectBetweenTwoTargets(Vector3 from, Vector3 to)
    {
        RaycastHit hit;
        if (Physics.Linecast(game.GetVectorInWorldSpace(from), game.GetVectorInWorldSpace(to), out hit))
        {
            if (hit.collider.transform.name == "building")
            {
                return true;
            }
        }
        return false;
    }
}
