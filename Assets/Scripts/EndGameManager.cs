using UnityEngine;
using System.Collections;

public class EndGameManager : MonoBehaviour {

	void Start() {
	}

	public void GoToMainMenu() {
		foreach (GameObject asteroid in GameObject.FindGameObjectsWithTag("Asteroid")) {
			Destroy(asteroid);
		}
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Spacecraft")) {
			Destroy(obj);
		}
		Application.LoadLevel ("MenuScene");
	}

	public void Quit() {
		Application.Quit ();
	}
}
