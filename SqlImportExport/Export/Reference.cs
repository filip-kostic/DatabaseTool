using System.Collections.Generic;

namespace SqlImportExport.Export
{
    class Reference : IReference
    {
        public Reference(string table, IEnumerable<string> fields)
        {
            Table = table;
            Fields = fields;
        }

        public string Table { get; }

        public IEnumerable<string> Fields { get; }
    }
}
