using System.Collections.Generic;

namespace SqlImportExport.Export
{
    public interface IGenerator
    {
        IEnumerable<string> Generate(string connectionString, string tableName, string condition, IEnumerable<IReference> references);
    }
}