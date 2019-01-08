using SqlImportExport;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DatabaseTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter valid arguments. Try \"sqlexport ?\" for help.");
                return;
            }

            switch (args[0])
            {
                case "export":
                    Export(args);
                    break;
                case "import":
                    Import(args);
                    break;
                case "?":
                default:
                    PrintHelp();
                    break;
            }
        }

        static void Export(string[] args)
        {
            var resolver = new Resolver(Providers.From, Providers.To);

            var arguments = GetExportArguments(args);
            var connectionString = ConfigurationManager.ConnectionStrings["ExportConnectionString"].ConnectionString;
            var generator = resolver.GetGenerator(connectionString, arguments.Table, arguments.MaxRows);

            int fileNumber = 1;
            foreach (var segment in generator.Generate(connectionString, arguments.Table, arguments.Condition, resolver.GetReferences(arguments.References)))
            {
                string fileName = GetFileName(arguments.Table, fileNumber);

                Console.WriteLine($"Exporting table: {arguments.Table};\tRows {1 + (fileNumber - 1) * arguments.MaxRows} - {fileNumber * arguments.MaxRows};\tFile: {fileName};");

                Output(arguments.Path, fileName, segment);

                fileNumber++;
            }
        }

        static (string Table, int MaxRows, string Condition, string Path, IEnumerable<(string, IEnumerable<string>)> References) GetExportArguments(string[] args)
            => (GetArgument(args, "table"),
                Convert.ToInt32(GetArgument(args, "maxRows", "100000")),
                GetArgument(args, "condition"),
                GetArgument(args, "path", GetRootPath()),
                GetArguments(args, "ref"));

        static (string From, string To) Providers
            => (ConfigurationManager.AppSettings["from"].ToString(),
                ConfigurationManager.AppSettings["to"].ToString());

        static void Output(string path, string fileName, string data)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText($"{path}\\{fileName}", data);
        }

        static void Import(string[] args)
        {
            var arguments = GetImportArguments(args);
            var connectionString = ConfigurationManager.ConnectionStrings["ImportConnectionString"].ConnectionString;
            var importer = new Resolver(Providers.From, Providers.To).GetImporter(connectionString);

            var files = Directory.GetFiles(arguments.Path, $"{arguments.Table}*.sql");
            for (int i = 0; i < files.Length; ++i)
            {
                Console.WriteLine($"Importing file {i + 1}/{files.Length};\t{files[i]};");
                try
                {
                    string data = File.ReadAllText(files[i]);
                    importer.Execute(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.InnerException?.Message ?? string.Empty);
                }
            }
        }

        static (string Table, string Path) GetImportArguments(string[] args)
            => (GetArgument(args, "table"),
                GetArgument(args, "path", GetRootPath()));

        static string GetArgument(string[] args, string argumentName, string defaultValue = null)
        {
            var defVal = string.IsNullOrEmpty(defaultValue) ? string.Empty : defaultValue;
            var argumentPart = $"{argumentName}=";
            return args.FirstOrDefault(x => x.Contains(argumentPart))?.Replace(argumentPart, string.Empty) ?? defVal;
        }

        static IEnumerable<(string, IEnumerable<string>)> GetArguments(string[] args, string argumentName)
            => args
                .Where(x => x.StartsWith("ref="))
                .Select(x => x.Replace("ref=", string.Empty))
                .Select(x =>
                {
                    var parts = x.Split(':');
                    return (parts[0], parts[1].Split(',').AsEnumerable());
                });

        static string GetFileName(string table, int fileNumber)
            => $"{table}_{fileNumber.ToString("000")}.sql";

        static string GetRootPath()
        {
            string rootPath = Assembly.GetExecutingAssembly().Location;
            return $"{rootPath.Substring(0, rootPath.LastIndexOf('\\'))}\\Export";
        }

        static void PrintHelp()
        {
            var sb = new StringBuilder();

            sb.AppendLine("export - Generates import script to file(s)");
            sb.AppendLine("\ttable\t\t- Table to be exported (fully qualified, with schema) [required]");
            sb.AppendLine("\tmaxRows\t\t- Maximum number of rows per file (defaults to 100.000) [optional]");
            sb.AppendLine("\tcondition\t- Condition for exporting the rows [optional]");
            sb.AppendLine("\tpath\t\t- Path to the files to be imported (defaults to the Export directory of databasetool location) [optional]");
            sb.AppendLine("\tref\t\t- Reference unique column(s) for a given foreign key column (this will replace a value with select query) [optional]");
            sb.AppendLine("\tEXAMPLE: databasetool export table=dbo.Unit");
            sb.AppendLine("\tEXAMPLE: databasetool export table=dbo.Unit maxRows=1000 condition=\"IsActive = 1\"");
            sb.AppendLine("\tEXAMPLE: databasetool export table=dbo.PageInfoUnitRelation maxRows=10000 ref=dbo.Unit:UnitNativePMSID ref=dbo.PageInfo:StaticDataIdentifier,Type");

            sb.AppendLine("import - Imports sql data from file(s); Meant to be used on files generated using export command");
            sb.AppendLine("\ttable\t\t- Table for the file(s) to be imported (best for files exported using this tool, guarantees order of execution) [required]");
            sb.AppendLine("\tpath\t\t- Path to the files to be imported (defaults to the Export directory of sqlexport location) [optional]");
            sb.AppendLine("\tEXAMPLE: sqlexport import table=dbo.Unit");

            Console.WriteLine(sb.ToString());
        }
    }
}
