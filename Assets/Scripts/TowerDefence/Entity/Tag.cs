namespace TowerDefence.Entity {

		public enum Tag
		{
			Flying,
			Furry,
			Biological,
			Mechanic,
			Big,
			Small,
			Undead,

			// Damage tags (on Damage, not just Entity) - physical is any untagged damage, so there's no
			// explicit Physical value. These compose freely (Magic Fire, Physical Ranged Fire, Armor
			// Piercing Ice, ...) rather than being one exclusive "damage type" enum - see Damage.Tags.
			Ranged,
			Magic,
			Elemental,
			ArmorPiercing,

			// Descriptive/UI tags (on SkillPlan.Tags, via GetTags) - shape-of-the-ability descriptors
			// that don't correspond to any mechanical field yet (e.g. nothing currently models "this
			// skill hits an area" vs "a single target" - these are author-set labels for now, not
			// derived from real AoE/slow mechanics).
			AoE,
			SingleTarget,
			Slow,
		}
}