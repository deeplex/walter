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

using System.Net.Http.Headers;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Deeplex.Saverwalter.WebAPI.Utils
{
    public class WalterFile
    {
        public string FileName { get; set; } = null!;
        public string Key { get; set; } = null!;
        public long LastModified { get; set; }
        public long? Size { get; set; }
        public byte[] Blob { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
    public static class FileHandling
    {

        public static List<WalterFile> ParseS3Stream(string xmlContent, string path)
        {
            List<WalterFile> walterFiles = [];

            using (var stringReader = new StringReader(xmlContent))
            {
                using var reader = XmlReader.Create(stringReader);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Contents")
                    {
                        var walterFile = new WalterFile();

                        reader.ReadToDescendant("Key");
                        var s3key = reader.ReadElementContentAsString();
                        walterFile.FileName = s3key.Split("/").Last();

                        if (s3key.Split("/").Reverse().Skip(1).First() == "trash")
                        {
                            continue;
                        }

                        walterFile.Key = string.Join("/", path, walterFile.FileName);

                        walterFiles.Add(walterFile);
                    }
                }
            }

            return walterFiles;
        }

        private static StreamContent FillContent(string path, HttpRequest request)
        {
            var content = new StreamContent(request.Body);

            var contentLength = request.ContentLength ?? -1;
            if (contentLength >= 0)
            {
                content.Headers.ContentLength = contentLength;
            }

            new FileExtensionContentTypeProvider()
                .TryGetContentType(Path.GetExtension(path), out var contentType);
            if (contentType != null)
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }

            return content;
        }

        private async static Task<HttpResponseMessage> SaveToTrash(string path, HttpRequest request, HttpClient httpClient)
        {
            HttpContent content = null!;
            // Put trash before the filename
            var splits = path.Split('/');
            var trashPath = $"{string.Join('/', splits.Reverse().Skip(1).Reverse())}/trash/{splits.Last()}";
            var trashRequest = new HttpRequestMessage(new HttpMethod(HttpMethod.Put.Method), trashPath) { Content = content };
            trashRequest.Content = FillContent(path, request);
            var trashResponse = await httpClient.SendAsync(trashRequest, CancellationToken.None);

            return trashResponse;
        }

        public static async Task<IActionResult> RedirectToFileServer(HttpRequest request, HttpClient client, string path)
        {
            HttpContent content = null!;
            var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), path) { Content = content };

            if (request.Method == HttpMethod.Put.Method)
            {
                requestMessage.Content = FillContent(path, request);
            }
            else if (request.Method == HttpMethod.Delete.Method)
            {
                var trashResponse = await SaveToTrash(path, request, client);

                if (!trashResponse.IsSuccessStatusCode)
                {
                    return new StatusCodeResult((int)trashResponse.StatusCode);
                }
            }

            var response = await client.SendAsync(requestMessage, CancellationToken.None);
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            var responseType = response.Content.Headers.ContentType?.MediaType ?? "plain/text";

            return new FileContentResult(responseContent, responseType);
        }
    }
}
