using System.Collections.Generic;

namespace SqlImportExport.Export
{
    interface IRow : IEnumerable<IField>
    {
        string ToValueRow();
    }
}