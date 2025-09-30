using Godot;
using System;

/// <summary>
/// Structured logger displaying messages in a standard format across the project.
/// </summary>
public static class Logger
{
    private static LogLevel logLevel = LogLevel.Debug;

    /// <summary>
    /// Defines the level of logging that will be processed
    /// </summary>
    public enum LogLevel
    {
        Debug = 100,
        Information = 200,
        Warning = 300,
        Error = 400
    }

    /// <summary>
    /// Assigns the level of logging that will be processed
    /// </summary>
    /// <param name="logLevel">Level to log</param>
    private static void SetLogLevel(LogLevel logLevel) => Logger.logLevel = logLevel;

    /// <summary>
    /// Logs a debug message to the console
    /// </summary>
    /// <param name="message">Message to log</param>
    public static void LogDebug(string message)
    {
        if (logLevel > LogLevel.Debug)
            return;

        GD.Print($"DEBUG: {message}");
    }

    /// <summary>
    /// General information to be logged in the console
    /// </summary>
    /// <param name="message">Message to log</param>
    public static void LogInformation(string message)
    {
        if (logLevel > LogLevel.Information)
            return;

        GD.Print($"INFO: {message}");
    }

    /// <summary>
    /// Logs a warning to the console
    /// </summary>
    /// <param name="message">Message to log</param>
    public static void LogWarning(string message) => LogWarning(null, message);

    /// <summary>
    /// Logs a warning to the console with the provided exception
    /// </summary>
    /// <param name="exception">Exception to log</param>
    /// <param name="message">Message to log</param>
    public static void LogWarning(Exception exception, string message)
    {
        if (logLevel > LogLevel.Warning)
            return;

        GD.PushWarning($"WARN: {message}\n{FormatException(exception)}");
    }

    /// <summary>
    /// Logs an error message to the console
    /// </summary>
    /// <param name="message">Message to log</param>
    public static void LogError(string message) => LogError(null, message);

    /// <summary>
    /// Logs an error message to the console with the provided exception
    /// </summary>
    /// <param name="exception">Exception to log</param>
    /// <param name="message">Message to log</param>
    public static void LogError(Exception exception, string message)
    {
        GD.PushError($"ERROR: {message}\n{FormatException(exception)}");
    }

    /// <summary>
    /// Formats the provided exception to a nicely formatted string. Handles null as part of the format.
    /// </summary>
    /// <param name="exception">Exception to format</param>
    /// <returns>Formatted string representation</returns>
    private static string FormatException(Exception exception)
    {
        return exception is null ? string.Empty : exception.ToString();
    }
}