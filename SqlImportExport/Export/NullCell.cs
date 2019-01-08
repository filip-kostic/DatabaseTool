namespace SqlImportExport.Export
{
    class NullCell : IField
    {
        public string Name => "Irrelevant";

        public string Value => "NULL";
    }
}