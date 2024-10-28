using UnityEngine;

public class HealthBar : MonoBehaviour
{
	Vector3 scale;

	float maxSize;

	void Awake()
	{
		scale = transform.localScale;
		maxSize = scale.x;
		transform.localScale = Vector3.zero;
	}

	public void Show(float healthPercentage)
	{
		scale.x = Mathf.Max(maxSize * healthPercentage, 0f);
		transform.localScale = scale;
	}
}
