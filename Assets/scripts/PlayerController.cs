using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

	private string MENU_INICIO = "menuInicio";

	private string HORIZONTAL = "Horizontal";
    private string ENABLE_MOVEMENT = "enableMovement";

    private string PLAYER_ATTACK = "Jugador_Atacar";
	private string CRYSTAL = "Crystal";

    private string TAKE_DAMAGE = "takeDamage";
	private string TAKE_HEAL = "takeHeal";
    private string PLAYER_DEATH = "playerDeath";
    private string SPEED = "speed";

    private string ATTACKING = "attacking";
    private string GROUNDED = "grounded";
	private string DEATH = "death";

	public float maxSpeed = 5f;
	public float speed = 2f;
	public bool grounded;

	public float jumpPower = 6.5f;
	private bool jump;
	private bool movement = true;

	private Rigidbody2D rb2d;
	private Animator anim;
	private SpriteRenderer spr;

	public Vector2 scale;

	public GameObject healthbar;
	public int damage;

	public GameObject hand;
	public GameObject bulletPrefab;

	private bool death = false;
	public float timeAnimationDeath;

	private int crystals = 0;
	private int maxCrystals = 10;
	public TextMesh txt_crystals;
	public TextMesh txt_bonus;

void Awake () {
		Assert.IsNotNull (healthbar);
		Assert.IsNotNull (hand);
		Assert.IsNotNull (bulletPrefab);
		Assert.AreNotApproximatelyEqual (0.0f, Mathf.Abs (damage));

        Assert.IsNotNull (txt_crystals);
        Assert.IsNotNull (txt_bonus);
	}

	// Use this for initialization
	void Start () {
		rb2d = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		spr = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		float vel = Mathf.Abs (rb2d.velocity.x);
		anim.SetFloat (SPEED, vel);
		anim.SetBool (GROUNDED, grounded);

		updateCrystals ();

		if (grounded && !death) {
			if ((Input.GetKeyDown (KeyCode.UpArrow))) {
				jump = true;
			}
			if (Input.GetKeyDown (KeyCode.Space)) {
				playerShooting ();
			}
			if (Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl)) {
				consumeCrystals ();
			}
		}
	}

	void FixedUpdate () {
		AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo (0);
		bool attacking = stateInfo.IsName (PLAYER_ATTACK);
		if (!attacking && !death) {
			moving ();
			jumping ();
		}
	}

	void moving () {
		float h = Input.GetAxis (HORIZONTAL);
		if (!movement) {
			h = 0;
		}
		rb2d.AddForce (Vector2.right * speed * h);

		float limitedSpeed = Mathf.Clamp (rb2d.velocity.x, -maxSpeed, maxSpeed);
		rb2d.velocity = new Vector2 (limitedSpeed, rb2d.velocity.y);

        // Direccion a la que se apunta
        if (h > 0.01f) {
            transform.localScale = new Vector3 (scale.x, scale.y, 1);
		}
		if (h < -0.01f) {
			transform.localScale = new Vector3 (-scale.x, scale.y, 1);
		}
	}

	void jumping () {
		if (jump) {
			rb2d.AddForce (Vector2.up * jumpPower, ForceMode2D.Impulse);
			jump = false;
		}
	}

	public void playerShooting () {
		float vel = Mathf.Abs (rb2d.velocity.x);

		if (vel <= 0.1f && vel >= -0.1f) {// Aproximandose a cero
			AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo (0);
			bool attacking = stateInfo.IsName (PLAYER_ATTACK);

			if (!attacking) {
				// Ataque
				Quaternion rotation = transform.rotation;
				Vector3 position = hand.transform.position;
				GameObject bullet = (GameObject)Instantiate (bulletPrefab, position, rotation);

				BulletController bc = bullet.GetComponent<BulletController> ();
				bc.setDamage (damage);

				if (transform.localScale.x < 0) {
					bc.mov.x = -1f;
				} else {
					bc.mov.x = 1f;
				}
				bc.mov.y = 0f;

				// Animacion de atacar
				anim.SetTrigger (ATTACKING);
			}
		}
	}

	void OnBecameInvisible () {
		transform.position = new Vector3 (-7.5f, -4.45f, 0f);
	}

	public void enemyKnockBack (object[] message) {
		if (!death) {
			float enemyPosX = (float)message [0];
			int damage = (int)message [1];
			healthbar.SendMessage (TAKE_DAMAGE, damage);

			//jump = true;
			//float side = Mathf.Sign (enemyPosX - transform.position.x);
			//rb2d.AddForce (Vector2.left * side * jumpPower, ForceMode2D.Impulse);

			// Personaje herido
			Color colorHerido = new Color (255 / 255f, 105 / 255f, 105 / 255f);
			spr.color = colorHerido;

			//movement = false;

			HealthbarController hb = healthbar.GetComponent<HealthbarController> ();
			int vida = hb.hp;

			Invoke (ENABLE_MOVEMENT, 2f);

			if (vida < 1) {
				death = true;
				Invoke (PLAYER_DEATH, timeAnimationDeath);
			}
		}
		anim.SetBool (DEATH, death);
	}

	void playerDeath () {
		Destroy (gameObject);
        SceneManager.LoadScene (MENU_INICIO);// Game Over
	}

	void enableMovement() {
		movement = true;
		spr.color = Color.white;
	}

	void OnTriggerEnter2D (Collider2D col) {
		catchCrystal (col);
	}

	void OnTriggerStay2D (Collider2D col) {
		catchCrystal (col);
	}

	private void updateCrystals () {
		txt_crystals.GetComponent<TextMesh>().text = crystals + "/" + maxCrystals;
		if (crystals == maxCrystals) {
			HealthbarController hb = healthbar.GetComponent<HealthbarController> ();
			int vida = hb.hp;
			if (vida < 100) {
				txt_bonus.GetComponent<TextMesh>().text = "Presiona la tecla CTRL para sanarte por completo.";
			} else {
				txt_bonus.GetComponent<TextMesh>().text = "";
			}
		} else {
			txt_bonus.GetComponent<TextMesh>().text = "";
		}
	}

	private void catchCrystal (Collider2D col) {
		if (col.gameObject.tag == CRYSTAL) {
			if (crystals < maxCrystals) {
				Destroy (col.gameObject);
				crystals++;
			}
		}
	}

	private void consumeCrystals () {
		HealthbarController hb = healthbar.GetComponent<HealthbarController> ();
		int vida = hb.hp;
		if (crystals == maxCrystals && vida < 100) {
			crystals = 0;
			healthbar.SendMessage (TAKE_HEAL, 100);
		}
	}
}
