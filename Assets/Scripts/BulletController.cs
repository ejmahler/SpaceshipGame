using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour {

	public float lifetime { get; set; }

	public GameObject expirationPrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		lifetime -= Time.fixedDeltaTime;
		if (lifetime < 0) {
			Destroy (gameObject);
			var obj = (GameObject)Instantiate (expirationPrefab, transform.position, new Quaternion());
			obj.rigidbody2D.velocity = rigidbody2D.velocity;
			Destroy (obj, 5.0f);
		}
	}

	void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.tag == "Bullet") {
			Destroy (gameObject);
			Destroy (other.gameObject);
		}
	}
}
