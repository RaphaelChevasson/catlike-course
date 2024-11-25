using UnityEngine;

public class GameLevel : PersistableObject {

	public static GameLevel Current { get; private set; }

	[SerializeField]
	SpawnZone spawnZone;

	[SerializeField]
	PersistableObject[] persistentObjects;

	//public void ConfigureSpawn(Shape shape) {
	public Shape SpawnShape () {
		return spawnZone.SpawnShape();
	}

	void OnEnable () {
		Current = this;
		if (persistentObjects == null) {
			persistentObjects = new PersistableObject[0];
		}
	}

	public override void Save (GameDataWriter writer) {
		writer.Write(persistentObjects.Length);
		for (int i = 0; i < persistentObjects.Length; i++) {
			persistentObjects[i].Save(writer);
		}
	}

	public override void Load (GameDataReader reader) {
		int savedCount = reader.ReadInt();
		for (int i = 0; i < savedCount; i++) {
			persistentObjects[i].Load(reader);
		}
	}
}