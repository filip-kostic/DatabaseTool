# Database Tool
Tool for exporting and importing database tables

Currently supported:

	EXPORT
		SQL Server (sqlserver)
	
	IMPORT
		PostgreSQL (postgresql)
		
Setup export:

1. Open App.config in DatabaseTool project or DatabaseTool.exe.config in the compiled version
2. In <appSettings> set "from" to the type of database you are exporting from
3. In <connectionStrings> set "ExportConnectionString" to the database you are targeting

Setup import:

1. Open App.config in DatabaseTool project or DatabaseTool.exe.config in the compiled version
3. In <appSettings> set "to" to the type of database you are importing to
3. In <connectionStrings> set "ImportConnectionString" to the database you are targeting

Usage:

	export
		table     [required] - Table to be exported (fully qualified, with schema)
		maxRows   [optional] - Maximum number of rows per file (defaults to 100.000)
		condition [optional] - Condition for exporting the rows
		path      [optional] - Path to the files to be imported (defaults to the Export directory of databasetool location)
		ref       [optional] - Reference unique column(s) for a given foreign key column (this will replace a value with select query)
		
	import
		table     [required] - Table for the file(s) to be imported (best for files exported using this tool, guarantees order of execution)
		path      [optional] - Path to the files to be imported (defaults to the Export directory of sqlexport location)
		
Export examples:
	1. The simplest command from sqlserver to postgresql:
		>> databasetool export table=dbo.SampleTable
	
		Should result in file dbo.SampleTable_001.sql:
			INSERT INTO dbo.SampleTable (NumCol,StrCol,DateCol,MoneyCol,XmlCol,RefId) VALUES
			(1,'Value 01','20-Nov-17 12:31:02 PM',1236.8900,'<root>text</root>',NULL),
			(2,NULL,'16-Dec-18 00:00:00 PM',0.0000,'<root><item>other text</item><item>sample text</item></root>',123),
			...
			ON CONFLICT (NumCol) DO NOTHING;
			
		Any rows past 100.000 (default) will end up in dbo.SampleTable_002.sql and so on.
		
	2. Command with a condition:
		>> databasetool export table=dbo.SampleTable maxRows=1000 condition="DateCol BETWEEN '2018-01-01' AND '2018-12-31'"
		
		or
		
		>> databasetool export table=dbo.SampleTable maxRows=10000 condition="EXISTS (SELECT 0 FROM dbo.OtherTable AS ot WHERE ot.SampleTableId = NumCol)"
		
	3. Command with replacing a foreign key with a select statement:
		>> databasetool export table=dbo.SampleTable maxRows=1000 ref=dbo.ReferencedTable:StaticDataIdentifier
			
		Should result in rows like:
			(2,NULL,'16-Dec-18 00:00:00 PM',0.0000,'<root><item>other text</item><item>sample text</item></root>',(SELECT rfr.Id FROM dbo.ReferencedTable AS rfr WHERE rfr.StaticDataIdentifier = 'fc3ca7e5-9a6b-4e68-945d-0000e9af7536'),
			...
			
		or
		
		>> databasetool export table=dbo.SampleTable maxRows=1000 ref=dbo.ReferencedTable:StaticDataIdentifier,Type
		
		Should result in rows like:
			Should result in rows like:
			(2,NULL,'16-Dec-18 00:00:00 PM',0.0000,'<root><item>other text</item><item>sample text</item></root>',(SELECT rfr.Id FROM dbo.ReferencedTable AS rfr WHERE rfr.StaticDataIdentifier = 'fc3ca7e5-9a6b-4e68-945d-0000e9af7536' AND rfr.Type = 3),
			...
			
Import examples:

	1. The simplest import using files generated with export functionality:
		>> databasetool import table=dbo.SampleTable
		
		Should iterate through files generated using the export functionality and execute them sequentially.
