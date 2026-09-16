using System.Collections.Generic;
using System.Linq;

namespace Player
{
	/// <summary>
	/// Placeholder shape - no Artefact data/behaviour has been designed yet (what an Artefact actually
	/// does: stat bonuses, triggered effects, a passive like a Skill? unclear). Kept as a real type
	/// rather than a bare string/enum so ArtefactCollection below has something concrete to hold, but
	/// expect this to grow real fields once that's decided.
	/// </summary>
	public interface IArtefact
	{
		string Name { get; }
	}

	/// <summary>
	/// Artefacts the player has found/owns - kept separate from PlayerSkills (not folded into one
	/// generic "unlocked things" list) since Artefacts and Skills are different meta-progression
	/// currencies likely to end up with different acquisition/equip rules.
	/// </summary>
	public class ArtefactCollection
	{
		public List<IArtefact> Owned { get; } = new();

		public bool Owns(IArtefact artefact) => Owned.Contains(artefact);

		public void Add(IArtefact artefact)
		{
			if (!Owns(artefact)) Owned.Add(artefact);
		}

		public IArtefact FindByName(string name) => Owned.FirstOrDefault(a => a.Name == name);
	}
}
