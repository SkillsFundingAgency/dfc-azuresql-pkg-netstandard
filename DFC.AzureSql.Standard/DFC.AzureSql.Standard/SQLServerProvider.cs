using System;
using System.Data;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;

namespace DFC.AzureSql.Standard
{
    public class SQLServerProvider : ISQLServerProvider
    {
        private readonly ILoggerHelper _loggerHelper;
        private readonly IDbConnection _dbConnection;
        private readonly Guid _correlationId = Guid.NewGuid();

        public SQLServerProvider(ILoggerHelper loggerHelper, IDbConnection dbConnection)
        {
            _loggerHelper = loggerHelper;
            _dbConnection = dbConnection;
        }

        public async Task<bool> UpsertResource(Document document, ILogger log, string commandText, string parameterName)
        {
            try
            {
                _loggerHelper.LogMethodEnter(log);

                await Task.Run(() => Execute(document, log, commandText, parameterName));

                _loggerHelper.LogMethodExit(log);
                return true;
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(log, _correlationId, ex);
                return false;
            }
        }

        private void Execute(Document document, ILogger log, string commandText, string parameterName)
        {
            using (_dbConnection)
            {
                using (var dbCommand = BuildCommand(commandText))
                {
                    try
                    {
                        _dbConnection.Open();
                        dbCommand.Parameters.Add(BuildParameter(dbCommand, document, parameterName));
                        dbCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _loggerHelper.LogException(log, _correlationId, ex);
                    }
                    finally
                    {
                        _dbConnection.Close();
                    }
                }
            }
        }

        private IDbDataParameter BuildParameter(IDbCommand command, Document document, string parameterName)
        {
            var dbParameter = command.CreateParameter();
            dbParameter.ParameterName = parameterName;            
            dbParameter.Direction = ParameterDirection.Input;
            dbParameter.Value = document.ToString();

            return dbParameter;
        }

        private IDbCommand BuildCommand(string commandText)
        {
            var result = _dbConnection.CreateCommand();
            result.CommandType = CommandType.StoredProcedure;
            result.CommandText = commandText;

            return result;
        }
    }
}
