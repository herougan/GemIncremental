using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence.Map
{
	/// <summary>
	/// Dijkstra pathfinding over a Map's grid - 4-directional, uniform edge cost (1 per step). Walkable =
	/// Floor/Source/Destination; Wall AND Buildable both block the path (Buildable is where Towers go -
	/// authored ground, not walkable ground, see TileType's own doc comment), so towers never end up
	/// standing in the monsters' way.
	/// Uniform cost makes this equivalent to a plain BFS, but it's written as Dijkstra (an explicit
	/// distance map + a priority frontier) since the grid is expected to grow real terrain costs later
	/// (mud/ice slow tiles, elevation, etc.) - swapping those in later is a one-line edge-cost change here,
	/// not a rewrite.
	/// </summary>
	public static class PathUtil
	{
		static readonly Vector2Int[] Directions =
		{
			new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
		};

		static bool IsWalkable(TileType type) => type is TileType.Floor or TileType.Source or TileType.Destination;

		/// <summary>One resolved Source -> Destination route in grid space - see FindAllPaths.</summary>
		public class Route
		{
			public Vector2Int Source;
			public Vector2Int? Destination;
			public int? Lane;
			public List<Vector2Int> Path = new();
		}

		/// <summary>
		/// Resolves every Source tile on the map to its own shortest route, per TileData.Lane's pairing
		/// rule: a Source tagged with a Lane pairs ONLY with Destinations sharing that exact Lane (a
		/// dedicated lane - no fallback to the shared pool, so a paired lane's monsters can never leak onto
		/// a different lane's exit); an untagged Source (Lane == null) draws from the pool of every
		/// untagged Destination ("any-to-any"). Both kinds of tile can coexist on the same map. Always
		/// returns one Route per Source, even if unreachable (Path/Destination then empty/null) - callers
		/// see every Source accounted for rather than silently missing ones with no valid route.
		/// </summary>
		public static List<Route> FindAllPaths(Map map)
		{
			List<TileData> sources = new(map.GetTilesOfType(TileType.Source));
			List<TileData> destinations = new(map.GetTilesOfType(TileType.Destination));

			Dictionary<int, List<Vector2Int>> byLane = new();
			List<Vector2Int> anyPool = new();
			foreach (TileData destination in destinations)
			{
				if (destination.Lane.HasValue)
				{
					if (!byLane.TryGetValue(destination.Lane.Value, out List<Vector2Int> lane)) byLane[destination.Lane.Value] = lane = new List<Vector2Int>();
					lane.Add(destination.Position);
				}
				else anyPool.Add(destination.Position);
			}

			List<Route> routes = new();
			foreach (TileData source in sources)
			{
				List<Vector2Int> eligible = source.Lane.HasValue && byLane.TryGetValue(source.Lane.Value, out List<Vector2Int> laned) ? laned : anyPool;
				List<Vector2Int> path = FindPath(map, source.Position, eligible);
				routes.Add(new Route
				{
					Source = source.Position,
					Destination = path.Count > 0 ? path[^1] : null,
					Lane = source.Lane,
					Path = path,
				});
			}
			return routes;
		}

		/// <summary>Simple single-Source/single-Destination convenience - finds the map's own first Source/Destination (TryFindTileOfType) and paths between them directly, ignoring Lane entirely. Prefer FindAllPaths for anything that might have more than one of either.</summary>
		public static List<Vector2Int> FindPath(Map map)
		{
			if (!map.TryFindTileOfType(TileType.Source, out Vector2Int source)) return new List<Vector2Int>();
			if (!map.TryFindTileOfType(TileType.Destination, out Vector2Int destination)) return new List<Vector2Int>();
			return FindPath(map, source, destination);
		}

		public static List<Vector2Int> FindPath(Map map, Vector2Int from, Vector2Int to) => FindPath(map, from, new[] { to });

		/// <summary>Dijkstra from `from` to whichever tile in `toAny` is nearest - stops the moment any target is dequeued, which is still optimal since Dijkstra visits tiles in non-decreasing distance order. Empty if none of `toAny` is reachable (or `toAny` is empty).</summary>
		public static List<Vector2Int> FindPath(Map map, Vector2Int from, IReadOnlyCollection<Vector2Int> toAny)
		{
			if (toAny.Count == 0) return new List<Vector2Int>();
			HashSet<Vector2Int> targets = toAny as HashSet<Vector2Int> ?? new HashSet<Vector2Int>(toAny);

			Dictionary<Vector2Int, int> distance = new() { [from] = 0 };
			Dictionary<Vector2Int, Vector2Int> previous = new();
			// A plain list stand-in for a priority queue - grids here are small (levels, not open-world
			// terrain), so O(n) frontier scans are fine; swap for a real heap if that stops being true.
			List<Vector2Int> frontier = new() { from };
			HashSet<Vector2Int> visited = new();

			while (frontier.Count > 0)
			{
				Vector2Int current = PopClosest(frontier, distance);
				if (targets.Contains(current)) return ReconstructPath(previous, from, current);
				if (!visited.Add(current)) continue;

				foreach (Vector2Int direction in Directions)
				{
					Vector2Int neighbour = current + direction;
					if (!map.InBounds(neighbour) || visited.Contains(neighbour)) continue;
					if (!IsWalkable(map.GetTile(neighbour).Type)) continue;

					int candidateDistance = distance[current] + 1;
					if (distance.TryGetValue(neighbour, out int existing) && existing <= candidateDistance) continue;

					distance[neighbour] = candidateDistance;
					previous[neighbour] = current;
					frontier.Add(neighbour);
				}
			}

			return new List<Vector2Int>(); // No target in toAny was reachable.
		}

		static Vector2Int PopClosest(List<Vector2Int> frontier, Dictionary<Vector2Int, int> distance)
		{
			int bestIndex = 0;
			for (int i = 1; i < frontier.Count; i++)
			{
				if (distance[frontier[i]] < distance[frontier[bestIndex]]) bestIndex = i;
			}
			Vector2Int best = frontier[bestIndex];
			frontier.RemoveAt(bestIndex);
			return best;
		}

		static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> previous, Vector2Int from, Vector2Int to)
		{
			List<Vector2Int> path = new() { to };
			Vector2Int current = to;
			while (current != from)
			{
				current = previous[current];
				path.Add(current);
			}
			path.Reverse();
			return path;
		}
	}
}
