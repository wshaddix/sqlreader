
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;

namespace SqlReader
{
    public class SqlReader
    {
        private string _connectionString;

        /// <summary>
        /// Initializes a new instance of a SqlReader. This will use the first connection string found in the .config file
        /// </summary>
        public SqlReader()
        {

        }

        /// <summary>
        /// Initializes a new instance of a SqlReader. This will use the connection string passed in the constructor.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the sql server</param>
        public SqlReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Runs a sql query against the database for the currently configured connection string and returns a
        /// single dynamic object.
        /// </summary>
        /// <param name="sqlScript">The sql script to run</param>
        /// <param name="args">The parameter values to use for the sql script</param>
        /// <returns></returns>
        public dynamic Get(string sqlScript, params object[] args)
        {
            // format the sql script into a valid sql statement
            var sqlStatement = string.Format(sqlScript, args);

            // execute the sql statement
            return ExecuteSql(sqlStatement)[0];
        }

        /// <summary>
        /// Runs a sql query against the database for the currently configured connection string and returns a
        /// list of dynamic objects.
        /// </summary>
        /// <param name="sqlScript">The sql script to run</param>
        /// <param name="args">The parameter values to use for the sql script</param>
        /// <returns></returns>
        public IList<dynamic> List(string sqlScript, params object[] args)
        {
            // format the sql script into a valid sql statement
            var sqlStatement = string.Format(sqlScript, args);

            // execute the sql statement
            return ExecuteSql(sqlStatement);
        }

        private IList<dynamic> ExecuteSql(string sqlStatement)
        {
            IList<dynamic> results;

            // ensure that we have a connection string to use
            EnsureConnectionStringIsValid();

            // connect to sql server and map results
            using (var connection = new SqlConnection(_connectionString))
            {
                // open the connection to the server
                connection.Open();

                var cmd = new SqlCommand(sqlStatement, connection);

                // execute the sql statement
                var reader = cmd.ExecuteReader();

                // map the results to dynamic objects
                results = MapReaderToDynamic(reader);

                // close the reader
                reader.Close();

            } // <== closes the connection to sql server

            return results;
        }

        private IList<dynamic> MapReaderToDynamic(SqlDataReader reader)
        {
            dynamic expando = new ExpandoObject();
            var results = new List<dynamic>();

            // loop through all the records
            while (reader.Read())
            {
                // cast the dynamic object as a dictionary so that we can assign name/values
                var e = expando as IDictionary<string, object>;

                // for every column in the result set we'll add a property to the dynamic object
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    e[reader.GetName(i)] = reader[i];
                }

                // add the dynamic object to the results list
                results.Add(e);
            }

            return results;
        }

        private void EnsureConnectionStringIsValid()
        {
            if (!string.IsNullOrEmpty(_connectionString)) return;

            // grab the first connection string from the web.config
            // if there isn't one then throw an error
            if (ConfigurationManager.ConnectionStrings.Count == 0)
            {
                throw new ConfigurationErrorsException("There is no connection string configured for your database.");
            }

            _connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;
        }
    }
}
