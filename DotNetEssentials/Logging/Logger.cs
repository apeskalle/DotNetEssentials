using DotNetEssentials.Crypto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace DotNetEssentials.Logging
{
    public static class Logger
    {
        #region PropertiesAndMembers

        public static HashSet<LogLevel> Levels { get; } = new HashSet<LogLevel>();

        public static HashSet<LogMode> Modes { get; } = new HashSet<LogMode>();

        public static string FilePath { get; private set; } = "Log.txt";

        public static string EntrySeparator { get; private set; } = "\n\n";

        public static string FileEntryEncryptionPassword { get; private set; } = null;

        private static long _logerFailed = Interlocked.Exchange(ref _logerFailed, 0);

        #endregion

        #region Initializers

        public static void SetLevels(params LogLevel[] levels)
        {
            if (Levels.Count != 0)
            {
                Levels.Clear();
            }

            if (levels != null)
            {
                foreach (var level in levels)
                {
                    Levels.Add(level);
                }
            }
        }

        public static void SetTypes(params LogMode[] modes)
        {
            if (Modes.Count != 0)
            {
                Modes.Clear();
            }

            if (modes != null)
            {
                foreach (var mode in modes)
                {
                    Modes.Add(mode);
                }
            }
        }

        public static void SetFilePath(string filePath) => FilePath = Guard.NotNullOrEmptyOrWhitespace(nameof(filePath), filePath, trim: true);

        public static void SetEntrySeparator(string entrySeparator) => EntrySeparator = Guard.NotNull(nameof(entrySeparator), entrySeparator);

        public static void SetFileEntryEncryptionPassword(string password) => FileEntryEncryptionPassword = password;

        #endregion

        #region Methods

        public static void DecryptLogEntries(string destination)
        {
            var encrypted = File.ReadAllText(FilePath);

            var dir = Path.GetDirectoryName(destination);
            Directory.CreateDirectory(dir);

            foreach(var entry in encrypted.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var decryptedEntry = StringCipher.Decrypt(entry, FileEntryEncryptionPassword);
                File.AppendAllText(destination, $"{decryptedEntry}{EntrySeparator}");
            }
        }

        #endregion

        #region LoggingMethods

        private static void Log(LogLevel level, string message, string category)
        {
            try
            {
                if (Modes.Count == 0)
                {
                    return;
                }

                if(!Levels.Contains(level))
                {
                    return;
                }

                message = string.IsNullOrWhiteSpace(message) ? "" : message;
                category = string.IsNullOrWhiteSpace(category) ? "" : category;

                var finalLogMessage = $"{level.ToString().ToUpperInvariant()} {category} {DateTimeOffset.UtcNow}\n{message}{EntrySeparator}";

                if(Modes.Contains(LogMode.Console))
                {
                    Console.Write(finalLogMessage);
                }

                if (Modes.Contains(LogMode.Console))
                {
                    Debug.Write(finalLogMessage);
                }

                if (Modes.Contains(LogMode.File))
                {
                    var dir = Path.GetDirectoryName(FilePath);
                    Directory.CreateDirectory(dir);

                    if (FileEntryEncryptionPassword != null)
                    {
                        // take the separator down and add a comma (not base64)
                        var replacedSeparatorWithCommaMessage = finalLogMessage.Substring(0, finalLogMessage.Length - EntrySeparator.Length);
                        var encryptedLogMessage = StringCipher.Encrypt(replacedSeparatorWithCommaMessage, FileEntryEncryptionPassword) + ',';

                        File.AppendAllText(FilePath, encryptedLogMessage);
                    }
                    else
                    {                        
                        File.AppendAllText(FilePath, finalLogMessage);
                    }
                }
            }
            catch(Exception ex)
            {
                Interlocked.Increment(ref _logerFailed);
                if(Interlocked.Read(ref _logerFailed) > 1)
                {
                    Interlocked.Exchange(ref _logerFailed, 0);
                    return;
                }
                LogDebug($"Logging failed: {ex}", $"{nameof(DotNetEssentials)}.{nameof(Logging)}.{nameof(Logger)}");
            }
        }

        private static void Log(LogLevel level, string message, Type category)
        {
            if (category == null)
            {
                Log(level, message, "");
            }
            else
            {
                Log(level, message, category.ToString());
            }
        }

        private static void Log<T>(LogLevel level, string message) => Log(level, message, typeof(T).ToString());

        /// <summary>
        /// For information that is valuable only to a developer debugging an issue.
        /// These messages may contain sensitive application data and so should not be enabled in a production environment.
        /// Example: "Credentials: {"User":"someuser", "Password":"P@ssword"}"
        /// </summary>
        public static void LogTrace<T>(string message) => Log<T>(LogLevel.Trace, message);
        /// <summary>
        /// For information that is valuable only to a developer debugging an issue.
        /// These messages may contain sensitive application data and so should not be enabled in a production environment.
        /// Example: "Credentials: {"User":"someuser", "Password":"P@ssword"}"
        /// </summary>
        public static void LogTrace(string message, Type category) => Log(LogLevel.Trace, message, category);
        /// <summary>
        /// For information that is valuable only to a developer debugging an issue.
        /// These messages may contain sensitive application data and so should not be enabled in a production environment.
        /// Example: "Credentials: {"User":"someuser", "Password":"P@ssword"}"
        /// </summary>
        public static void LogTrace(string message, string category = "") => Log(LogLevel.Trace, message, category);

        /// <summary>
        /// For information that has short-term usefulness during development and debugging.
        /// Example: "Entering method Configure with flag set to true."
        /// You typically would not enable Debug level logs in production unless you are troubleshooting, due to the high volume of logs.
        /// </summary>
        public static void LogDebug<T>(string message) => Log<T>(LogLevel.Debug, message);
        /// <summary>
        /// For information that has short-term usefulness during development and debugging.
        /// Example: "Entering method Configure with flag set to true."
        /// You typically would not enable Debug level logs in production unless you are troubleshooting, due to the high volume of logs.
        /// </summary>
        public static void LogDebug(string message, Type category) => Log(LogLevel.Debug, message, category);
        /// <summary>
        /// For information that has short-term usefulness during development and debugging.
        /// Example: "Entering method Configure with flag set to true."
        /// You typically would not enable Debug level logs in production unless you are troubleshooting, due to the high volume of logs.
        /// </summary>
        public static void LogDebug(string message, string category = "") => Log(LogLevel.Debug, message, category);

        /// <summary>
        /// For tracking the general flow of the application.
        /// These logs typically have some long-term value.
        /// Example: "Request received for path /api/todo"
        /// </summary>
        public static void LogInfo<T>(string message) => Log<T>(LogLevel.Info, message);
        /// <summary>
        /// For tracking the general flow of the application.
        /// These logs typically have some long-term value.
        /// Example: "Request received for path /api/todo"
        /// </summary>
        public static void LogInfo(string message, Type category) => Log(LogLevel.Info, message, category);
        /// <summary>
        /// For tracking the general flow of the application.
        /// These logs typically have some long-term value.
        /// Example: "Request received for path /api/todo"
        /// </summary>
        public static void LogInfo(string message, string category = "") => Log(LogLevel.Info, message, category);

        /// <summary>
        /// For abnormal or unexpected events in the application flow.
        /// These may include errors or other conditions that do not cause the application to stop, but which may need to be investigated.
        /// Handled exceptions are a common place to use the Warning log level.
        /// Example: "FileNotFoundException for file quotes.txt."
        /// </summary>
        public static void LogWarning<T>(string message) => Log<T>(LogLevel.Warning, message);
        /// <summary>
        /// For abnormal or unexpected events in the application flow.
        /// These may include errors or other conditions that do not cause the application to stop, but which may need to be investigated.
        /// Handled exceptions are a common place to use the Warning log level.
        /// Example: "FileNotFoundException for file quotes.txt."
        /// </summary>
        public static void LogWarning(string message, Type category) => Log(LogLevel.Warning, message, category);
        /// <summary>
        /// For abnormal or unexpected events in the application flow.
        /// These may include errors or other conditions that do not cause the application to stop, but which may need to be investigated.
        /// Handled exceptions are a common place to use the Warning log level.
        /// Example: "FileNotFoundException for file quotes.txt."
        /// </summary>
        public static void LogWarning(string message, string category = "") => Log(LogLevel.Warning, message, category);

        /// <summary>
        /// For errors and exceptions that cannot be handled.
        /// These messages indicate a failure in the current activity or operation (such as the current HTTP request), not an application-wide failure.
        /// Example log message: "Cannot insert record due to duplicate key violation."
        /// </summary>
        public static void LogError<T>(string message) => Log<T>(LogLevel.Error, message);
        /// <summary>
        /// For errors and exceptions that cannot be handled.
        /// These messages indicate a failure in the current activity or operation (such as the current HTTP request), not an application-wide failure.
        /// Example log message: "Cannot insert record due to duplicate key violation."
        /// </summary>
        public static void LogError(string message, Type category) => Log(LogLevel.Error, message, category);
        /// <summary>
        /// For errors and exceptions that cannot be handled.
        /// These messages indicate a failure in the current activity or operation (such as the current HTTP request), not an application-wide failure.
        /// Example log message: "Cannot insert record due to duplicate key violation."
        /// </summary>
        public static void LogError(string message, string category = "") => Log(LogLevel.Error, message, category);

        /// <summary>
        /// For failures that require immediate attention.
        /// Examples: data loss scenarios, out of disk space.
        /// </summary>
        public static void LogCritical<T>(string message) => Log<T>(LogLevel.Critical, message);
        /// <summary>
        /// For failures that require immediate attention.
        /// Examples: data loss scenarios, out of disk space.
        /// </summary>
        public static void LogCritical(string message, Type category) => Log(LogLevel.Critical, message, category);
        /// <summary>
        /// For failures that require immediate attention.
        /// Examples: data loss scenarios, out of disk space.
        /// </summary>
        public static void LogCritical(string message, string category = "") => Log(LogLevel.Critical, message, category);

        #endregion
    }
}
