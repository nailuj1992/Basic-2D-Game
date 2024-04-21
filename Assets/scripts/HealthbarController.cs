using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthbarController : MonoBehaviour {

	private int maxHp = 100;
	private int minHp = 0;
	public int hp;

	private Animator anim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();

		hp = maxHp;
		setHp (hp);
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void takeDamage (int amount) {
		hp = Mathf.Clamp (hp - amount, minHp, maxHp);
		setHp (hp);
	}

	public void takeHeal (int amount) {
		hp = Mathf.Clamp (hp + amount, minHp, maxHp);
		setHp (hp);
	}

	private void setHp (int hp) {
		anim.SetInteger ("vida", hp);
	}
}
