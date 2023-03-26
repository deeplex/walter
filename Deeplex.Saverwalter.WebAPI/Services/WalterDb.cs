﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WalterDbService;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class WalterDbImpl : WalterDb
    {
        public SaverwalterContext ctx { get; set; }

        public WalterDbImpl()
        {
            DotNetEnv.Env.Load();

            var databaseURL = Environment.GetEnvironmentVariable("DATABASE_URL");
            var databasePort = Environment.GetEnvironmentVariable("DATABASE_PORT");
            var databaseUser = Environment.GetEnvironmentVariable("DATABASE_USER");
            var databasePass = Environment.GetEnvironmentVariable("DATABASE_PASS");

            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseNpgsql(
                 $@"Server={databaseURL}
                ;Port={databasePort}
                ;Database=postgres
                ;Username={databaseUser}
                ;Password={databasePass}");
         
            ctx = new SaverwalterContext(optionsBuilder.Options);

            //if (ctx.Database.EnsureCreated())
            //{
                // The database did not exist and has been created
            //}
            //else
            //{
                //ctx.Database.Migrate();
            //}
        }

        public void SaveWalter()
        {
            ctx.SaveChanges();
        }
    }
}
