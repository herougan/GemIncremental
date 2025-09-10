using System;

namespace TowerDefence.Entity.Skills.Buffs
{
	public interface IAura : IBuff
	{
		// Define properties or methods that an Aura should have

		SkillData Data { get; }
		float Range { get; }
		bool IsAffectSelf { get; }
		bool IsAffectOthers { get; }
		bool IsInstance { get; } // Indicates if the aura is an instance from an Aura Source
	}

	public class Aura : Buff, IAura
	{
		// Implementation of aura logic

		/// <summary>
		/// Range is in units of tiles. (1 tile is x=1, y=1 big)
		/// </summary>
		public float Range { get; private set; }

		public bool IsAffectSelf { get; private set; }

		public bool IsAffectOthers { get; private set; } = true;

		public bool IsInstance => throw new NotImplementedException();

		SkillData Data { get; private set; }

		SkillData IAura.Data => Data;
	}
}

// TODO move Aura into Buff, just a "Aura check".
// In the Entity menu, Auras would be different from Buffs.