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

using Deeplex.Saverwalter.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class FileSortTests
    {
        [Fact]
        public async Task SortierenAufAnforderungVerschiebtFlacheDateienInIhrUploadJahr()
        {
            var s3 = new FakeS3();
            s3.Add("vertraege/3/a.pdf", "a", new DateTimeOffset(2022, 6, 15, 12, 0, 0, TimeSpan.Zero));
            s3.Add("vertraege/3/2024/b.pdf", "b", new DateTimeOffset(2022, 6, 15, 12, 0, 0, TimeSpan.Zero));
            Environment.SetEnvironmentVariable("S3_PROVIDER", FakeS3.Base);

            var result = await new FileSortController(new HttpClient(s3)).Post();

            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<FileSortController.FileSortResult>().Which.Sorted.Should().Be(1);
            s3.Objects.Keys.Should().BeEquivalentTo("vertraege/3/2022/a.pdf", "vertraege/3/2024/b.pdf");
        }

        [Fact]
        public async Task SortierenOhneDateiablageIstNichtVerfuegbar()
        {
            Environment.SetEnvironmentVariable("S3_PROVIDER", null);

            var result = await new FileSortController(new HttpClient(new FakeS3())).Post();

            result.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(503);
        }
    }
}
