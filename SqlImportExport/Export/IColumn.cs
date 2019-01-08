namespace SqlImportExport.Export
{
    interface IColumn
    {
        string Name { get; set; }
        bool IsNullable { get; set; }
        string Type { get; set; }
        int Length { get; set; }
        bool IsUnique { get; set; }
        string ReferencedTable { get; set; }
        string ReferencedColumn { get; set; }
        bool IsForeignKeyColumn { get; }
        bool IsPartOfPrimaryKey { get; }
    }
}