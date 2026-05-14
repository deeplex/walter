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

using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/abrechnungslauf/print")]
    public class AbrechnungslaufPrintController : ControllerBase
    {
        public class PrintRequest
        {
            public List<int> WohnungIds { get; set; } = [];
            public int Jahr { get; set; }
            /// <summary>Optional: nur diesen Vertrag drucken. Null = alle Verträge der Gruppe.</summary>
            public int? VertragId { get; set; }
        }

        private readonly AbrechnungslaufPrintService _printService;

        public AbrechnungslaufPrintController(AbrechnungslaufPrintService printService)
        {
            _printService = printService;
        }

        [HttpPost("pdf")]
        public async Task<IActionResult> PrintPdf([FromBody] PrintRequest request)
            => await Drucke(request, "pdf");

        [HttpPost("docx")]
        public async Task<IActionResult> PrintDocx([FromBody] PrintRequest request)
            => await Drucke(request, "docx");

        private async Task<IActionResult> Drucke(PrintRequest request, string format)
        {
            if (request.WohnungIds.Count == 0)
                return BadRequest("Mindestens eine Wohnung muss ausgewählt sein.");

            try
            {
                var entries = format == "docx"
                    ? await _printService.ErstelleDocxsAsync(request.WohnungIds, request.Jahr, request.VertragId)
                    : await _printService.ErstellePdfsAsync(request.WohnungIds, request.Jahr, request.VertragId);

                if (entries.Count == 0)
                    return NotFound("Keine aktiven Verträge in der Abrechnungsgruppe gefunden.");

                if (entries.Count == 1)
                {
                    var single = entries[0];
                    var mime = format == "docx"
                        ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                        : "application/pdf";
                    return File(single.Inhalt, mime, single.Dateiname);
                }

                using var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var entry in entries)
                    {
                        var zipEntry = archive.CreateEntry(entry.Dateiname, CompressionLevel.Optimal);
                        using var entryStream = zipEntry.Open();
                        await entryStream.WriteAsync(entry.Inhalt);
                    }
                }

                zipStream.Position = 0;
                var zipName = $"NK_{request.Jahr}_Abrechnung.zip";
                return File(zipStream.ToArray(), "application/zip", zipName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }
    }
}
