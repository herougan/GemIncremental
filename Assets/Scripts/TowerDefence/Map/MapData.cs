using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence.Map
{
	/// <summary>
	/// Floor is walkable ground PathUtil's Dijkstra can route monsters over; Buildable is also authored
	/// ground but deliberately NOT walkable (see PathUtil) - it's where Towers go, kept out of the monster
	/// path on purpose so a thin Floor corridor flanked by Buildable plots is a legitimate, buildable map
	/// rather than towers and monsters sharing the same tiles. Wall is fully blocked (neither walkable nor
	/// buildable - decoration/boundary). Source/Destination count as walkable, and a map can have any
	/// number of either - see TileData.Lane and PathUtil.FindAllPaths for how multiple ones get resolved
	/// into routes. There's deliberately no "Path" tile type - the path is *computed* from Source to
	/// Destination over Floor tiles, not hand-authored.
	/// Still placeholder vocabulary otherwise (no elevation, buildable-but-only-for-X, etc. - that's for
	/// later).
	/// </summary>
	public enum TileType
	{
		Floor,
		Wall,
		Buildable,
		Source,
		Destination,
	}

	public struct TileData
	{
		public TileType Type;
		public int X;
		public int Y;

		/// <summary>
		/// Only meaningful on Source/Destination tiles - null means "any" (joins the shared any-to-any
		/// pool of every other untagged Source/Destination); a value means "paired lane N" (matches ONLY
		/// Destinations/Sources sharing that same Lane, no fallback to the any-pool). See
		/// PathUtil.FindAllPaths for the resolution rule and MapPlan.LaneAssignments for how it's authored
		/// (the raw-string format has no room for a per-tile number, so lanes are a separate overlay).
		/// </summary>
		public int? Lane;

		public Vector2Int Position => new Vector2Int(X, Y);
	}

	/// <summary>
	/// Pure in-memory map representation - a grid of TileData, nothing Unity/GameObject about it (Vector2Int
	/// is just a coordinate struct, same spirit as ddouble - no GameObject/scene dependency). See
	/// MapGenerator.LoadMap for how this gets built (from a MapPlan asset or a raw string - same seam
	/// either way), MapGenerator.BuildGamespace for the visual layer built from one of these, and PathUtil
	/// for the Source -> Destination pathfinding run over it. Same data/visual split as Entity/EntityController.
	/// </summary>
	public class Map
	{
		public int Width { get; }
		public int Height { get; }

		readonly TileData[,] tiles;

		public Map(int width, int height)
		{
			Width = width;
			Height = height;
			tiles = new TileData[width, height];
		}

		public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
		public bool InBounds(Vector2Int position) => InBounds(position.x, position.y);

		public TileData GetTile(int x, int y) => tiles[x, y];
		public TileData GetTile(Vector2Int position) => tiles[position.x, position.y];

		public void SetTile(int x, int y, TileType type)
		{
			tiles[x, y] = new TileData { Type = type, X = x, Y = y };
		}

		/// <summary>Overwrites just the Lane of an already-set tile (Type/X/Y untouched) - see TileData.Lane. Meant for Source/Destination tiles; harmless but meaningless on anything else.</summary>
		public void SetLane(int x, int y, int? lane)
		{
			TileData tile = tiles[x, y];
			tile.Lane = lane;
			tiles[x, y] = tile;
		}

		public IEnumerable<TileData> AllTiles()
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++) yield return tiles[x, y];
			}
		}

		public IEnumerable<TileData> GetTilesOfType(TileType type)
		{
			foreach (TileData tile in AllTiles())
			{
				if (tile.Type == type) yield return tile;
			}
		}

		/// <summary>First tile of the given type, scanning in x-then-y order - the simple single-Source/single-Destination case. False (default Vector2Int) if the map has none. Prefer GetTilesOfType for anything that might have more than one.</summary>
		public bool TryFindTileOfType(TileType type, out Vector2Int position)
		{
			foreach (TileData tile in AllTiles())
			{
				if (tile.Type != type) continue;
				position = tile.Position;
				return true;
			}
			position = default;
			return false;
		}
	}
}
