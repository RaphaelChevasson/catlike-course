using UnityEngine;

public class BulletVisualization : MonoBehaviour
{
	static int lifeFactorID = Shader.PropertyToID("_LifeFactor");

	static MaterialPropertyBlock materialPropertyBlock;

	PrefabInstancePool<BulletVisualization> pool;

	MeshRenderer meshRenderer;

	void Awake()
	{
		materialPropertyBlock ??= new MaterialPropertyBlock();
		meshRenderer = GetComponent<MeshRenderer>();
	}
	
	public BulletVisualization Spawn(Quaternion rotation)
	{
		BulletVisualization instance = pool.GetInstance(this);
		instance.pool = pool;
		instance.transform.localRotation = rotation;
		return instance;
	}

	public void Despawn() => pool.Recycle(this);

	public void UpdateVisualization(Vector2 position, float lifeFactor)
	{
		transform.localPosition = position;
		materialPropertyBlock.SetFloat(lifeFactorID, lifeFactor);
		meshRenderer.SetPropertyBlock(materialPropertyBlock);
	}
}
