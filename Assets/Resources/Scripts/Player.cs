using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public AudioClip shootSound;
    public AudioClip reloadSound;
    private AudioSource source;

    private int MAX_BULLETS;
	public bool isShooting;
	public bool isReloading;
	public GameObject Weapon;
	private int bulletsLeft;
	private BulletsUI bulletsUI;
    public string name;

	void Start () {
		MAX_BULLETS = 5;
		isShooting = false;
		isReloading = false;
		bulletsLeft = 5;
		bulletsUI = new BulletsUI();

        source = GetComponent<AudioSource>();
    }

    void Update () {
    }

	public IEnumerator Shoot(){
		isShooting = true;
		bulletsLeft--;
		bulletsUI.RemoveBullet ();
		Weapon.GetComponent<Animation> ().Play ("shootAnim");
        source.Stop();
        source.PlayOneShot(shootSound);
        yield return new WaitForSeconds (0.5f);
		if (bulletsLeft == 0) {
			StartCoroutine(Reload ());
		}
		isShooting = false;
	}

	public IEnumerator Reload(){
		isReloading = true;
		Weapon.GetComponent<Animation> ().Play ("reloadAnim");
        source.Stop();
        source.PlayOneShot(reloadSound);
        yield return new WaitForSeconds (3f);
		bulletsUI.ShowAllBullets ();
		bulletsLeft = MAX_BULLETS;
		isReloading = false;
	}
}
