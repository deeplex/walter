using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Deeplex.Saverwalter.Model
{
    public class SaverWalterContextFactory : IDesignTimeDbContextFactory<SaverwalterContext>
    {
        public SaverwalterContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseNpgsql(
                 $@"Server=127.0.0.1
                ;Database=saverdesign");

            return new SaverwalterContext(optionsBuilder.Options);
        }
    }
}
