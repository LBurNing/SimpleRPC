using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game
{
    public static class LogHelper
    {
        private static readonly StringBuilder _log = new StringBuilder();

        public static void Log(string log)
        {
            if (!Debug.unityLogger.logEnabled)
                return;

            _log.AppendLine($"{DateTime.Now.ToString()} D: {log}");
            Debug.Log(log);
        }

        public static void LogError(string log)
        {
            _log.AppendLine($"E: <color=#ff0000>{log}</color>");
            Debug.LogError(log);
        }

        public static void LogWarning(string log)
        {
            _log.AppendLine($"E: <color=#FFFF00>{log}</color>");
            Debug.LogWarning(log);
        }

        public static string LogInfo {  get { return _log.ToString(); } }
    }
}