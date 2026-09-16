using System.Collections.Generic;
using System.Linq;
using TowerDefence.Context;
using TowerDefence.Entity;
using TowerDefence.Entity.Skills;
using Util.Maths;

namespace Util.Game
{
	/// <summary>
	/// Global damage-by-source tracker for a battle-analysis report ("which Tower contributed the most
	/// damage, split by Skill") - subscribes once to Util.Events.EntityEventBus (the same bus
	/// GameManager.HandleEntityEvent uses), rather than needing every Entity/Skill to report itself.
	///
	/// This is the answer to "so how" from the design discussion: ISource (via TriggerContext.Source,
	/// now stamped by WrappedAction.Invoke, and Damage.Source copied from it - see those two files)
	/// is how a single hit knows who/what dealt it. That was never going to be enough on its own,
	/// though - something still has to *aggregate* across an entire battle, which is what this is.
	/// The two ideas aren't competing; ISource is the per-hit attribution, this is the running total.
	///
	/// Call Reset() at the start of a wave/battle, Start() once (e.g. from GameManager), read GetReport()
	/// at the end (a creature dying, a wave ending) - both callers just read whatever's accumulated so
	/// far, nothing here decides when a "battle" begins or ends.
	/// </summary>
	public static class BattleTracker
	{
		public class Entry
		{
			public ddouble TotalDamage;
			public int Hits;
		}

		static readonly Dictionary<(IEntity attacker, string skillName), Entry> byAttackerAndSkill = new();
		static bool subscribed;
		static float startTime;

		public static void Start()
		{
			if (subscribed) return;
			// Subscribe(OnHit, ...), not OnAnyEvent - this only ever cared about one TriggerType, so it
			// no longer runs (and immediately bails) on every other event in the game.
			Util.Events.EntityEventBus.Subscribe(TriggerType.OnHit, HandleHit);
			subscribed = true;
		}

		public static void Stop()
		{
			if (!subscribed) return;
			Util.Events.EntityEventBus.Unsubscribe(TriggerType.OnHit, HandleHit);
			subscribed = false;
		}

		public static void Reset()
		{
			byAttackerAndSkill.Clear();
			startTime = UnityEngine.Time.time;
		}

		static void HandleHit(TriggerContext ctx)
		{
			if (ctx.Damage?.Attacker == null) return;

			string skillName = (ctx.Damage.Source as ISkill)?.Plan?.Name ?? "Basic Attack";
			var key = (ctx.Damage.Attacker, skillName);

			if (!byAttackerAndSkill.TryGetValue(key, out Entry entry))
			{
				entry = new Entry();
				byAttackerAndSkill[key] = entry;
			}
			entry.TotalDamage += ctx.Damage.FinalValue;
			entry.Hits++;
		}

		/// <summary>Every (Attacker, SkillName) recorded since the last Reset(), most damage first - the actual battle report.</summary>
		public static List<(IEntity attacker, string skillName, Entry entry)> GetReport()
		{
			return byAttackerAndSkill
				.Select(kv => (kv.Key.attacker, kv.Key.skillName, kv.Value))
				.OrderByDescending(r => (double)r.Value.TotalDamage)
				.ToList();
		}

		public static ddouble GetTotalDamage()
		{
			ddouble total = 0;
			foreach (Entry entry in byAttackerAndSkill.Values) total += entry.TotalDamage;
			return total;
		}

		/// <summary>Total damage / seconds since the last Reset() - an average over the whole tracked window, not an instantaneous rate (see HudDisplay for where this gets read). 0 before Start()/Reset() has ever run.</summary>
		public static ddouble GetDPS()
		{
			float elapsed = UnityEngine.Time.time - startTime;
			return elapsed > 0f ? GetTotalDamage() / elapsed : default(ddouble);
		}
	}
}
