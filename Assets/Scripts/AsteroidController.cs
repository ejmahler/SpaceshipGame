using UnityEngine;
using System.Collections.Generic;

public class AsteroidController : MonoBehaviour {

	public enum AsteroidSize { Large, Medium, Small };
	public enum AsteroidVariety { Standard, Fast, Homing };

	public AsteroidSize size;
	public AsteroidVariety variety;

	public GameObject smallerAsteroidPrefab;
	public GameObject explosionEffectPrefab;

	public Material standardMaterial;
	public Material fastMaterial;
	public Material homingMaterial;

	public List<AudioClip> explosionSounds;

	// Use this for initialization
	void Start () {
		var randomSpeed = Random.Range (50, 100);
		rigidbody2D.velocity = transform.up * randomSpeed;

		if (variety == AsteroidVariety.Standard) {
			GetComponentInChildren<MeshRenderer>().material = standardMaterial;
		} else if (variety == AsteroidVariety.Fast) {
			GetComponentInChildren<MeshRenderer>().material = fastMaterial;
		} else if (variety == AsteroidVariety.Homing) {
			GetComponentInChildren<MeshRenderer>().material = homingMaterial;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (variety == AsteroidVariety.Fast) {
			transform.rotation.SetLookRotation (rigidbody2D.velocity, Vector3.forward);
			if (rigidbody2D.velocity.magnitude < 250.0f) {
				rigidbody2D.AddForce (rigidbody2D.velocity.normalized, ForceMode2D.Impulse);
			}
		} else if (variety == AsteroidVariety.Homing) {
			var ship = GameObject.FindGameObjectWithTag("Spacecraft");
			if(ship != null) {
				var shipDirection = (ship.transform.position - transform.position).normalized;

				transform.rotation.SetLookRotation (rigidbody2D.velocity, Vector3.forward);

				rigidbody2D.AddForce (shipDirection * 0.5f, ForceMode2D.Impulse);
				float maxVelocity = 75.0f;
				if(rigidbody2D.velocity.magnitude > maxVelocity) {
					rigidbody2D.velocity = rigidbody2D.velocity.normalized * maxVelocity;
				}
			}
		}
	}

	void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.tag == "Bullet") {
			Destroy (other.gameObject);

			DestroyAsteroid ();
		}
	}

	public void DestroyAsteroid() {
		Destroy (gameObject);
		
		var obj = (GameObject)Instantiate(explosionEffectPrefab, transform.position, new Quaternion());
		Destroy (obj, 5.0f);
		
		obj.audio.enabled = true;
		obj.audio.clip = Utilities.ChooseRandom(explosionSounds);
		obj.audio.Play();
		
		
		if(smallerAsteroidPrefab != null) {
			var rotateLeft = Quaternion.AngleAxis(45 + Random.Range (-20, 20), Vector3.forward);
			var rotateRight = Quaternion.AngleAxis(-45 + Random.Range (-20, 20), Vector3.forward);
			
			var forwardLeftDirection = rotateLeft * transform.rotation;
			var forwardRightDirection = rotateRight * transform.rotation;
			
			GameObject forwardLeft = (GameObject)Instantiate (
				smallerAsteroidPrefab, 
				transform.position + rotateLeft * transform.up * 10,
				forwardLeftDirection);
			GameObject forwardRight = (GameObject)Instantiate (
				smallerAsteroidPrefab,
				transform.position + rotateRight * transform.up * 10,
				forwardRightDirection);
			
			forwardLeft.GetComponent<AsteroidController>().variety = variety;
			forwardRight.GetComponent<AsteroidController>().variety = variety;
			
		}
	}
}
