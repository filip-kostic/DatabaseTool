using SqlImportExport.Export;
using SqlImportExport.Export.PostgreSQL;
using SqlImportExport.Export.SqlServer;
using SqlImportExport.Import;
using SqlImportExport.Import.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlImportExport
{
    public class Resolver : IResolver
    {
        public Resolver(string from, string to)
        {
            From = from;
            To = to;
        }

        string From { get; }
        string To { get; }

        IProvider GetProvider(string connectionString, string table)
        {
            switch (From)
            {
                case "sqlserver":
                    return new SqlServerProvider(connectionString, table, GetFieldFactory());
            }
            throw new NotImplementedException($"There is no registered handler for {From} provider!");
        }

        IFieldFactory GetFieldFactory()
        {
            switch (To)
            {
                case "postgresql":
                    return new PostgresFieldFactory();
            }
            throw new NotImplementedException($"There is no registered handler for {To} field factory!");
        }

        ITarget GetTarget(string table)
        {
            switch (To)
            {
                case "postgresql":
                    return new PostgresTarget(table);
            }
            throw new NotImplementedException($"There is no registered handler for {To} target!");
        }

        public IGenerator GetGenerator(string connectionString, string table, int maxRows)
        {
            var provider = GetProvider(connectionString, table);
            var target = GetTarget(table);

            return new Generator(maxRows, provider, target);
        }

        public IImporter GetImporter(string connectionString)
        {
            switch (To)
            {
                case "postgresql":
                    return new PostgresImporter(connectionString);
            }
            throw new NotImplementedException($"There is no registered handler for {To} importer!");
        }

        public IEnumerable<IReference> GetReferences(IEnumerable<(string, IEnumerable<string>)> references)
            => references.Select(x => new Reference(x.Item1, x.Item2));
    }
}
