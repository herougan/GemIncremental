using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence.Map
{
	[Serializable]
	public struct TileColorEntry
	{
		public TileType Type;
		public Color Color;
	}

	/// <summary>
	/// Three independent jobs, merged into one class because they're always used back-to-back (load, then
	/// build, then path): loading map data into an in-memory Map grid (static, no GameObjects involved -
	/// LoadMap(MapPlan)/LoadMap(string)), building the 2D visual tiles for a loaded Map in gamespace
	/// (instance, since it needs a Transform to parent instances under and a per-map prefab palette), and
	/// resolving that Map's Source(s)/Destination(s) into world-space routes (BuildPaths). Map itself stays
	/// completely ignorant of all three - see MapData.cs.
	///
	/// LoadMap(MapPlan) is the real authoring path (a saved asset); LoadMap(string) is the same parser
	/// exposed directly for anything that doesn't go through an asset (a test, a future "load a
	/// player-made map from a pasted string" flow) - it can't carry MapPlan.LaneAssignments though (a raw
	/// string alone has nowhere to put per-tile Lane numbers), so every Source/Destination it produces is
	/// untagged ("any"). Both converge on the same placeholder grid format - see Legend.
	/// </summary>
	public class MapGenerator : MonoBehaviour
	{
		#region Loading (static - pure data, no GameObjects)

		static readonly Dictionary<char, TileType> Legend = new()
		{
			['.'] = TileType.Floor,
			['#'] = TileType.Wall,
			['B'] = TileType.Buildable,
			['S'] = TileType.Source,
			['D'] = TileType.Destination,
		};

		public static Map LoadMap(MapPlan plan)
		{
			Map map = LoadMap(plan.RawData);
			foreach (TileLaneAssignment assignment in plan.LaneAssignments)
			{
				if (map.InBounds(assignment.X, assignment.Y)) map.SetLane(assignment.X, assignment.Y, assignment.Lane);
			}
			return map;
		}

		public static Map LoadMap(string data)
		{
			string[] rows = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < rows.Length; i++) rows[i] = rows[i].Trim('\r', ' ');
			int height = rows.Length;
			int width = 0;
			foreach (string row in rows) width = Math.Max(width, row.Length);

			Map map = new Map(width, height);
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < rows[y].Length; x++)
				{
					TileType type = Legend.TryGetValue(rows[y][x], out var t) ? t : TileType.Floor;
					map.SetTile(x, y, type);
				}
			}
			return map;
		}

		/// <summary>
		/// Procedurally carves a random, legitimate Source -> Destination map: a Wall border, an interior
		/// filled with Buildable (tower real estate) by default, and a single winding width-1 Floor
		/// corridor from a top-left Source to a bottom-right Destination - same "thin path flanked by
		/// buildable plots" shape as a hand-authored map, just randomized. Each step of the carve moves
		/// one tile strictly closer to the Destination (picking the still-misaligned axis at random when
		/// both axes are still off), which both guarantees termination and gives the corridor its jagged,
		/// winding shape rather than a straight staircase.
		/// STUB-ish: single Source/Destination only (both Lane-less), no branches/dead-ends/loops, no
		/// tuning for "interesting" layouts - a placeholder generator, not a level designer.
		/// </summary>
		public static Map GenerateRandomMap(int width, int height, System.Random rng = null)
		{
			rng ??= new System.Random();
			Map map = new Map(width, height);

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					bool border = x == 0 || y == 0 || x == width - 1 || y == height - 1;
					map.SetTile(x, y, border ? TileType.Wall : TileType.Buildable);
				}
			}

			Vector2Int source = new Vector2Int(1, 1);
			Vector2Int destination = new Vector2Int(width - 2, height - 2);

			Vector2Int current = source;
			map.SetTile(current.x, current.y, TileType.Floor);

			int maxSteps = width * height * 4; // Generous safety cap against a pathological rng run.
			for (int step = 0; step < maxSteps && current != destination; step++)
			{
				bool moveOnX = rng.NextDouble() < 0.5;
				if (current.x == destination.x) moveOnX = false;
				else if (current.y == destination.y) moveOnX = true;

				if (moveOnX) current.x += Math.Sign(destination.x - current.x);
				else current.y += Math.Sign(destination.y - current.y);

				map.SetTile(current.x, current.y, TileType.Floor);
			}

			map.SetTile(source.x, source.y, TileType.Source);
			map.SetTile(destination.x, destination.y, TileType.Destination);
			return map;
		}

		#endregion Loading

		#region Gamespace building (instance - needs a Transform/prefab palette)

		public float TileSize = 1f;

		// One shared prefab for every cell - not a prefab per TileType. TileColors is what actually
		// distinguishes a Wall from a Floor from a Buildable plot visually: same GameObject, tinted per
		// instance. Cheaper to author (one prefab, not five) and matches how a real tileset usually works
		// (one mesh, palette-swapped), rather than each TileType needing its own bespoke shape.
		public GameObject TilePrefab;
		public List<TileColorEntry> TileColors = new();

		Dictionary<TileType, Color> TileColorMap =>
			tileColorCache ??= BuildColorCache();
		Dictionary<TileType, Color> tileColorCache;

		Dictionary<TileType, Color> BuildColorCache()
		{
			var cache = new Dictionary<TileType, Color>();
			foreach (TileColorEntry entry in TileColors) cache[entry.Type] = entry.Color;
			return cache;
		}

		/// <summary>Instantiates TilePrefab once per cell under this transform, tinted per TileColors - the visual counterpart to LoadMap above. A TileType with no color entry just keeps the prefab's own default material color rather than being skipped - every cell gets a tile, per the design ("TilePrefabs should be used for EACH cell").</summary>
		public void BuildGamespace(Map map)
		{
			if (TilePrefab == null) return;

			foreach (TileData tile in map.AllTiles())
			{
				GameObject instance = Instantiate(TilePrefab, TileToWorld(tile.X, tile.Y), Quaternion.identity, transform);
				if (!TileColorMap.TryGetValue(tile.Type, out Color color)) continue;

				// .material (not sharedMaterial) clones the material for this one Renderer the first time
				// it's touched, so tinting one tile never bleeds into every other instance sharing the
				// same source material.
				Renderer renderer = instance.GetComponentInChildren<Renderer>();
				if (renderer != null) renderer.material.color = color;
			}
		}

		/// <summary>Destroys every previously-instantiated tile under this transform - call before BuildGamespace when replacing a map (e.g. randomizing) rather than adding to it.</summary>
		public void Clear()
		{
			for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
		}

		public Vector3 TileToWorld(int x, int y) => new Vector3(x * TileSize, 0, y * TileSize);
		public Vector3 TileToWorld(Vector2Int grid) => TileToWorld(grid.x, grid.y);

		#endregion Gamespace building

		#region Pathing (bridges PathUtil's grid-only Vector2Int routes to gamespace)

		/// <summary>
		/// What a Monster spawned at a given Source tile should walk - every Source's resolved path (see
		/// PathUtil.FindAllPaths), converted from grid coordinates to the same world positions
		/// BuildGamespace placed the tiles at, keyed by that Source's grid position. A spawner looks up
		/// Paths[sourcePosition] and hands the resulting List&lt;Node&gt; straight to
		/// MonsterController.SetPath - no separate "route" wrapper type, since the mob only ever needs the
		/// Node list itself. Missing/empty for a Source with no reachable eligible Destination.
		/// </summary>
		public Dictionary<Vector2Int, List<Node>> Paths { get; private set; } = new();

		/// <summary>
		/// (Re)computes Paths from scratch against the given Map's current tile state - this is also the
		/// map-changed-dynamically recalculation entry point, not a separate method: BuildPaths has no
		/// incremental state of its own to invalidate (PathUtil.FindAllPaths already re-derives everything
		/// from the Map every time it's called), so calling this again after any tile mutation (a wall
		/// added/removed, RandomizeMap replacing the map outright, ...) is the whole recalculation.
		/// </summary>
		public Dictionary<Vector2Int, List<Node>> BuildPaths(Map map)
		{
			Paths = new Dictionary<Vector2Int, List<Node>>();
			foreach (PathUtil.Route gridRoute in PathUtil.FindAllPaths(map))
			{
				List<Node> nodes = new List<Node>(gridRoute.Path.Count);
				foreach (Vector2Int gridPosition in gridRoute.Path) nodes.Add(new Node(gridPosition, TileToWorld(gridPosition)));
				Paths[gridRoute.Source] = nodes;
			}
			return Paths;
		}

		#endregion Pathing
	}
}
