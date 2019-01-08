namespace SqlImportExport.Export
{
    class RawCell : IField
    {
        public RawCell(string name, object data)
        {
            Name = name;
            Value = data.ToString();
        }

        public string Name { get; }

        public string Value { get; }
    }
}