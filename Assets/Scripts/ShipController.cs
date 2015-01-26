using UnityEngine;
using System.Collections.Generic;

public class ShipController : MonoBehaviour {

	public struct ShipInput {
		public float leftRight;
		public float forward;
		public bool fire;
	};

	public GameObject bulletPrefab;

	public float fireSpeed;
	public float fireCooldown;
	public float bulletLifetime;
	private float currentFireCooldown = 0;

	private List<ParticleSystem> thrustParticles = new List<ParticleSystem>();
	private ParticleSystem shootParticles;

	public GameObject explosionEffectPrefab;

	public List<AudioClip> explosionSounds;
	public List<AudioClip> shootSounds;

	// Use this for initialization
	void Start () {
		foreach (ParticleSystem particles in GetComponentsInChildren<ParticleSystem>()) {
			if(particles.gameObject.name == "Thrust") {
				thrustParticles.Add(particles);
			} else {
				shootParticles = particles;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	}

	void FixedUpdate() {
		currentFireCooldown -= Time.fixedDeltaTime;
	}

	void OnCollisionEnter2D(Collision2D other) {
		Destroy (gameObject);

		var obj = (GameObject)Instantiate (explosionEffectPrefab, transform.position, new Quaternion ());
		Destroy (obj, 5.0f);

		obj.audio.enabled = true;
		obj.audio.clip = Utilities.ChooseRandom(explosionSounds);
		obj.audio.Play();

		if (other.gameObject.tag == "Asteroid") {
			other.gameObject.GetComponent<AsteroidController>().DestroyAsteroid();
		}
	}

	public void ApplyInput(ShipInput input)
	{
		transform.RotateAround(transform.position, transform.forward, -180 * Time.fixedDeltaTime * input.leftRight);

		if (input.fire && currentFireCooldown <= 0) {
			float shipRadius = ((CircleCollider2D)collider2D).radius;
			GameObject newBullet = 
				(GameObject)Instantiate(bulletPrefab, transform.position + transform.up * shipRadius * 1.8f, transform.rotation);

			Vector2 currentVelocity = rigidbody2D.velocity;
			Vector2 fireVelocity = transform.up * fireSpeed;
			newBullet.rigidbody2D.velocity = currentVelocity + fireVelocity;
			newBullet.GetComponent<BulletController>().lifetime = bulletLifetime;

			currentFireCooldown = fireCooldown;
			shootParticles.Play();

			newBullet.audio.enabled = true;
			newBullet.audio.clip = Utilities.ChooseRandom(shootSounds);
			newBullet.audio.Play();
		}

		if (input.forward > 0) {
			rigidbody2D.AddForce (transform.up * input.forward * 500);
			foreach (ParticleSystem particles in thrustParticles) {
				particles.Play ();
			}
		} else {
			foreach (ParticleSystem particles in thrustParticles) {
				particles.Stop ();
			}
		}
	}
}
