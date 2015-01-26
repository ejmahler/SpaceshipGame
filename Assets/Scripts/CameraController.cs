using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	BoxCollider2D boxCollider;

	void Start() {
		boxCollider = (BoxCollider2D)collider2D;
	}

	void OnTriggerExit2D(Collider2D other)
	{
		CorrectPosition ((CircleCollider2D)other);
	}

	void Update() {
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Asteroid")) {
			CorrectPosition ((CircleCollider2D)obj.collider2D);
		}
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Bullet")) {
			CorrectPosition ((CircleCollider2D)obj.collider2D);
		}
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Spacecraft")) {
			CorrectPosition ((CircleCollider2D)obj.collider2D);
		}
	}

	private void CorrectPosition(CircleCollider2D obj) {
		Vector3 otherPosition = obj.transform.position;
		if (otherPosition.x - obj.radius > boxCollider.size.x/2) {
			obj.transform.position -= new Vector3(boxCollider.size.x + obj.radius*2, 0, 0);
		}
		if (otherPosition.y - obj.radius > boxCollider.size.y/2) {
			obj.transform.position -= new Vector3(0, boxCollider.size.y + obj.radius*2, 0);
		}
		if (otherPosition.x + obj.radius < -boxCollider.size.x/2) {
			obj.transform.position += new Vector3(boxCollider.size.x + obj.radius*2, 0, 0);
		}
		if (otherPosition.y + obj.radius < -boxCollider.size.y/2) {
			obj.transform.position += new Vector3(0, boxCollider.size.y + obj.radius*2, 0);
		}
	}
}
