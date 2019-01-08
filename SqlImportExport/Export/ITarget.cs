using System.Collections.Generic;

namespace SqlImportExport.Export
{
    interface ITarget
    {
        string FormattedInsert(IRows rows, IEnumerable<IColumn> columns);
    }
}
