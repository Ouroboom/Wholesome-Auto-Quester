﻿using robotManager.Helpful;
using System.Drawing;

namespace Wholesome_Auto_Quester.Helpers
{
    public class Logger
    {
        public static void Log(string str)
        {
            Logging.Write($"[{Main.ProductName}] " + str, Logging.LogType.Normal, Color.Brown);
        }

        public static void LogDebug(string str)
        {
            if (WholesomeAQSettings.CurrentSetting.LogDebug)
            {
                Logging.Write($"[{Main.ProductName}] " + str, Logging.LogType.Debug, Color.Chocolate);
            }
        }

        public static void LogError(string str)
        {
            Logging.Write($"[{Main.ProductName}] " + str, Logging.LogType.Error, Color.Red);
        }
    }
}
