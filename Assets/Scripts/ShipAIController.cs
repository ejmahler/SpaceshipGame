using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ShipAIController : MonoBehaviour {

	private ShipController shipController;

	private GameObject currentTarget = null;

	// Use this for initialization
	void Start () {
		shipController = GetComponentInParent<ShipController> ();

		StartCoroutine (StatusEvade ());
	}

	private IEnumerator StatusShoot() {
		while (true) {
			if(CheckAsteroidCollisions()) {
				StartCoroutine(StatusEvade());
				break;
			} else {
				var input = ShootAtEnemy ();
				shipController.ApplyInput(input);
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator StatusEvade() {
		while (true) {
			var collisionObject = CheckAsteroidCollisions();
			if(collisionObject == null) {
				StartCoroutine(StatusStop());
				break;
			} else {
				var input = AvoidAsteroid (collisionObject);

				for(int i = 0; i < 10; i++) {
					shipController.ApplyInput(input);
					yield return new WaitForFixedUpdate();
				}
				continue;
			}
			//yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator StatusStop() {
		while (true) {
			if(shipController.rigidbody2D.velocity.magnitude < 10f) {
				StartCoroutine(StatusShoot());
				break;
			} else if(CheckAsteroidCollisions()) {
				StartCoroutine(StatusEvade());
				break;
			} else {
				var input = StopShip ();
				shipController.ApplyInput(input);
			}
			yield return new WaitForFixedUpdate();
		}
	}






	private ShipController.ShipInput ShootAtEnemy() {
		ShipController.ShipInput result = new ShipController.ShipInput();
		if (currentTarget == null) {
			var potentialTargets = GameObject.FindGameObjectsWithTag("Asteroid");
			var currentPosition = shipController.transform.position;
			
			if(potentialTargets.Count() > 0) {
				currentTarget = potentialTargets.OrderBy((asteroid) => {
					float multiplier;
					var size = asteroid.GetComponent<AsteroidController>().size;
					if(size == AsteroidController.AsteroidSize.Large) {
						multiplier = 1.0f;
					} else if(size == AsteroidController.AsteroidSize.Medium) {
						multiplier = 0.5f;
					} else {
						multiplier = 0.25f;
					} 
					return (currentPosition - asteroid.transform.position).sqrMagnitude * multiplier;
				}).First();
			}
		}
		
		if(currentTarget != null) {
			
			Vector3 hitPosition;
			if(PredictTargetPosition(currentTarget, out hitPosition)) {
				
				Vector2 hitDirection = (hitPosition - shipController.transform.position).normalized;
				float dot = Vector3.Dot(hitDirection, shipController.transform.up);

				if(dot > 0.999f) {
					result.fire = true;
				} else {
					if(Vector3.Cross(hitDirection, shipController.transform.up).z > 0) {
						result.leftRight = 1;
					} else {
						result.leftRight = -1;
					}
				}
			} else {
				currentTarget = null;
			}
		}
		return result;
	}

	private ShipController.ShipInput StopShip() {
		ShipController.ShipInput result = new ShipController.ShipInput();
		
		Vector2 away = -shipController.rigidbody2D.velocity.normalized;
		float dot = Vector3.Dot (away, shipController.transform.up);
		if(dot > 0.9f) {
			result.forward = 1;
		}

		if (dot < 0.99f)
		{
			if(Vector3.Cross(away, shipController.transform.up).z > 0) {
				result.leftRight = 1;
			} else {
				result.leftRight = -1;
			}
		}

		return result;
	}

	private ShipController.ShipInput AvoidAsteroid(GameObject asteroid) {
		ShipController.ShipInput result = new ShipController.ShipInput();

		Vector2 asteroidDirection = asteroid.rigidbody2D.velocity.normalized;
		Vector2 perpendicular = new Vector2 (asteroidDirection.y, -asteroidDirection.x);

		if (Vector3.Dot (perpendicular, shipController.transform.up) < 0) {
			perpendicular = -perpendicular;
		}

		float dot = Vector3.Dot (perpendicular, shipController.transform.up);
		if(dot > 0.7f) {
			result.forward = 1;
		}
		
		if (dot < 0.99f) {
			if(Vector3.Cross(perpendicular, shipController.transform.up).z > 0) {
				result.leftRight = 1;
			} else {
				result.leftRight = -1;
			}
		}

		return result;
	}

	private GameObject CheckAsteroidCollisions() {
		float soonestCollision = 999999.0f;
		GameObject soonestCollisionObject = null;

		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Asteroid")) {
			float collisionTime;
			if(PredictCollision(obj, out collisionTime)) {
				if(collisionTime < 5 && collisionTime < soonestCollision) {
					soonestCollisionObject = obj;
					soonestCollision = collisionTime;
				}
			}
		}
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Bullet")) {
			float collisionTime;
			if(PredictCollision(obj, out collisionTime)) {
				if(collisionTime < 5 && collisionTime < soonestCollision) {
					soonestCollisionObject = obj;
					soonestCollision = collisionTime;
				}
			}
		}
		return soonestCollisionObject;
	}

	private bool PredictCollision(GameObject other, out float collisionTime) {
		collisionTime = 0;

		//treat ourselves like a stationary circle, and 'other' like a ray
		Vector2 circleCenter = shipController.transform.position;
		Vector2 rayOrigin = other.transform.position;
		
		//subtract our velocity from the other velocity, and treat that as the ray direction
		Vector2 rayDirection = other.rigidbody2D.velocity - shipController.rigidbody2D.velocity;
		
		//subtract the circle center from the ray origin to act as if the sphere were centered at 0,0,0
		rayOrigin -= circleCenter;
		
		//the radius of our circle is our circle plus other's radius
		float circleRadius = ((CircleCollider2D)shipController.collider2D).radius * 2 + ((CircleCollider2D)other.collider2D).radius;

		//use the formula found here: http://wiki.cgsociety.org/index.php/Ray_Sphere_Intersection
		float a = Vector3.Dot(rayDirection, rayDirection);//this isn't normalized, so it isn't necessarily 1
		float b = 2 * Vector3.Dot(rayOrigin, rayDirection);
		float c = Vector3.Dot(rayOrigin, rayOrigin) - circleRadius*circleRadius;
		
		float discriminant = b*b - 4*a*c;
		
		//if the discriminant is less than 0, there is no intersection
		//if the discriminant is 0 then the ray is tangent to the sphere, so in the interest of less code
		//we just don't handle it - it will either be handled next frame when they get closer or it won't
		//because they've moved away
		if(discriminant<=0)
		{
			return false;
		}
		else
		{
			//find the two parametric points of contact with the sphere by finishing the quadratic equation
			float rootDiscriminant = (float)System.Math.Sqrt(discriminant);
			
			//there are 2 intersection points, find the t0 one first, we most likely don't need
			//to compute the t1 one
			float t0 = (-b - rootDiscriminant)/(2*a);
			
			//because rayDirection is in m/s and rayDirection * collisionTime gives the distance between
			//the ray origin and the collision point, t0 is the number of seconds until the collision
			
			//if this collision happens in the future
			if(t0 > 0)
			{
				collisionTime = t0;
				return true;
			}
			else
			{
				//the t0 collision happens in the past. if the t1 collision also happens in the
				//past then a collision is not imminent as both objects are oving away from it
				float t1 = (-b + rootDiscriminant)/(2*a);

				if(t1 > 0)
				{
					//the t1 happens in the future and the t0 intersection happens in the past,
					//so we are currently experiencing a collision
					collisionTime = t1;
					return true;
				}
				else
				{
					//if this collision also happens in the past, return false, a colision is not imminent
					return false;
				}
			}
		}
	}

	private bool PredictTargetPosition(GameObject target, out Vector3 position) {

		Vector3 targetPosition = target.transform.position - shipController.transform.position;
		Vector3 targetVelocity = target.rigidbody2D.velocity - shipController.rigidbody2D.velocity;
		float fireSpeed = shipController.fireSpeed;
		
		float a = Vector3.Dot(targetVelocity, targetVelocity) - fireSpeed * fireSpeed;
		float b = 2 * Vector3.Dot(targetVelocity, targetPosition);
		float c = Vector3.Dot(targetPosition, targetPosition);

		float determinant = b * b - 4 * a * c;
		if (determinant < 0) {
			position = new Vector3();
			return false;
		}
				
		float p = -b / (2 * a);
		float q = (float)System.Math.Sqrt(determinant) / (2 * a);
		
		float t1 = p - q;
		float t2 = p + q;

		float t;

		if (t1 < t2) {
			var temp = t2;
			t2 = t1;
			t1 = temp;
		}

		if (t1 < 0) {
			position = new Vector3 ();
			return false;
		} else if (t2 < 0) {
			t = t1;
		} else {
			t = t2;
		}

		if (t > shipController.bulletLifetime) {
			position = new Vector3 ();
			return false;
		}

		Vector3 targetDisplacement = t * target.rigidbody2D.velocity;
		position = target.transform.position + targetDisplacement;
		return true;
	}
}
