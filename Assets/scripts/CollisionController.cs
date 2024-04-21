using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionController : MonoBehaviour {

    private string JUGADOR = "Jugador";
    private string ENEMY = "Enemy";

	private string PIES = "Pies";
	private string INVISIBLE_WALL_ENEMY = "InvisibleWallEnemy";

    private Collider2D cld;

	// Use this for initialization
	void Start () {
		cld = GetComponent<Collider2D> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnCollisionEnter2D (Collision2D col) {
		if (cld.gameObject.tag == JUGADOR || cld.gameObject.tag == PIES) {
			if (col.gameObject.tag == INVISIBLE_WALL_ENEMY) {
				Physics2D.IgnoreCollision (col.collider, cld, true);
			}
		}
		if (cld.gameObject.tag == ENEMY) {
			if (col.gameObject.tag == ENEMY) {
				Physics2D.IgnoreCollision (col.collider, cld, true);
			}
		}
	}
}
