using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoresTable : MonoBehaviour {

    private DBManager DB;
    public Text table;
	// Use this for initialization
	void Start () {
        DB = new DBManager();
        table.text = "\nHIGH SCORES\n\n" + DB.getHighScoresTabe();
    }
    
	// Update is called once per frame
	void Update () {
		
	}


}
