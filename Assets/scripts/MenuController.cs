using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

	private string JUEGO = "juego";

	public void playGame () {
        SceneManager.LoadScene (JUEGO);
	}

	public void quitGame () {
		Application.Quit ();
	}
}
