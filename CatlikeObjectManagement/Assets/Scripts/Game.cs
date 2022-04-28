using System.Collections.Generic;
using UnityEngine;

public class Game : PersistableObject
{
	public ShapeFactory shapeFactory;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }

    [SerializeField] private KeyCode createKey = KeyCode.C;
    [SerializeField] private KeyCode destroyKey = KeyCode.X;
    [SerializeField] private KeyCode newGameKey = KeyCode.N;
    [SerializeField] private KeyCode saveKey = KeyCode.S;
    [SerializeField] private KeyCode loadKey = KeyCode.L;
    [SerializeField] private PersistentStorage storage;

    private const int saveVersion = 1;
    private List<Shape> shapes;
    private string savePath;
    private float creationProgress, destructionProgress;

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
        else if(Input.GetKeyDown(destroyKey))
        {
			DestroyShape();
		}
        else if(Input.GetKey(newGameKey))
        {
			BeginNewGame();
		}
        else if(Input.GetKeyDown(saveKey))
        {
            storage.Save(this, saveVersion);
		}
        else if(Input.GetKeyDown(loadKey))
        {
            BeginNewGame();
			storage.Load(this);
		}

        creationProgress += Time.deltaTime * CreationSpeed;
        while(creationProgress >= 1f)
        {
			creationProgress -= 1f;
			CreateShape();
		}

        destructionProgress += Time.deltaTime * DestructionSpeed;
		while(destructionProgress >= 1f)
        {
			destructionProgress -= 1f;
			DestroyShape();
		}
	}

    private void BeginNewGame()
    {
        for(int i = 0; i < shapes.Count; i++)
        {
			shapeFactory.Reclaim(shapes[i]);
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
        instance.SetColor(Random.ColorHSV(
			hueMin: 0f, hueMax: 1f,
			saturationMin: 0.5f, saturationMax: 1f,
			valueMin: 0.25f, valueMax: 1f,
			alphaMin: 1f, alphaMax: 1f
		));
        shapes.Add(instance);
	}

    void DestroyShape()
    {
		if(shapes.Count > 0)
        {
			int index = Random.Range(0, shapes.Count);
			shapeFactory.Reclaim(shapes[index]);
            int lastIndex = shapes.Count - 1;
			shapes[index] = shapes[lastIndex];
			shapes.RemoveAt(lastIndex);
		}
	}

    public override void Save(GameDataWriter writer)
    {
		writer.Write(shapes.Count);
		for(int i = 0; i < shapes.Count; i++)
        {
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
			shapes[i].Save(writer);
		}
	}

    public override void Load(GameDataReader reader)
    {
        int version = reader.Version;
        if(version > saveVersion)
        {
			Debug.LogError("Unsupported future save version " + version);
			return;
		}
		int count = version <= 0 ? -version : reader.ReadInt();
		for(int i = 0; i < count; i++)
        {
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
			Shape instance = shapeFactory.Get(shapeId, materialId);
			instance.Load(reader);
			shapes.Add(instance);
		}
	}
}
