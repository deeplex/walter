using Deeplex.Saverwalter.Model;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace Deeplex.Saverwalter.ModelTests
{
    public class Class1
    {
        [Fact]
        public void AddNewEntity()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<SaverwalterContext>()
                .UseInMemoryDatabase(databaseName: "test_database")
            .Options;

            using var context = new SaverwalterContext(options);
            var entity = new NatuerlichePerson { Nachname = "Test Entity" };

            // Act
            var result = context.Add(entity);
            context.SaveChanges();

            // Assert
            result.Should().BeEquivalentTo(entity, options => options.Excluding(e => e.NatuerlichePersonId));
            context.Set<NatuerlichePerson>().Should().Contain(entity);
        }

    }
}
