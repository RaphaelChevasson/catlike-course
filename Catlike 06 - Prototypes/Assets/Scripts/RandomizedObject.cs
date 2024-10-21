using UnityEngine;

public class RandomizedObject : SkylineObject
{
	[SerializeField]
	GameObject[] items;

	[SerializeField]
	float spawnProbability = 0.5f;

	void OnEnable ()
	{
		for (int i = 0; i < items.Length; i++)
		{
			items[i].SetActive(Random.value < spawnProbability);
		}
	}
}
