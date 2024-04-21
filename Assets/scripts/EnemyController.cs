using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour {

	private string JUGADOR = "Jugador";
	private string DEFAULT = "Default";
    private string MENU_INICIO = "menuInicio";

    private string ENABLE_MOVEMENT = "enableMovement";

    private string TAKE_DAMAGE = "takeDamage";
    private string ENEMY_DEATH = "enemyDeath";

    private string WALKING = "walking";
    private string ATTACKING = "attacking";
    private string VIDA = "vida";

	// Variables para gestionar el radio de visión, el de ataque y la velocidad
	public float visionRadius;
	public float attackRadius;
	public float maxSpeed = 1f;
	public float speed = 0f;

	// Variable para guardar al jugador
	private GameObject player;

	// Variable para guardar la posición inicial
	private Vector3 initialPosition, target;

	private Rigidbody2D rb2d;
	private Animator anim;
	private SpriteRenderer spr;

	public Vector2 scale;

	public GameObject healthbar;
	public int damage;

	private bool pulled = false;
	public GameObject hand;
	public GameObject bulletPrefab;
	public float attackSpeed;
	private bool attacking;

	public float timeAnimationDeath;

	public bool finalBoss;

	void Awake () {
		Assert.IsNotNull (healthbar);
		Assert.IsNotNull (hand);
		Assert.IsNotNull (bulletPrefab);
		Assert.AreNotApproximatelyEqual (0.0f, Mathf.Abs (timeAnimationDeath));
	}

	// Use this for initialization
	void Start () {
		// Recuperamos al jugador gracias al Tag
		player = GameObject.FindGameObjectWithTag (JUGADOR);

		// Guardamos nuestra posición inicial
		initialPosition = transform.position;

		rb2d = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		spr = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		// Por defecto nuestro target siempre será nuestra posición inicial
		target = initialPosition;

		// Comprobamos un Raycast del enemigo hasta el jugador
		float vision;
		if (pulled) {
			vision = Vector3.Distance(player.transform.position, transform.position);
		} else {
			vision = visionRadius;
		}

		RaycastHit2D hit = Physics2D.Raycast (
			transform.position, 
			player.transform.position - transform.position, 
			vision, 
			1 << LayerMask.NameToLayer (DEFAULT) 
			// Poner el propio Enemy en una layer distinta a Default para evitar el raycast
			// También poner al objeto Attack y al Prefab Slash una Layer Attack 
			// Sino los detectará como entorno y se mueve atrás al hacer ataques
		);

		// Aquí podemos debugear el Raycast
		Vector3 forward = transform.TransformDirection (player.transform.position - transform.position);
		Debug.DrawRay (transform.position, forward, Color.red);

		// Si el Raycast encuentra al jugador lo ponemos de target
		if (hit.collider != null) {
			if (hit.collider.tag == JUGADOR) {
				target = player.transform.position;
			}
		}

		// Calculamos la distancia y dirección actual hasta el target
		float distance = Vector3.Distance(target, transform.position);

		bool walking = false;
		// Si es el enemigo y está en rango de ataque nos paramos y le atacamos
		if (target != initialPosition && distance < attackRadius) {
			// Aquí le atacamos al jugador
			walking = false;
			pulled = false;
			enemyShooting ();
		} else {// En caso contrario nos movemos hacia él
			walking = move(target);
		}

		// Una última comprobación para evitar bugs forzando la posición inicial
		if (target == initialPosition && distance < 0.02f){
			transform.position = initialPosition; 
			// Y cambiamos la animación de nuevo a Idle
			walking = false;
		}

		anim.SetBool (WALKING, walking);

		// Y un debug optativo con una línea hasta el target
		Debug.DrawLine(transform.position, target, Color.green);
	}

	// Podemos dibujar el radio de visión y ataque sobre la escena dibujando una esfera
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (transform.position, visionRadius);
		Gizmos.DrawWireSphere (transform.position, attackRadius);
	}

	public void playerKnockBack (int damage) {
		healthbar.SendMessage (TAKE_DAMAGE, damage);

		// Personaje herido
		Color colorHerido = new Color (255 / 255f, 105 / 255f, 105 / 255f);
		spr.color = colorHerido;

		HealthbarController hb = healthbar.GetComponent<HealthbarController> ();
		int vida = hb.hp;
		anim.SetInteger (VIDA, vida);

		Invoke (ENABLE_MOVEMENT, 2f);

		if (vida < 1) {
			Invoke (ENEMY_DEATH, Mathf.Abs (timeAnimationDeath));
			if (finalBoss) {
                SceneManager.LoadScene (MENU_INICIO);// Juego Ganado
			}
		} else {
			Vector3 target = player.transform.position;
			float distance = Vector3.Distance(target, transform.position);
			bool walking = false;

			if (distance > visionRadius) {
				walking = move (target);
				pulled = true;
			}

			anim.SetBool (WALKING, walking);
		}
	}

	private bool move (Vector3 target) {
		Vector3 dir = (target - transform.position).normalized;

		rb2d.MovePosition(transform.position + dir * speed * Time.deltaTime);
		//rb2d.AddForce (Vector2.right * speed);
		float limitedSpeed = Mathf.Clamp (rb2d.velocity.x, -maxSpeed, maxSpeed);
		rb2d.velocity = new Vector2 (limitedSpeed, rb2d.velocity.y);

		// Al movernos establecemos la animación de movimiento
		if (dir.x > 0) {
			transform.localScale = new Vector3 (Mathf.Abs (scale.x), Mathf.Abs (scale.y), 1);
		}
		if (dir.x < 0) {
			transform.localScale = new Vector3 (-Mathf.Abs (scale.x), Mathf.Abs (scale.y), 1);
		}

		bool walking = true;
		return walking;
	}

	void enableMovement() {
		spr.color = Color.white;
	}

	void enemyDeath () {
		Destroy (gameObject);
	}

	public void enemyShooting () {
		if (!attacking) {
			StartCoroutine (attack (attackSpeed));
		}
	}

	IEnumerator attack (float seconds) {
		attacking = true;
		if (target != initialPosition && bulletPrefab != null) {
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

			yield return new WaitForSeconds (seconds);
		}
		attacking = false;
	}
}
