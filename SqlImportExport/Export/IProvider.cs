using System.Collections.Generic;

namespace SqlImportExport.Export
{
    interface IProvider
    {
        IEnumerable<IColumn> GetColumnDescriptions();

        int GetCount(string condition);

        IRows GetRows(string condition, int startRow, int endRow, IEnumerable<IColumn> columns, IEnumerable<IReference> references);
    }
}