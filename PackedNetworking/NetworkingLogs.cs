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
        /// Sets the methods to use as loggers.
        /// </summary>
        /// <param name="info">E.g. info about connecting/disconnecting.</param>
        /// <param name="warning">Messages to make you aware of potential unexpected behaviour.</param>
        /// <param name="error">Improper use of the library.</param>
        public static void Set(Log info = null, Log warning = null, Log error = null)
        {
            infoMthd = info ?? infoMthd;
            warningMthd = warning ?? warningMthd;
            errorMthd = error ?? errorMthd;
        }

        /// <summary>
        /// Use this to disable a logging type by passing this method into the corresponding parameter of the 'Set' method.
        /// </summary>
        /// <param name="message"></param>
        public static void NoLog(string message) { }

        internal static void LogInfo(string message) => infoMthd?.Invoke(GetFullMessage(message));
        internal static void LogWarning(string message) => warningMthd?.Invoke(GetFullMessage(message));
        internal static void LogError(string message) => errorMthd?.Invoke(GetFullMessage(message));
        internal static void LogFatal(string message) => fatalMthd?.Invoke("FATAL: " + GetFullMessage(message));

        private static string GetFullMessage(string message) => Prefix + message;
    }
}