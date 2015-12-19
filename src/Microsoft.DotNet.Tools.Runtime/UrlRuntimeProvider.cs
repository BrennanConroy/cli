// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Tools.Runtime
{
    public class UrlRuntimeProvider : IRuntimeProvider
    {
        private string _feed;
        private HttpClient _client;
        public UrlRuntimeProvider(string feed)
        {
            _feed = feed + (feed.EndsWith("/") ? "" : "/");
            _client = new HttpClient();
        }

        public string GetLatest(string rid)
        {
            var extension = rid.Contains("win") ? ".zip" : ".tar.gz";
            var result = _client.GetAsync(_feed + "$index").Result;
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine($"{_feed} is missing an $index file".Yellow());
                return string.Empty;
            }
            var index = result.Content.ReadAsStringAsync().Result;
            var latest = index.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Where(file => file.EndsWith(extension) && file.Contains(rid))
                .OrderByDescending(f => f)
                .FirstOrDefault();
            return latest;
        }

        public string GetRuntime(string runtimeName)
        {
            var result = _client.GetAsync(_feed + runtimeName).Result;
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to download {runtimeName}".Yellow());
                return string.Empty;
            }

            var downloadLocation = "C:/users/brecon/downloads/temp";
            using (var stream = new FileStream(downloadLocation, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
            {
                result.Content.CopyToAsync(stream).Wait();
            }

            return downloadLocation;
        }
    }
}
