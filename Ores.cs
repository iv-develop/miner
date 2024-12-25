using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class Ores : Node
{
	const string OrePlaceholderPath = "res://ores/notexture.png";


	public class OreTexture {
		public static Godot.Texture2D GetPlaceholderTexture() {
			return ResourceLoader.Load<Godot.Texture2D>(OrePlaceholderPath);
		}

		// May be overridern.
		public Godot.Texture2D GetTexture() {
			return ResourceLoader.Load<Godot.Texture2D>(TexturePath) ?? OreTexture.GetPlaceholderTexture();
		}

		// Defines how many texture variants the ore has. Texture layout: vertical.
		// Texture layout: vertical.
		public int Variants {get; private set;} = 1;

		// Path to the texture folder. Example: "res://ores/gold".
		// Names: "res://ores/texture/0.png", "res://ores/texture/1.png"
		public string TexturePath {get;} = "res://ores/name";

		public OreTexture(string TexturePath, int Variants) {
			this.TexturePath = TexturePath;
			this.Variants = Variants;
		}
	}

	abstract class Ore {
		protected static readonly OreTexture DefaultTexture = new OreTexture("res://ores/notexture", 1);
		private OreTexture _texture;
		protected virtual OreTexture Texture => _texture ??= GetTexturePath();
		protected virtual OreTexture GetTexturePath() => DefaultTexture;

		public Godot.Texture2D GetTexture() {
			return Texture.GetTexture() ?? OreTexture.GetPlaceholderTexture();
		}

		public int Variants {get {return Texture.Variants;}}

		static Random rand = new Random();

		// Returns a list of RELATIVE coordinates of ore cluster elements
		public virtual Godot.Collections.Array<Vector2I> GenerateCluster() {
			var r = rand.NextDouble();
			var number_of_ores = (int)(r * 7.0) + 1;
			var positions = new Godot.Collections.Array<Vector2I>();
			for (int i = 0; i < number_of_ores; i++) {
				var x = rand.NextDouble();
				var y = rand.NextDouble();
				positions.Add(new Vector2I((int)(x * 4), (int)(y * 4)));
			}
			return positions;
		}
		
		// Cluster spawn chance. N per tile
		abstract public double SpawnChance {get;}

		public void Print() {
			GD.Print("\nOre: " + this.GetType().Name);
			GD.Print("SpawnChance: " + SpawnChance);
			GD.Print("Texture: " + Texture.TexturePath);
		}
	}

	static Random rand = new Random();
	static List<Ore> OresToSpawn = new List<Ore>();
	
	public override void _Ready()
	{
		base._Ready();
		Assembly.GetExecutingAssembly().GetTypes().AsEnumerable().Where(t => t.IsSubclassOf(typeof(Ore))).ToList().ForEach(t => {
			var instance = (Ore)Activator.CreateInstance(t);
			OresToSpawn.Add(instance);
		});
	}
	public static Godot.Collections.Array<Godot.Collections.Array> TickTile() {
		var ores = new Godot.Collections.Array<Godot.Collections.Array>();
		for (int i = 0; i < OresToSpawn.Count; i++) {
			var ore = OresToSpawn[i];
			var r = rand.NextDouble();
			if (r < ore.SpawnChance) {
				foreach (var pos in ore.GenerateCluster()) {
					var array = new Godot.Collections.Array();
					array.Add(i);
					array.Add(ore.Variants);
					array.Add(pos);
					ores.Add(array);
				}
			}
		}
		return ores;
	}

	public static Godot.TileSet GenerateOreTileset() {
		var tiles = new Godot.TileSet();
		tiles.AddTerrainSet();
		tiles.AddTerrain(0);

		for (int i = 0; i < OresToSpawn.Count; i++) {
			var ore = OresToSpawn[i];
			var source = new TileSetAtlasSource();
			Godot.Texture2D texture = ore.GetTexture();
			source.Texture = texture;
			for (var variant = 0; variant < ore.Variants; variant += 1){
				source.CreateTile(new Godot.Vector2I(variant, 0));
			}
			tiles.AddSource(source, i);
		}
		return tiles;
	}


	public static void Print() {
		GD.Print("Itited ores:");
		foreach (var ore in OresToSpawn) {
			ore.Print();
			GD.Print(ore.GenerateCluster());
		}
	}






	class Gold : Ore {
		private static readonly OreTexture TexturePath = new OreTexture("res://ores/gold.png", 1);
		protected override OreTexture GetTexturePath() => TexturePath;
		public override double SpawnChance => 0.009;
	}



	class Uranium : Ore {
		private static readonly OreTexture TexturePath = new OreTexture("res://ores/uranium.png", 3);
		protected override OreTexture GetTexturePath() => TexturePath;
		public override double SpawnChance => 0.001;
	}

	class Nephrite : Ore {
		private static readonly OreTexture TexturePath = new OreTexture("res://ores/gem.png", 3);
		protected override OreTexture GetTexturePath() => TexturePath;
		public override double SpawnChance => 0.001;

		// Custom cluster generation fn. Simple multi-instance random walker
		public override Godot.Collections.Array<Vector2I> GenerateCluster() {
			var r = rand.NextDouble();
			var number_of_walkers = 1 + (int)(r * 5.0);
			var ores = new Godot.Collections.Array<Vector2I>();

			var steps = new Godot.Collections.Array<Vector2I> {
				new Vector2I(1, 0),
				new Vector2I(-1, 0),
				new Vector2I(0, 1),
				new Vector2I(0, -1),
				new Vector2I(1, 1),
				new Vector2I(1, -1),
				new Vector2I(-1, 1),
				new Vector2I(-1, -1)
			};
			for (var w = 0; w < number_of_walkers; w++) {
				var number_of_ores = 5 + (int)(r * 10.0);
				var walker_pos = new Vector2I(0, 0);
				for (var step = 0; step < number_of_ores; step++) {
					walker_pos += steps.PickRandom();
					ores.Add(walker_pos);
				}
			}
			return ores;
		}
	}
	
	class MouseGem : Ore {
		private static readonly OreTexture TexturePath = new OreTexture("res://ores/mouse.png", 1);
		protected override OreTexture GetTexturePath() => TexturePath;
		public override double SpawnChance => 0.0001;

		// Override default generation fn. That returns single gem
		public override Godot.Collections.Array<Vector2I> GenerateCluster() {
			return new Godot.Collections.Array<Vector2I>{new Vector2I(0, 0)};
		}
	}	
}
