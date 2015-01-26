using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour {

	private bool spawningSpacecraft = false;
	private bool spawningAsteroid = false;

	public GameObject spacecraftPrefab, asteroidPrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (GameObject.FindGameObjectsWithTag ("Spacecraft").Length == 0 && !spawningSpacecraft) {
			Invoke ("SpawnSpacecraft", 1.0f);
			spawningSpacecraft = true;
		}

		if (GameObject.FindGameObjectsWithTag ("Asteroid").Length < 4 && !spawningAsteroid) {
			Invoke ("SpawnAsteroid", 1.0f);
			spawningAsteroid = true;
		}

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Quit ();
		}
	}

	public void BeginGame() {
		Application.LoadLevel ("MainScene");
	}

	private void SpawnSpacecraft() {
		Instantiate (spacecraftPrefab);
		spawningSpacecraft = false;
	}

	private void SpawnAsteroid() {
		Vector3 asteroidPosition = new Vector3 (Random.Range (-500.0f, 500.0f), Random.Range (-300.0f, 300.0f), 0.0f);
		Quaternion asteroidRotation = Quaternion.AngleAxis(Random.Range (0, 360), Vector3.forward);
		Instantiate (asteroidPrefab, asteroidPosition, asteroidRotation);
		spawningAsteroid = false;
	}

	public void Quit() {
		Application.Quit ();
	}
}
