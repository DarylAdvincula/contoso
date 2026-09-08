using System.Data.Common;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ContosoUniversity.Logging;

namespace ContosoUniversity.Data;

public class SchoolInterceptorTransientErrors : DbCommandInterceptor
{
    private int _counter = 0;
    private readonly Logging.ILogger _logger = new Logger();

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result
    )
    {
        ProcessCommand(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result, 
        CancellationToken cancellationToken = default
    )
    {
        ProcessCommand(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        ProcessCommand(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<object> result, 
        CancellationToken cancellationToken = default
    )
    {
        ProcessCommand(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<int> result
    )
    {
        ProcessCommand(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default
    )
    {
        ProcessCommand(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void ProcessCommand(DbCommand command)
    {
        bool throwTransientErrors = false;

        foreach (DbParameter parameter in command.Parameters)
        {
            if (parameter.DbType == System.Data.DbType.String ||
                parameter.DbType == System.Data.DbType.StringFixedLength)
            {
                // set throw transient errors to true if "Throw" keyword was detected in the command parameters
                // replace it with "an, it will show results of students with names that contain the substring "an"
                if (parameter.Value?.ToString() == "%Throw%")
                {
                    throwTransientErrors = true;
                    parameter.Value = "%an%";
                }
            }
        }

        // throws exception 4 times
        // before showing the results for the keyword "an"
        if (throwTransientErrors && _counter < 4)
        {
            _logger.Information("Returning transient error for command: {0}", command.CommandText);
            _counter++;

            throw CreateDummySqlException();
        }
    }

    private SqlException CreateDummySqlException()
    {
        int sqlErrorNumber = 20;

        var sqlErrorCtor = typeof(SqlError)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(c => c.GetParameters().Length == 8);

        var sqlError = sqlErrorCtor.Invoke(new object[] { sqlErrorNumber, (byte)0, (byte)0, "", "", "", 1, null! });

        var errorCollection = Activator.CreateInstance(typeof(SqlErrorCollection), true);

        var addMethod = typeof(SqlErrorCollection)
            .GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic);

        addMethod!.Invoke(errorCollection, new[] { sqlError });

        // Modern SqlException requires a 4-parameter constructor
        var sqlExceptionCtor = typeof(SqlException)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(c => c.GetParameters().Length == 4);

        var sqlException = (SqlException)sqlExceptionCtor.Invoke(new object[] 
        { 
            "Dummy Transient Error", 
            errorCollection, 
            null, 
            Guid.NewGuid() 
        });

        return sqlException;
    }
}
