using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Util.Debug
{
	[ExecuteInEditMode]
	public class LogManager : MonoBehaviour
	{
		#region Preamble

		static LogManager instance;

		/// <summary>
		/// Lazily self-creates a "LogManager" GameObject the first time anything asks for Instance,
		/// rather than requiring every scene to remember to add one - `LogManager.Instance.LogWarning(...)`
		/// is called from dozens of places across the codebase (any ActionHandler's malformed-data guard
		/// clauses especially), and a missing LogManager used to mean a NullReferenceException AT the
		/// warning call site instead of the actual warning it was trying to report - the real message
		/// never surfaced, just a crash pointing at the LogWarning line itself. Assigning `instance`
		/// directly (not through this property) inside Awake still works fine underneath this - Awake
		/// just becomes a formality confirming whichever one got here first.
		/// </summary>
		public static LogManager Instance
		{
			get
			{
				if (instance != null) return instance;
				GameObject go = new GameObject(nameof(LogManager));
				instance = go.AddComponent<LogManager>();
				if (Application.isPlaying) DontDestroyOnLoad(go);
				return instance;
			}
		}

		void Awake()
		{
			if (instance == null) instance = this;
			else if (instance != this) Destroy(this);
		}
		#endregion Preamble

		#region Logging Functions

		private List<string> logs = new List<string>();
		private List<string> normalLogs = new List<string>();
		private List<string> warningLogs = new List<string>();
		private List<string> errorLogs = new List<string>();
		private string logString = "";

		void Start()
		{

		}

		/// <summary>
		/// Every Log/LogWarning/LogError also mirrors to Unity's own Debug.Log family - previously this
		/// only ever appended to the internal `logs` list (for a custom in-game log readout via
		/// GetLogString/BuildLogs), which meant nothing ever showed in the real Console window unless
		/// that custom UI existed and was open. Mirroring is what actually makes warnings/errors visible
		/// (and, for errors, clickable-with-stack-trace) during normal Play Mode testing.
		/// </summary>
		public void Log(string log)
		{
			logs.Add(log);
			normalLogs.Add(log);
			UnityEngine.Debug.Log(log);
		}

		public void LogWarning(string log)
		{
			logs.Add($"<color=yellow>{log}</color>");
			warningLogs.Add($"<color=yellow>{log}</color>");
			UnityEngine.Debug.LogWarning(log);
		}

		public void LogError(string log)
		{
			logs.Add($"<color=red>{log}</color>");
			errorLogs.Add($"<color=red>{log}</color>");
			UnityEngine.Debug.LogError(log);
		}

		public ref List<string> GetLogs()
		{
			return ref logs;
		}

		public string GetLogString()
		{
			return logString;
		}

		public void BuildLogs()
		{
			logString = string.Join(Environment.NewLine, logs.Reverse<string>().Take(20));
		}

		public void BuildLogs(bool includeNormal, bool includeWarning, bool includeError)
		{
			List<string> filteredLogs = new List<string>();
			if (includeNormal)
			{
				filteredLogs.AddRange(normalLogs);
			}
			if (includeWarning)
			{
				filteredLogs.AddRange(warningLogs);
			}
			if (includeError)
			{
				filteredLogs.AddRange(errorLogs);
			}
			logString = string.Join(Environment.NewLine, filteredLogs.Reverse<string>().Take(20));
		}

		public void ClearLogs()
		{
			logs.Clear();
			normalLogs.Clear();
			warningLogs.Clear();
			errorLogs.Clear();
			logString = "";
		}

		#endregion Logging Functions
	}
}
