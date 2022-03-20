using UnityEngine;

namespace PackedNetworking.Util
{
    public static class NetworkingLogs
    {
        public delegate void Log(string message);

        private static Log infoMthd = Debug.Log;
        private static Log warningMthd = Debug.LogWarning;
        private static Log errorMthd = Debug.LogError;
        private static Log fatalMthd = Debug.LogError;

        internal static string Prefix { get; set; }

        /// <summary>
        /// Sets the methods to use as loggers. By default 'error' and 'fatal' gets logged using Debug.LogError.
        /// </summary>
        /// <param name="info">E.g. info about connecting/disconnecting.</param>
        /// <param name="warning">Messages to make you aware of potential unexpected behaviour.</param>
        /// <param name="error">Improper use of the library.</param>
        /// <param name="fatal">Errors, which should never occur, please contact me.</param>
        public static void Set(Log info, Log warning, Log error, Log fatal)
        {
            infoMthd = info;
            warningMthd = warning;
            errorMthd = error;
            fatalMthd = fatal;
        }

        internal static void LogInfo(string message) => infoMthd?.Invoke(GetFullMessage(message));
        internal static void LogWarning(string message) => warningMthd?.Invoke(GetFullMessage(message));
        internal static void LogError(string message) => errorMthd?.Invoke(GetFullMessage(message));
        internal static void LogFatal(string message) => fatalMthd?.Invoke("FATAL: " + GetFullMessage(message));

        private static string GetFullMessage(string message)
        {
            return Prefix + message;
        }
    }
}