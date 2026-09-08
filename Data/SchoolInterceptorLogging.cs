using ContosoUniversity.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace ContosoUniversity.Data;

public class SchoolInterceptorLogging : DbCommandInterceptor
{
    private Logging.ILogger _logger = new Logger();

    // handle successful sync retrieval command execution (SELECT, but for only one result) interceptions
    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        _logger.TraceApi(
            "SQL Database",
            "SchoolInterceptor.ScalarExecuted",
            eventData.Duration,  // execution duration (time elapsed)
            "Command: {0}",
            command.CommandText);

        return base.ScalarExecuted(command, eventData, result);
    }

    // handle successful async retrieval command execution (SELECT, but for only one result) interceptions
    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default
    )
    {
        _logger.TraceApi(
            "SQL Database",
            "SchoolInterceptor.ScalarExecutedAsync",
            eventData.Duration, // execution duration (time elapsed)
            "Command: {0}",
            command.CommandText
        );

        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result
    )
    {
        _logger.TraceApi(
            "SQL Database", 
            "SchoolInterceptor.NonQueryExecuted", 
            eventData.Duration, 
            "Command: {0}: ", 
            command.CommandText
         );

        return base.NonQueryExecuted(command, eventData, result);
    }

    // handle successful async non-retrieval command (INSERT, UPDATE, DELETE, ...) execution interceptions
    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        _logger.TraceApi(
            "SQL Database",
            "SchoolInterceptor.NonQueryExecutedAsync",
            eventData.Duration,
            "Command: {0}: ",
            command.CommandText
        );

        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    // handle successful sync retrieval command execution (SELECT, but could return multiple rows) interceptions
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result
    )
    {
        _logger.TraceApi(
            "SQL Database",
            "SchoolInterceptor.ReaderExecuted",
            eventData.Duration,
            "Command: {0}: ",
            command.CommandText);

        return base.ReaderExecuted(command, eventData, result);
    }

    // handle successful async retrieval command execution (SELECT, but could return multiple rows) interceptions
    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        _logger.TraceApi(
            "SQL Database",
            "SchoolInterceptor.ReaderExecutedAsync",
            eventData.Duration,
            "Command: {0}: ",
            command.CommandText);

        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    // global execution exceptions logging
    public override void CommandFailed(
        DbCommand command, 
        CommandErrorEventData eventData
    )
    {
        if (eventData.Exception != null)
        {
            _logger.Error(
                eventData.Exception,
                "Error executing command: {0}",
                command.CommandText
            );
        }

        base.CommandFailed(command, eventData);
    }
}
