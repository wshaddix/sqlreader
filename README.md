sqlreader
=========
[![](http://i.imgur.com/g1WHerF.png)](http://www.ndepend.com)

A dynamic read-only micro orm used by web applications to easily query an ado.net database.

# Overview
Sql Reader is a small library that is used by web applications to make it quick and simple to read data from a database. The application architecture that this was built for is a web application that is built for high scaleability by separating its actions into two groups. The first group are those actions that change the state of the system in some way and those are Commands. The next group are those actions that read or query the state of the system in some way and those are Queries. When the web ui needs to read data from the data store I didn't want to have to create a bunch of POCOs or use T4 to generate a bunch of classes that are only there to map data from the database. Sql Reader is my attempt to make reading data from the database as simple as possible.

# Goals

* Support dynamic objects such that you can just query the database for a recordset and get a dynamic object back with properties that represent the values from the result set
* Use parameterized sql script so that it's not vulnerable to sql injection attack
* Optionally use POCOs for strongly typed query support

		SqlReader.Query<Customer>(c => c.FirstName.Equals("Mark"));
   
# Installation
Install from NuGet by searching for SqlReader

# Usage
See the unit tests for basic usage but a quick example is:
```csharp
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
    public void Can_Do_Basic_List()
    {
        const string sql = "select  * from person";
        var reader = new DataReader(_testDbConnectionString);
        var personList = reader.List<Person>(sql);

        Assert.That(personList.Count == 52, "Didn't get back 52 records");
        Assert.That(personList.Any(p => p.FirstName.Equals("First")), "Didn't contain specific person with first name 'First'");
    }
```
