using UnityEngine;
using System.Collections;
using Vuforia;
using System;

public class Citizen : MonoBehaviour {

    public AudioClip scream;
    private AudioSource source;

    private float MOVE_SPEED;
	private int CITIZEN_HP;
	private float moveSpeed;
	private bool isUnderAttack;
	private GameObject attacker;
    private int attackerPower;
	private bool isDead;
	private Vector3 startPosVector;
	private Vector3 startRotationVector;
	private Vector3 endPosVector;
    private Game game;
    public int hp;

	void Start () {
		MOVE_SPEED = 3f;
		CITIZEN_HP = 5;
		moveSpeed = MOVE_SPEED;
		isUnderAttack = false;
		isDead = false;
		hp = CITIZEN_HP;
        int r = (int)UnityEngine.Random.Range (1f,5f);
		startPosVector = new Vector3 ();
		startRotationVector = new Vector3 ();
		endPosVector = new Vector3 ();

        game = GameObject.Find("ImageTarget").GetComponent<Game>();

        switch (r) {
		case 1:
			startPosVector = new Vector3 (0f, 0.0107f, -0.09f);
			startRotationVector = new Vector3 (0f, 180f, 0f);
			endPosVector = new Vector3 (0f, 0.0107f, -0.5f);
			break;
		case 2:
			startPosVector = new Vector3 (0f, 0.0107f, 00.09f);
			startRotationVector = new Vector3 (0f, 0f, 0f);
			endPosVector = new Vector3 (0f, 0.0107f, 0.5f);
			break;
		case 3:
			startPosVector = new Vector3 (0.09f, 0.0107f, 0f);
			startRotationVector = new Vector3 (0f, 90f, 0f);
			endPosVector = new Vector3 (0.5f, 0.0107f, 0f);
			break;
		case 4:
			startPosVector = new Vector3 (-0.09f, 0.0107f, 0f);
			startRotationVector = new Vector3 (0f, -90f, 0f);
			endPosVector = new Vector3 (-0.5f, 0.0107f, 0f);
			break;
		}
			
		transform.localScale = new Vector3 (0.07f, 0.07f, 0.07f); //set scale
		transform.localPosition = startPosVector; // set position
		transform.localEulerAngles = startRotationVector;

        source = GetComponent<AudioSource>();
    }

	void OnCollisionEnter(Collision col){
		if (!isDead) {
			if (col.collider.transform.name == "zombie1(Clone)" || col.collider.transform.name == "zombie2(Clone)" || col.collider.transform.name == "zombie3(Clone)") {
				moveSpeed = 0f;
				isUnderAttack = true;
                GetComponent<Animation> ().Stop ();
                GetComponent<Animation>().Play("Damage2");
                attacker = col.collider.gameObject;
                attackerPower = attacker.GetComponent<Zombie>().POWER;
                source.Stop();
                source.PlayOneShot(scream);
                InvokeRepeating ("GotHit", 0f, 1f);
			}
		}
	}
	
	void Update () {
		if (!isDead) {
			if (isUnderAttack && !attacker) {//if was under attack but the attacker died
				CancelInvoke("GotHit");
				moveSpeed = MOVE_SPEED;
                GetComponent<Animation>().Stop();
                GetComponent<Animation> ().Play ("Walk");
				isUnderAttack = false;	
			}else{
				transform.Translate (Vector3.forward * moveSpeed * Time.deltaTime);//move forward
			}
			CheckHP ();
			CheckExit ();
		}
	}

	void GotHit(){
		hp = hp - attackerPower;
	}

	void CheckHP(){
		if (hp <= 0) {
			Dead ();
		}
	}

	void CheckExit(){
		if (GetDistanceToExit() <= 0.005) {//if reached exit
            game.CitizenSurvived (this);//let game know citizen survived
			GameObject.Destroy (this.gameObject, 0f);//remove citizen
		}
	}

	public void Dead(){
		isDead = true;
		GetComponent<Animation>().Stop();
		GetComponent<Animation>().Play("Death");
        game.CitizenDied (this);//let game know citizen died
		GameObject.Destroy (this.gameObject , 2f);
	}

    public float GetDistanceToExit()
    {
        return Vector3.Distance(transform.localPosition, endPosVector);
    }
}
