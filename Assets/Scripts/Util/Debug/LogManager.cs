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
		public static LogManager Instance;
		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(this);
			}
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
		public void Log(string log)
		{
			logs.Add(log);
			normalLogs.Add(log);
		}

		public void LogWarning(string log)
		{
			logs.Add($"<color=yellow>{log}</color>");
			warningLogs.Add($"<color=yellow>{log}</color>");
		}

		public void LogError(string log)
		{
			logs.Add($"<color=red>{log}</color>");
			errorLogs.Add($"<color=red>{log}</color>");
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