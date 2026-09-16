using UnityEngine;

namespace TowerDefence.Map
{
	/// <summary>
	/// One waypoint along a computed Source -> Destination path (see MapGenerator.BuildPaths/PathUtil -
	/// a whole path is just a List&lt;Node&gt;, handed to a Monster on spawn for it to walk; there's no
	/// separate wrapper type for "a route" beyond that list). GridPosition is the tile coordinate (matches
	/// TileData.Position); WorldPosition is that tile's actual gamespace position (same X/TileSize/Z math
	/// as MapGenerator.BuildGamespace's own tile placement, so a Node lines up exactly with the
	/// instantiated tile under it) - MonsterController.SetPath walks WorldPosition directly without
	/// knowing about tiles/TileSize at all.
	/// </summary>
	public class Node
	{
		public Vector2Int GridPosition;
		public Vector3 WorldPosition;

		public Node(Vector2Int gridPosition, Vector3 worldPosition)
		{
			GridPosition = gridPosition;
			WorldPosition = worldPosition;
		}
	}
}
