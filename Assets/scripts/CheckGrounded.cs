using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGrounded : MonoBehaviour {

    private string GROUND = "Ground";
    private string WATER = "Water";

    private PlayerController player;

	// Use this for initialization
	void Start () {
		player = GetComponentInParent<PlayerController> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnCollisionStay2D (Collision2D col) {
		if (col.gameObject.tag == GROUND || col.gameObject.tag == WATER) {
			player.grounded = true;
		}
	}

	void OnCollisionExit2D (Collision2D col) {
		if (col.gameObject.tag == GROUND || col.gameObject.tag == WATER) {
			player.grounded = false;
		}
	}
}
