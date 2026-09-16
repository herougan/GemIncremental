#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TowerDefence.Map;
using UnityEditor;
using UnityEngine;
// Aliased, not bare `Map` - TowerDefence.Editor and TowerDefence.Map are sibling namespaces (both
// direct children of TowerDefence), so an unqualified `Map` here always resolves to the namespace
// (CS0118), never the class - same recurring gotcha as everywhere else this session.
using MapGrid = TowerDefence.Map.Map;

namespace TowerDefence.Editor
{
	/// <summary>
	/// Runs the "read a map format into an in-memory Map, then path over it" pipeline end to end with no
	/// scene/Play Mode - same "build everything in memory, log PASS/FAIL per section" shape as
	/// EntityEventChainSmokeTest, just for TowerDefence.Map instead of Entity/Skills. Exists specifically
	/// to answer "does the Map-from-string-then-in-memory-grid system actually work" without needing to
	/// eyeball tiles in the Scene view - every assertion here is a plain equality/connectivity check.
	/// </summary>
	public static class MapSmokeTest
	{
		[MenuItem("Tools/TowerDefence/Run Map Smoke Test")]
		public static void Run()
		{
			bool allPass = true;
			allPass &= TestLoadFromString();
			allPass &= TestSingleSourceDestinationPath();
			allPass &= TestLanePairing();
			allPass &= TestMapPlanLaneAssignments();
			allPass &= TestRandomMap();
			allPass &= TestGamespaceAndWorldPaths();

			Debug.Log(allPass
				? "[MapSmokeTest] ALL SECTIONS PASSED"
				: "[MapSmokeTest] AT LEAST ONE SECTION FAILED - see above");
		}

		// ===== A: parsing a raw string into TileType cells =====

		static bool TestLoadFromString()
		{
			string raw = string.Join("\n", new[]
			{
				"####",
				"#S.#",
				"#.D#",
				"####",
			});
			MapGrid map = MapGenerator.LoadMap(raw);

			bool pass = map.Width == 4 && map.Height == 4
				&& map.GetTile(0, 0).Type == TileType.Wall
				&& map.GetTile(1, 1).Type == TileType.Source
				&& map.GetTile(2, 1).Type == TileType.Floor
				&& map.GetTile(1, 2).Type == TileType.Floor
				&& map.GetTile(2, 2).Type == TileType.Destination;

			Debug.Log($"[A: LoadMap(string)] {(pass ? "PASS" : "FAIL")} - {map.Width}x{map.Height} grid, tile types at known coordinates.");
			return pass;
		}

		// ===== B: Dijkstra finds a connected Source -> Destination route =====

		static bool TestSingleSourceDestinationPath()
		{
			string raw = string.Join("\n", new[]
			{
				"######",
				"#S...#",
				"#B.BB#",
				"#B.BB#",
				"#..BD#",
				"######",
			});
			MapGrid map = MapGenerator.LoadMap(raw);
			List<Vector2Int> path = PathUtil.FindPath(map);

			bool endpointsCorrect = path.Count > 0 && path[0] == new Vector2Int(1, 1) && path[^1] == new Vector2Int(4, 4);
			bool connected = IsFullyConnected(path);
			bool neverCrossesWallOrBuildable = path.All(p => map.GetTile(p).Type is TileType.Floor or TileType.Source or TileType.Destination);

			bool pass = endpointsCorrect && connected && neverCrossesWallOrBuildable;
			Debug.Log($"[B: PathUtil.FindPath] {(pass ? "PASS" : "FAIL")} - {path.Count} Nodes, endpoints correct={endpointsCorrect}, connected={connected}, walkable-only={neverCrossesWallOrBuildable}.");
			return pass;
		}

		// ===== C: multiple Source/Destination with Lane pairing + an any-to-any pool =====

		static bool TestLanePairing()
		{
			// Two Sources, two Destinations: (1,1)->Lane 1, (1,3)->untagged ("any"); Destinations at
			// (6,1)->Lane 1, (6,3)->untagged. A correct resolution pairs the Lane-1 Source with the
			// Lane-1 Destination specifically, and the untagged Source with the untagged Destination -
			// never crossed, even though both routes are the same length and equally "valid" distance-wise.
			string raw = string.Join("\n", new[]
			{
				"########",
				"#S....D#",
				"#......#",
				"#S....D#",
				"########",
			});
			MapGrid map = MapGenerator.LoadMap(raw);
			map.SetLane(1, 1, 1);
			map.SetLane(6, 1, 1);
			// (1,3) and (6,3) stay untagged (null) - the any-to-any pool.

			List<PathUtil.Route> routes = PathUtil.FindAllPaths(map);
			PathUtil.Route laneRoute = routes.FirstOrDefault(r => r.Source == new Vector2Int(1, 1));
			PathUtil.Route anyRoute = routes.FirstOrDefault(r => r.Source == new Vector2Int(1, 3));

			bool pass = routes.Count == 2
				&& laneRoute != null && laneRoute.Destination == new Vector2Int(6, 1) && laneRoute.Lane == 1
				&& anyRoute != null && anyRoute.Destination == new Vector2Int(6, 3) && anyRoute.Lane == null;

			Debug.Log($"[C: Lane pairing] {(pass ? "PASS" : "FAIL")} - {routes.Count} routes resolved, Lane-1 paired correctly={laneRoute?.Destination == new Vector2Int(6, 1)}, any-pool paired correctly={anyRoute?.Destination == new Vector2Int(6, 3)}.");
			return pass;
		}

		// ===== D: MapPlan.LaneAssignments actually reaches the loaded Map =====

		static bool TestMapPlanLaneAssignments()
		{
			MapPlan plan = ScriptableObject.CreateInstance<MapPlan>();
			plan.RawData = string.Join("\n", new[]
			{
				"####",
				"#S.#",
				"#.D#",
				"####",
			});
			plan.LaneAssignments.Add(new TileLaneAssignment { X = 1, Y = 1, Lane = 3 });

			MapGrid map = MapGenerator.LoadMap(plan);
			bool pass = map.GetTile(1, 1).Lane == 3 && map.GetTile(2, 2).Lane == null;

			Object.DestroyImmediate(plan);
			Debug.Log($"[D: MapPlan.LaneAssignments] {(pass ? "PASS" : "FAIL")} - Source (1,1) Lane={(pass ? "3 as authored" : "wrong")}.");
			return pass;
		}

		// ===== E: the procedural generator always produces a walkable, fully-connected map =====

		static bool TestRandomMap()
		{
			MapGrid map = MapGenerator.GenerateRandomMap(14, 12);
			bool hasExactlyOneOfEach = map.GetTilesOfType(TileType.Source).Count() == 1 && map.GetTilesOfType(TileType.Destination).Count() == 1;
			List<Vector2Int> path = PathUtil.FindPath(map);
			bool pass = hasExactlyOneOfEach && path.Count > 0 && IsFullyConnected(path);

			Debug.Log($"[E: GenerateRandomMap] {(pass ? "PASS" : "FAIL")} - 1 Source/1 Destination={hasExactlyOneOfEach}, path found and connected={path.Count > 0 && IsFullyConnected(path)}.");
			return pass;
		}

		// ===== F: MapGenerator's gamespace/world-space conversion agrees with BuildPaths' own Nodes =====

		static bool TestGamespaceAndWorldPaths()
		{
			GameObject scratch = new GameObject("MapSmokeTest-Scratch");
			bool pass;
			try
			{
				MapGenerator generator = scratch.AddComponent<MapGenerator>();
				generator.TileSize = 2f; // Deliberately not 1, so a bug that hardcodes tile size would show up here.

				string raw = string.Join("\n", new[]
				{
					"####",
					"#S.#",
					"#.D#",
					"####",
				});
				MapGrid map = MapGenerator.LoadMap(raw);
				Dictionary<Vector2Int, List<Node>> paths = generator.BuildPaths(map);

				bool hasRoute = paths.TryGetValue(new Vector2Int(1, 1), out List<Node> nodes) && nodes.Count > 0;
				bool worldMatchesTileSize = hasRoute && nodes[0].WorldPosition == new Vector3(1 * 2f, 0, 1 * 2f) && nodes[^1].WorldPosition == new Vector3(2 * 2f, 0, 2 * 2f);

				pass = hasRoute && worldMatchesTileSize;
				Debug.Log($"[F: World-space paths] {(pass ? "PASS" : "FAIL")} - route found={hasRoute}, WorldPosition honours TileSize={worldMatchesTileSize}.");
			}
			finally
			{
				Object.DestroyImmediate(scratch);
			}
			return pass;
		}

		// ===== Util =====

		static bool IsFullyConnected(List<Vector2Int> path)
		{
			for (int i = 1; i < path.Count; i++)
			{
				int stepDistance = System.Math.Abs(path[i].x - path[i - 1].x) + System.Math.Abs(path[i].y - path[i - 1].y);
				if (stepDistance != 1) return false; // Not 4-directionally adjacent - the path has a gap or a diagonal jump.
			}
			return path.Count > 0;
		}
	}
}
#endif
