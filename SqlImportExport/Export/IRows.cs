using System.Collections.Generic;

namespace SqlImportExport.Export
{
    interface IRows : IEnumerable<IRow>
    {
        string ToValueRows();
    }
}