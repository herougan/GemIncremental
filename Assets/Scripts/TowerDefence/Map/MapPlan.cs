using System;
using System.Collections.Generic;
using TowerDefence.Entity;
using UnityEngine;
using Util.Serialisation;

namespace TowerDefence.Map
{
	/// <summary>Assigns a Lane number to one tile - see TileData.Lane for what that means. Only listed tiles get a Lane; anything not listed defaults to null ("any").</summary>
	[Serializable]
	public struct TileLaneAssignment
	{
		public int X;
		public int Y;
		public int Lane;
	}

	/// <summary>
	/// Design-time map data, authored as an asset like MonsterPlan/SkillPlan/TowerPlan - see
	/// MapGenerator.LoadMap(MapPlan) for how this turns into a runtime Map. RawData is the same
	/// placeholder one-char-per-tile format MapGenerator.LoadMap(string) parses directly - '.' Floor,
	/// '#' Wall, 'B' Buildable, 'S' Source, 'D' Destination (see MapGenerator.Legend) - STUB: not the
	/// real authoring format, just proves a MapPlan asset round-trips through the same parser a raw
	/// string does.
	///
	/// LaneAssignments is a separate overlay on top of RawData: the raw grid has no room for a per-tile
	/// number, so pairing a Source/Destination onto a specific Lane (see TileData.Lane) is authored here
	/// instead, by coordinate. LoadMap(string) alone (no MapPlan involved) has no way to carry these -
	/// every Source/Destination it produces is untagged ("any").
	/// </summary>
	[CreateAssetMenu(fileName = "MapPlan", menuName = "TowerDefence/Map/MapPlan")]
	public class MapPlan : ScriptableObject, IPlan
	{
		public SerialisableGuid Guid { get; protected set; }
		[field: SerializeField] public string Name { get; set; }

		[TextArea(5, 20)]
		public string RawData;

		public List<TileLaneAssignment> LaneAssignments = new();

		public MapPlan()
		{
			Guid = new SerialisableGuid(System.Guid.NewGuid());
			Name = "New Map";
		}
	}
}
