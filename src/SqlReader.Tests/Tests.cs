using NUnit.Framework;

namespace SqlReader.Tests
{
    [TestFixture]
    public class Tests
    {
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

        [Test]
        public void Can_Do_Basic_Get_With_Default_Connection_String_And_Where_Clause()
        {
            const string sql = "select * from dbo.ScriptTable where Id = '{0}'";
            var reader = new SqlReader();
            var result = reader.Get(sql, "20130717145005521");

            Assert.That(result.Id.Equals("20130717145005521"));
            Assert.That(result.Script != null);
            Assert.That(result.Release != null);
            Assert.That(result.AppliedOnUtc != null);
        }

        [Test]
        public void Can_Do_Basic_List_With_Default_Connection_String()
        {
            const string sql = "select * from dbo.ScriptTable";
            var reader = new SqlReader();
            var result = reader.List(sql);

            Assert.That(result.Count == 2);
        }
    }
}
