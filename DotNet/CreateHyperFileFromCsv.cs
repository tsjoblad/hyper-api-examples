// -----------------------------------------------------------------------------
//
// This file is the copyrighted property of Tableau Software and is protected
// by registered patents and other applicable U.S. and international laws and
// regulations.
//
// You may adapt this file and modify it to fit into your context and use it
// as a template to start your own projects.
//
// -----------------------------------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;

using Tableau.HyperAPI;

namespace Example
{
    internal class CreateHyperFileFromCsv : Example
    {
        /// <summary>
        /// Create a new Hyper file with a single table and load data from a CSV file into it.
        /// </summary>
        /// <param name="exampleDataDir">Path to the directory with example data.</param>
        public override void Execute(string exampleDataDir)
        {
            Console.WriteLine("EXAMPLE - Load data from a CSV file into a table in a new Hyper file.");

            // Optional process parameters. They are documented in the Tableau Hyper documentation, chapter "Process Settings"
            // (https://help.tableau.com/current/api/hyper_api/en-us/reference/sql/processsettings.html).
            var processParameters = new Dictionary<string, string>
            {
                // Limits the number of Hyper event log files to two.
                { "log_file_max_count", "2" },
                // Limits the size of Hyper event log files to 100 megabytes.
                { "log_file_size_limit", "100M" }
            };

            // Start the Hyper process with telemetry enabled.
            using (HyperProcess hyper = new HyperProcess(Telemetry.SendUsageDataToTableau, "example", processParameters))
            {
                // Optional connection parameters. They are documented in the Tableau Hyper documentation, chapter "Connection Settings"
                // (https://help.tableau.com/current/api/hyper_api/en-us/reference/sql/connectionsettings.html).
                var connectionParameters = new Dictionary<string, string>
                {
                    { "lc_time", "en_US" }
                };

                // Connect to Hyper and create new Hyper file "customer.hyper".
                // It replaces the file if it already exists when CreateMode.CreateAndReplace is set.
                using (Connection connection = new Connection(hyper.Endpoint, "customer.hyper", CreateMode.CreateAndReplace, connectionParameters))
                {
                    // Table definition - its name and the list of columns.
                    // Since the table name is not prefixed with an explicit schema name, the table will reside in the default "public" namespace.
                    TableDefinition customerTable = new TableDefinition("Customer")
                       .AddColumn("Customer ID", SqlType.Text(), Nullability.NotNullable)
                       .AddColumn("Customer Name", SqlType.Text(), Nullability.NotNullable)
                       .AddColumn("Loyalty Reward Points", SqlType.BigInt(), Nullability.NotNullable)
                       .AddColumn("Segment", SqlType.Text(), Nullability.NotNullable);

                    // Create the table in the Hyper file.
                    connection.Catalog.CreateTable(customerTable);

                    string pathToCsv = Path.Join(exampleDataDir, "customers.csv");

                    // Load all rows into "Customers" table from the CSV file.
                    // ExecuteCommand executes a SQL statement and returns the impacted row count.
                    // TableDefinition.Name property is a QualifiedName object which is escaped properly when
                    // converted to a string; but the path to the CSV file needs to be escaped.
                    int countInCustomerTable = connection.ExecuteCommand(
                        $"COPY {customerTable.TableName} from {Sql.EscapeStringLiteral(pathToCsv)} with " +
                        $"(format csv, NULL 'NULL', delimiter ',', header)");

                    Console.WriteLine($"The number of rows in table {customerTable.TableName} is {countInCustomerTable}");
                }

                Console.WriteLine("The connection to the Hyper file has been closed.");
            }

            Console.WriteLine("The Hyper process has been shut down.");
        }
    }
}
