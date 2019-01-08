using System.Collections.Generic;
using System.Data;

namespace SqlImportExport.Export
{
    interface IFieldFactory
    {
        IField CreateReference(IDataReader reader, string referencedColumn, string column);

        IField Create(IDataReader reader, IColumn column, IEnumerable<IField> references);
    }
}