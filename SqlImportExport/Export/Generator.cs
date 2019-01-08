using System.Collections.Generic;

namespace SqlImportExport.Export
{
    class Generator : IGenerator
    {
        int MaxRows { get; }
        IProvider Provider { get; }
        ITarget Target { get; }

        public Generator(int maxRows, IProvider provider, ITarget target)
        {
            MaxRows = maxRows;
            Provider = provider;
            Target = target;
        }

        public IEnumerable<string> Generate(string connectionString, string tableName, string condition, IEnumerable<IReference> references)
        {
            var columns = Provider.GetColumnDescriptions();

            var count = Provider.GetCount(condition);

            for (int i = 1; i <= count; i += MaxRows)
            {
                var rows = Provider.GetRows(condition, i, i + MaxRows - 1, columns, references);
                yield return Target.FormattedInsert(rows, columns);
            }
        }
    }
}
