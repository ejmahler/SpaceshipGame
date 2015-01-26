using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(GameController))]
public class UIManager : MonoBehaviour {

	private GameController gameController;

	public Text pauseText;
	public Text placementErrorText;
	
	public Text remainingAsteroidText;
	public Text remainingEnemyText;

	public Text tutorialText;
	public Text tutorialArrowText;

	public Button spawnFastButton;
	public Button spawnhomingButton;

	// Use this for initialization
	void Start () {
		gameController = GetComponent<GameController> ();
	}
	
	// Update is called once per frame
	void Update () {
		remainingEnemyText.text = gameController.remainingSpacecraft.ToString ();
		remainingAsteroidText.text = gameController.remainingAsteroids.ToString ();
		pauseText.gameObject.SetActive (gameController.paused);
		placementErrorText.gameObject.SetActive (gameController.placementError);

		if (gameController.userStatus == GameController.UserStatus.Normal && gameController.remainingAsteroids > 0 && gameController.currentPlacementCooldown < 0) {
			spawnFastButton.interactable = true;
			spawnhomingButton.interactable = true;
		} else {
			spawnFastButton.interactable = false;
			spawnhomingButton.interactable = false;
		}

		tutorialArrowText.enabled = (gameController.tutorialStatus == GameController.TutorialStatus.ChooseType);
		tutorialText.enabled = (gameController.tutorialStatus != GameController.TutorialStatus.Completed);

		if (gameController.tutorialStatus == GameController.TutorialStatus.ChooseType) {
			tutorialText.text = "Click one of the buttons to the right to choose an asteroid type";
		} else if (gameController.tutorialStatus == GameController.TutorialStatus.Place) {
			tutorialText.text = "Click somewhere to place the asteroid";
		} else if (gameController.tutorialStatus == GameController.TutorialStatus.Orient) {
			tutorialText.text = "Click one more time somewhere else to aim and launch the asteroid. The enemy ship might be a good target!";
		}
	}
}
