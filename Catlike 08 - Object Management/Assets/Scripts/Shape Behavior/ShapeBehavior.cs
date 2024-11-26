using UnityEngine;

public abstract class ShapeBehavior
#if UNITY_EDITOR
	: ScriptableObject
#endif
{
	public abstract ShapeBehaviorType BehaviorType { get; }

	public abstract void GameUpdate (Shape shape);

	public abstract void Save (GameDataWriter writer);

	public abstract void Load (GameDataReader reader);

	public abstract void Recycle ();

#if UNITY_EDITOR
	public bool IsReclaimed { get; set; }

	void OnEnable () {
		if (IsReclaimed) {
			Recycle();
		}
	}
#endif
}