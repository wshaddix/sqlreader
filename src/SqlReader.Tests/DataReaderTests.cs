using NUnit.Framework;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SqlReader.Tests
{
    [TestFixture]
    public class DataReaderTests
    {
        private readonly Random _random = new Random();
        private string _masterDbConnectionString;
        private string _testDbConnectionString;

        [Test]
        public void Can_Do_Basic_Get()
        {
            const string sql = "select top 1 * from person";
            var reader = new DataReader(_testDbConnectionString);
            var person = reader.Get<Person>(sql);

            Assert.That(!string.IsNullOrEmpty(person.FirstName), "First name is empty");
            Assert.That(!string.IsNullOrEmpty(person.LastName), "Last name is empty");
            Assert.That(!string.IsNullOrEmpty(person.City), "City is empty");
            Assert.That(person.Age > 0, "Age is 0");
        }

        [Test]
        public void Can_Do_Basic_List()
        {
            const string sql = "select  * from person";
            var reader = new DataReader(_testDbConnectionString);
            var personList = reader.List<Person>(sql);

            Assert.That(personList.Count == 52, "Didn't get back 52 records");
            Assert.That(personList.Any(p => p.FirstName.Equals("First")), "Didn't contain specific person with first name 'First'");
        }

        [Test]
        public void Can_Do_Specific_Get()
        {
            const string sql = "select * from person where FirstName = 'First'";
            var reader = new DataReader(_testDbConnectionString);
            var person = reader.Get<Person>(sql);

            Assert.That(person.FirstName.Equals("First"), "First name is not 'First'");
            Assert.That(person.LastName.Equals("Last"), "Last name is not 'Last'");
            Assert.That(person.City.Equals("City"), "City is not 'City'");
            Assert.That(person.Age == 123, "Age is not '123'");
        }

        [Test]
        public void Can_Get_Null_Back_If_Query_Has_No_Match()
        {
            const string sql = "select * from person where FirstName = 'NotInTheDatabase'";
            var reader = new DataReader(_testDbConnectionString);
            var person = reader.Get<Person>(sql);

            Assert.That(null == person, "person instance is not null");
        }

        [Test]
        public void Can_Handle_Null_Values()
        {
            const string sql = "select * from person where FirstName = 'FirstNull'";
            var reader = new DataReader(_testDbConnectionString);
            var person = reader.Get<Person>(sql);

            Assert.That(person.FirstName.Equals("FirstNull"), "First name is not 'FirstNull'");
            Assert.That(string.IsNullOrEmpty(person.LastName), "Last name is not null");
            Assert.That(person.City.Equals("City"), "City is not 'City'");
            Assert.That(person.Age == 123, "Age is not '123'");
        }

        [Test]
        public void Can_Query_Using_First_Connection_String_When_Not_Specified()
        {
            const string sql = "select * from person where FirstName = 'First'";
            var reader = new DataReader();
            var person = reader.Get<Person>(sql);

            Assert.That(person.FirstName.Equals("First"), "First name is not 'First'");
            Assert.That(person.LastName.Equals("Last"), "Last name is not 'Last'");
            Assert.That(person.City.Equals("City"), "City is not 'City'");
            Assert.That(person.Age == 123, "Age is not '123'");
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            GetConnectionStrings();
            CreateDatabase();
            CreateDataTable();
            SeedDatabase();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            DestroyDatabase();
        }

        private void CreateDatabase()
        {
            const string sql = "Create Database TestDb";
            RunSql(sql, _masterDbConnectionString);
        }

        private void CreateDataTable()
        {
            const string sql = "Create table Person(FirstName nvarchar(50), LastName nvarchar(50), City nvarchar(50), Age int)";
            RunSql(sql, _testDbConnectionString);
        }

        private void DestroyDatabase()
        {
            const string sql = "alter database TestDb set offline with rollback immediate; alter database Testdb set online; drop database TestDb;";
            RunSql(sql, _masterDbConnectionString);
        }

        private int GetAge()
        {
            return GetRandomNumber(20, 95);
        }

        private string GetCity()
        {
            var cities = new[] { "Andersonville", "Blairsville", "Columbus", "Dawsonvile", "Elberton", "Freedmont", "Garrison", "Harlem", "Indianapolis", "Jonesburough" };
            var position = GetRandomNumber(0, 10);
            return cities[position];
        }

        private void GetConnectionStrings()
        {
            _masterDbConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MasterDb"].ConnectionString;
            _testDbConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
        }

        private string GetFirstName()
        {
            var names = new[] { "Alex", "Bill", "Chad", "Derrick", "Evan", "Fred", "Gary", "Harrison", "Ivan", "Jesse" };
            var position = GetRandomNumber(0, 10);
            return names[position];
        }

        private string GetLastName()
        {
            var names = new[] { "Abercrombie", "Bates", "Carleson", "DeMarcurio", "Ellis", "Friedman", "Smith", "Liberty", "Anderson", "Shaddix" };
            var position = GetRandomNumber(0, 10);
            return names[position];
        }

        private int GetRandomNumber(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        private void RunSql(string sql, string connectionString)
        {
            using (var db = new SqlConnection(connectionString))
            {
                db.Open();
                var cmd = db.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                db.Dispose();
            }
        }

        private void SeedDatabase()
        {
            var sql = "insert into person (FirstName, LastName, City, Age) values ('{0}', '{1}', '{2}', {3})";

            for (var i = 0; i < 50; i++)
            {
                var executeSql = string.Format(sql, GetFirstName(), GetLastName(), GetCity(), GetAge());
                RunSql(executeSql, _testDbConnectionString);
            }

            // we need one well known record to test the fetching of specific data
            sql = "insert into person(FirstName, LastName, City, Age) values ('First', 'Last', 'City', 123)";
            RunSql(sql, _testDbConnectionString);

            // we need one well known record to test the fetching of null data
            sql = "insert into person(FirstName, LastName, City, Age) values ('FirstNull', null, 'City', 123)";
            RunSql(sql, _testDbConnectionString);
        }

        public class Person
        {
            public int Age { get; set; }

            public string City { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}