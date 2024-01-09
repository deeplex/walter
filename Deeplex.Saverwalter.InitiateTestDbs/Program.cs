// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.InitiateTestDbs.Templates;

namespace Deeplex.Saverwalter.InitiateTestDbs
{
    internal class Program
    {
        static async Task Main()
        {
            var databaseUser = Environment.GetEnvironmentVariable("DATABASE_USER");
            if (databaseUser == null)
            {
                Console.WriteLine("Can't find environment variable DATABASE_USER");
            }

            var databasePass = Environment.GetEnvironmentVariable("DATABASE_PASS");
            if (databaseUser == null)
            {
                Console.WriteLine("Can't find environment variable DATABASE_PASS");
            }

            var databaseHost = Environment.GetEnvironmentVariable("DATABASE_HOST");
            if (databaseUser == null)
            {
                Console.WriteLine("Can't find environment variable DATABASE_HOST");
            }

            var databasePort = Environment.GetEnvironmentVariable("DATABASE_PORT");
            if (databaseUser == null)
            {
                Console.WriteLine("Can't find environment variable DATABASE_PORT");
            }

            await GenericDatabase.ConnectAndPopulate(
                databaseHost!,
                databasePort!,
                "walter_dev_generic_db",
                databaseUser!,
                databasePass!,
                10);

            await GenericDatabase.ConnectAndPopulate(
                databaseHost!,
                databasePort!,
                "walter_dev_full_generic_db",
                databaseUser!,
                databasePass!,
                100);
        }
    }
}
