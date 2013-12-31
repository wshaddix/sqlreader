using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SqlReader
{
    public class DataReader
    {
        private string _connectionString;

        /// <summary>
        /// Initializes a new instance of a DataReader. This will use the first connection string
        /// found in the .config file
        /// </summary>
        public DataReader()
        {
        }

        /// <summary>
        /// Initializes a new instance of a DataReader. This will use the connection string passed in
        /// the constructor.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the sql
        /// server</param>
        public DataReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Runs a sql query against the database for the currently configured connection string and
        /// returns a single dynamic object.
        /// </summary>
        /// <param name="sqlScript">The sql script to run</param>
        /// <param name="args">The parameter values to use for the sql script</param>
        /// <returns></returns>
        public T Get<T>(string sqlScript, params object[] args) where T : class, new()
        {
            // format the sql script into a valid sql statement
            var sqlStatement = string.Format(sqlScript, args);

            // execute the sql statement
            var results = ExecuteSql<T>(sqlStatement);

            if (null != results && results.Count > 0)
            {
                return results[0];
            }

            return null;
        }

        /// <summary>
        /// Runs a sql query against the database for the currently configured connection string and
        /// returns a list of dynamic objects.
        /// </summary>
        /// <param name="sqlScript">The sql script to run</param>
        /// <param name="args">The parameter values to use for the sql script</param>
        /// <returns></returns>
        public IList<T> List<T>(string sqlScript, params object[] args) where T : new()
        {
            // format the sql script into a valid sql statement
            var sqlStatement = string.Format(sqlScript, args);

            // execute the sql statement
            return ExecuteSql<T>(sqlStatement);
        }

        private void EnsureConnectionStringIsValid()
        {
            if (!string.IsNullOrEmpty(_connectionString)) return;

            // grab the first connection string from the web.config if there isn't one then throw an
            // error
            if (ConfigurationManager.ConnectionStrings.Count == 0)
            {
                throw new ConfigurationErrorsException("There is no connection string configured for your database.");
            }

            _connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;
        }

        private IList<T> ExecuteSql<T>(string sqlStatement) where T : new()
        {
            IList<T> results;

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
                results = MapReaderToDynamic<T>(reader);

                // close the reader
                reader.Close();
            } // <== closes the connection to sql server

            return results;
        }

        private static IList<T> MapReaderToDynamic<T>(IDataReader reader) where T : new()
        {
            var results = new List<T>();

            // loop through all the records
            while (reader.Read())
            {
                // create a new instance of T to populate and add to the list
                var item = new T();

                // create a new object mapper for T so that we can map the recordset results to
                // properties
                var mapper = new ObjectMapper<T>(item);

                // for every column in the result set we'll add a property to the dynamic object
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    mapper.Map(reader.GetName(i), reader[i]);
                }

                // add the dynamic object to the results list
                results.Add(item);
            }

            return results;
        }
    }
}