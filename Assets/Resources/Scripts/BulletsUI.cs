using UnityEngine;
using System.Collections;

public class BulletsUI {

	private int MAX_BULLETS;
	private int bulletsleft;
	private Object bulletPrefab;
	private GameObject[] bulletsArray;
	private Vector3[] bulletsPos;

	public BulletsUI(){
		MAX_BULLETS = 5;
		bulletsleft = MAX_BULLETS;
		bulletsArray = new GameObject[MAX_BULLETS];
		bulletPrefab = Resources.Load ("prefabs/bullet", typeof(GameObject));
		bulletsPos = new Vector3[] {
			new Vector3 (-0.12f, -0.55f, 2f),
			new Vector3 (-0.06f, -0.55f, 2f),
			new Vector3 (0f, -0.55f, 2f),
			new Vector3 (0.06f, -0.55f, 2f),
			new Vector3 (0.12f, -0.55f, 2f),
		};

		for (int i = 0; i < bulletsArray.Length; i++) {
			bulletsArray [i] = GameObject.Instantiate (bulletPrefab,Camera.main.transform) as GameObject;
			bulletsArray [i].transform.localPosition = bulletsPos [i];
			bulletsArray [i].SetActive (true);
		}
	}

	public void RemoveBullet(){
		bulletsleft--;
		bulletsArray [bulletsleft].SetActive (false);
	}

	public void ShowAllBullets(){
		for (int i = 0; i < bulletsArray.Length; i++) {
			bulletsArray [i].SetActive (true);
		}
		bulletsleft = MAX_BULLETS;
	}
}
