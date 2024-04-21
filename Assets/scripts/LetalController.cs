using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetalController : MonoBehaviour {

	private string JUGADOR = "Jugador";

	private string ENEMY_KNOCK_BACK = "enemyKnockBack";
	
	public int damage;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnCollisionEnter2D (Collision2D col) {
		if (col.gameObject.tag == JUGADOR) {
			object[] message = new object[2];
			message [0] = transform.position.x;
			message [1] = damage;
			col.gameObject.SendMessage (ENEMY_KNOCK_BACK, message);
		}
	}
}
