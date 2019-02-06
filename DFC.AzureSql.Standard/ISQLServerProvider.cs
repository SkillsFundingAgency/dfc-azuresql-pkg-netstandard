using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;

namespace DFC.AzureSql.Standard
{
    public interface ISQLServerProvider
    {
        Task<bool> UpsertResource(Document document, ILogger log, string commandText, string parameterName);
    }
}
