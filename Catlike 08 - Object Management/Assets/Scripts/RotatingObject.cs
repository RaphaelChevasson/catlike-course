using UnityEngine;

public class RotatingObject : PersistableObject {

	[SerializeField]
	Vector3 angularVelocity;

	void FixedUpdate () {
		transform.Rotate(angularVelocity * Time.deltaTime);
	}
}