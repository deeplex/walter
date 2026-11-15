// Copyright (c) 2023-2026 Kai Lawrence
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

using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Legt beim Start und danach in regelmäßigen Abständen die fehlenden Sollstellungen des
    /// laufenden Jahres an (<see cref="SollstellungService"/>), also auch zum Monatswechsel.
    /// Der Vorgang ist wiederholbar und schreibt nur, wenn etwas fehlt. Ein Fehler wird
    /// protokolliert; der nächste Lauf versucht es erneut.
    /// </summary>
    public class SollstellungHintergrunddienst(Container container, ILogger<SollstellungHintergrunddienst> logger)
        : BackgroundService
    {
        private static readonly TimeSpan Abstand = TimeSpan.FromHours(6);

        protected override async Task ExecuteAsync(CancellationToken stoppToken)
        {
            using var timer = new PeriodicTimer(Abstand);
            do
            {
                await LaufAsync();
            }
            while (await WarteAsync(timer, stoppToken));
        }

        private static async Task<bool> WarteAsync(PeriodicTimer timer, CancellationToken stoppToken)
        {
            try { return await timer.WaitForNextTickAsync(stoppToken); }
            catch (OperationCanceledException) { return false; }
        }

        /// <summary>Ein Lauf; öffentlich, damit er sich ohne Zeitgeber ausführen lässt.</summary>
        public async Task LaufAsync()
        {
            try
            {
                var heute = DateOnly.FromDateTime(DateTime.Today);
                await using var scope = AsyncScopedLifestyle.BeginScope(container);
                var angelegt = await container.GetInstance<SollstellungService>().AnlegenAsync(heute.Year, heute);
                if (angelegt > 0)
                    logger.LogInformation("{Anzahl} Sollstellungen für {Jahr} angelegt.", angelegt, heute.Year);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Sollstellungen konnten nicht angelegt werden.");
            }
        }
    }
}
