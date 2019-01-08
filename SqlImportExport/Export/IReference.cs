using System.Collections.Generic;

namespace SqlImportExport.Export
{
    public interface IReference
    {
        string Table { get; }
        IEnumerable<string> Fields { get; }
    }
}
