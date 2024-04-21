using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BulletController : MonoBehaviour {

    private string JUGADOR = "Jugador";
    private string ENEMY = "Enemy";

    private string PLAYER_KNOCK_BACK = "playerKnockBack";
    private string ENEMY_KNOCK_BACK = "enemyKnockBack";

    private string GROUND = "Ground";

    [HideInInspector]
	public Vector2 mov;

	public float speed;

	private int damage = 0;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		transform.position += new Vector3 (mov.x, mov.y, 0) * speed * Time.deltaTime;
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (col.tag != JUGADOR) {
			if (col.tag == GROUND) {
				destroyBullet ();
			}
			if (col.tag == ENEMY) {
				col.SendMessage (PLAYER_KNOCK_BACK, Mathf.Abs (damage));
				destroyBullet ();
			}
		}
		if (col.tag != ENEMY) {
			if (col.tag == GROUND) {
				destroyBullet ();
			}
			if (col.tag == JUGADOR) {
				object[] message = new object[2];
				message [0] = transform.position.x;
				message [1] = Mathf.Abs (damage);
				col.SendMessage (ENEMY_KNOCK_BACK, message);
				destroyBullet ();
			}
		}
	}

	void destroyBullet () {
		Destroy (gameObject);
	}

	public void setDamage (int damage) {
		this.damage = damage;
	}

	void OnBecameInvisible () {
		destroyBullet ();
	}
}
