using System.Collections.Generic;
using UnityEngine;

public class Game : PersistableObject
{
	public ShapeFactory shapeFactory;
    [SerializeField] private KeyCode createKey = KeyCode.C;
    [SerializeField] private KeyCode newGameKey = KeyCode.N;
    [SerializeField] private KeyCode saveKey = KeyCode.S;
    [SerializeField] private KeyCode loadKey = KeyCode.L;
    [SerializeField] private PersistentStorage storage;

    private const int saveVersion = 1;
    private List<Shape> shapes;
    private string savePath;

    private void Awake()
    {
		shapes = new List<Shape>();
        
	}

    private void Update()
    {
		if(Input.GetKeyDown(createKey))
        {
			CreateShape();
		}
        else if(Input.GetKey(newGameKey))
        {
			BeginNewGame();
		}
        else if(Input.GetKeyDown(saveKey))
        {
            storage.Save(this);
		}
        else if(Input.GetKeyDown(loadKey))
        {
            BeginNewGame();
			storage.Load(this);
		}
	}

    private void BeginNewGame()
    {
        for(int i = 0; i < shapes.Count; i++)
        {
			Destroy(shapes[i].gameObject);
		}
        shapes.Clear();
    }

    private void CreateShape()
    {
		Shape instance = shapeFactory.GetRandom();
		Transform t = instance.transform;
		t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        shapes.Add(instance);
	}

    public override void Save(GameDataWriter writer)
    {
        writer.Write(-saveVersion);
		writer.Write(shapes.Count);
		for(int i = 0; i < shapes.Count; i++)
        {
            writer.Write(shapes[i].ShapeId);
			shapes[i].Save(writer);
		}
	}

    public override void Load(GameDataReader reader)
    {
        int version = -reader.ReadInt();
        if(version > saveVersion)
        {
			Debug.LogError("Unsupported future save version " + version);
			return;
		}
		int count = version <= 0 ? -version : reader.ReadInt();
		for(int i = 0; i < count; i++)
        {
            int shapeId = version > 0 ? reader.ReadInt() : 0;
			Shape instance = shapeFactory.Get(shapeId);
			instance.Load(reader);
			shapes.Add(instance);
		}
	}
}
