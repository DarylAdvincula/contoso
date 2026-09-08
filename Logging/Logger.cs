using System.Diagnostics;
using System.Text;

namespace ContosoUniversity.Logging;

public class Logger : ILogger
{
    public void Information(string message)
    {
        Console.WriteLine(message);
    }

    public void Information(string fmt, params object[] vars)
    {
        Console.WriteLine(fmt, vars);
    }

    public void Information(Exception exception, string fmt, params object[] vars)
    {
        Console.WriteLine(FormatExceptionMessage(exception, fmt, vars));
    }

    public void Warning(string message)
    {
        Console.WriteLine(message);
    }

    public void Warning(string fmt, params object[] vars)
    {
        Console.WriteLine(fmt, vars);
    }

    public void Warning(Exception exception, string fmt, params object[] vars)
    {
        Console.WriteLine(FormatExceptionMessage(exception, fmt, vars));
    }

    public void Error(string message)
    {
        Console.WriteLine(message);
    }

    public void Error(string fmt, params object[] vars)
    {
        Console.WriteLine(fmt, vars);
    }

    public void Error(Exception exception, string fmt, params object[] vars)
    {
        Console.WriteLine(FormatExceptionMessage(exception, fmt, vars));
    }

    public void TraceApi(string componentName, string method, TimeSpan timespan)
    {
        TraceApi(componentName, method, timespan, "");
    }

    public void TraceApi(string componentName, string method, TimeSpan timespan, string fmt, params object[] vars)
    {
        TraceApi(componentName, method, timespan, string.Format(fmt, vars));
    }
    public void TraceApi(string componentName, string method, TimeSpan timespan, string properties)
    {
        string message = String.Concat("Component:", componentName, ";Method:", method, ";Timespan:", timespan.ToString(), ";Properties:", properties);
        Console.WriteLine(message);
    }

    private static string FormatExceptionMessage(Exception exception, string fmt, object[] vars)
    {
        var sb = new StringBuilder();

        sb.Append(string.Format(fmt, vars));
        sb.Append(" Exception: ");
        sb.Append(exception.ToString());
        return sb.ToString();
    }
}
