using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour {

	private Camera mainCamera;

	public enum TutorialStatus { ChooseType, Place, Orient, Completed };
	public enum UserStatus { Normal, Placing, Orienting };

	public TutorialStatus tutorialStatus { get; private set; }
	public UserStatus userStatus { get; private set; }

	public bool paused { get; private set; }
	public bool placementError { get; private set; }

	private bool spawningSpacecraft = false;
	private AsteroidController.AsteroidVariety placingAsteroidVariety;
	private Vector2 placementPosition;

	public GameObject spacecraftPrefab;
	public GameObject asteroidPrefab;

	public int remainingAsteroids = 10;
	public int remainingSpacecraft = 2;

	public float asteroidPlacementCooldown = 10.0f;
	public float currentPlacementCooldown { get; private set; }

	void Start () {
		foreach (GameObject asteroid in GameObject.FindGameObjectsWithTag("Asteroid")) {
			asteroid.transform.rotation = Quaternion.AngleAxis(Random.Range (0, 360), transform.forward);
		}
		paused = false;
		placementError = false;
		tutorialStatus = TutorialStatus.ChooseType;
		userStatus = UserStatus.Normal;

		mainCamera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera>();
		currentPlacementCooldown = 0.0f;
	}

	void Update() {
		currentPlacementCooldown -= Time.deltaTime;

		if (Input.GetKeyDown (KeyCode.Space)) {
			Pause ();
		}
		if (Input.GetKeyDown (KeyCode.Q)) {
			SpawnFastAsteroid();
		}
		if (Input.GetKeyDown (KeyCode.W)) {
			SpawnHomingAsteroid();
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Quit ();
		}

		if (GameObject.FindGameObjectsWithTag ("Spacecraft").Length == 0 && !spawningSpacecraft) {
			if(remainingSpacecraft > 0) { 
				StartCoroutine(SpawnSpacecraft());
				remainingSpacecraft -= 1;
			} else {
				AsteroidsWin ();
			}
		}

		if (GameObject.FindGameObjectsWithTag ("Asteroid").Length == 0) {
			AsteroidsLose ();
		}

		if (Input.GetMouseButtonDown (0) && userStatus == UserStatus.Orienting) {
			userStatus = UserStatus.Normal;

			if(IsValidAsteroidPlacement(placementPosition)) {
				Vector2 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
				
				Vector2 placementDirection = (worldPosition - placementPosition).normalized;

				var newAsteroid = (GameObject)Instantiate(
					asteroidPrefab,
					placementPosition,
					Quaternion.FromToRotation(Vector3.up, placementDirection)
					);
				var newAsteroidController = newAsteroid.GetComponent<AsteroidController>();

				newAsteroidController.variety = placingAsteroidVariety;

				remainingAsteroids--;
				currentPlacementCooldown = asteroidPlacementCooldown;

				if(tutorialStatus != TutorialStatus.Completed) {
					tutorialStatus = TutorialStatus.Completed;
				}
			} else {
				StartCoroutine(PlacementError());

				if(tutorialStatus != TutorialStatus.Completed) {
					tutorialStatus = TutorialStatus.ChooseType;
				}
			}
		}

		if (Input.GetMouseButtonDown (0) && userStatus == UserStatus.Placing) {
			placementPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
			if(IsValidAsteroidPlacement(placementPosition)) {
				userStatus = UserStatus.Orienting;

				if(tutorialStatus != TutorialStatus.Completed) {
					tutorialStatus = TutorialStatus.Orient;
				}
			} else {
				StartCoroutine(PlacementError());
				userStatus = UserStatus.Normal;

				if(tutorialStatus != TutorialStatus.Completed) {
					tutorialStatus = TutorialStatus.ChooseType;
				}
			}
		}
	}

	public void AsteroidsWin() {
		foreach (GameObject asteroid in GameObject.FindGameObjectsWithTag("Asteroid")) {
			DontDestroyOnLoad(asteroid);
		}
		Application.LoadLevel ("VictoryScene");
	}

	public void AsteroidsLose() {
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Spacecraft")) {
			DontDestroyOnLoad(obj);
		}
		Application.LoadLevel ("DefeatScene");
	}

	public void Pause() {
		paused = !paused;
		Time.timeScale = 1.0f - Time.timeScale;
	}

	private IEnumerator SpawnSpacecraft() {
		spawningSpacecraft = true;

		yield return new WaitForSeconds (1.0f);

		bool placed = false;
		while (!placed) {

			//make 100 attempts this frame
			for(int i = 0; i < 100; i++) {
				Vector2 position = new Vector2(Random.Range(-500.0f, 500.0f), Random.Range(-300.0f, 300.0f));
				if(IsValidSpacecraftPlacement(position)) {
					spawningSpacecraft = false;
					Instantiate (spacecraftPrefab, position, new Quaternion());
					placed = true;
					break;
				}
			}
			yield return null;
		}
	}

	public void SpawnFastAsteroid() {
		if (remainingAsteroids > 0 && currentPlacementCooldown < 0) {
			placingAsteroidVariety = AsteroidController.AsteroidVariety.Fast;
			userStatus = UserStatus.Placing;

			if(tutorialStatus != TutorialStatus.Completed) {
				tutorialStatus = TutorialStatus.Place;
			}
		}
	}

	public void SpawnHomingAsteroid() {
		if (remainingAsteroids > 0 && currentPlacementCooldown < 0) {
			placingAsteroidVariety = AsteroidController.AsteroidVariety.Homing;
			userStatus = UserStatus.Placing;

			if(tutorialStatus != TutorialStatus.Completed) {
				tutorialStatus = TutorialStatus.Place;
			}
		}
	}

	private bool IsValidAsteroidPlacement(Vector2 position) {
		var results = Physics2D.CircleCastAll (position, 50.0f, new Vector2 ());
		
		foreach (var result in results) {
			if(result.collider.gameObject.tag == "Spacecraft") {
				return false;
			}
		}
		return true;
	}

	private bool IsValidSpacecraftPlacement(Vector2 position) {
		var results = Physics2D.CircleCastAll (position, 50.0f, new Vector2 ());
		
		foreach (var result in results) {
			if(result.collider.gameObject.tag == "Asteroid") {
				return false;
			}
		}
		return true;
	}

	private IEnumerator PlacementError() {
		for(int i = 0; i < 3; i++) {
			placementError = true;
			yield return new WaitForSeconds(0.7f);
			placementError = false;
			yield return new WaitForSeconds(0.3f);
		}
	}

	public void Quit() {
		Application.Quit ();
	}
}
