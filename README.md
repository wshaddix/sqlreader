sqlreader
=========

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
    public void Can_Do_Basic_Get_With_Default_Connection_String()
    {
        const string sql = "select top 1 * from dbo.ScriptTable";
        var reader = new SqlReader();
        var result = reader.Get(sql);

        Assert.That(result.Id != null);
        Assert.That(result.Script != null);
        Assert.That(result.Release != null);
        Assert.That(result.AppliedOnUtc != null);
    }
```
