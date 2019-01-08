using SqlImportExport.Export;
using SqlImportExport.Import;
using System.Collections.Generic;

namespace SqlImportExport
{
    public interface IResolver
    {
        IGenerator GetGenerator(string connectionString, string table, int maxRows);

        IImporter GetImporter(string connectionString);

        IEnumerable<IReference> GetReferences(IEnumerable<(string, IEnumerable<string>)> references);
    }
}