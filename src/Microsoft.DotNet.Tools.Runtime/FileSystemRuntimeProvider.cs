// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Tools.Runtime
{
    public class FileSystemRuntimeProvider : IRuntimeProvider
    {
        private string _feed;
        public FileSystemRuntimeProvider(string feed)
        {
            _feed = feed;
        }

        public string GetLatest(string rid)
        {
            if (Directory.Exists(_feed))
            {
                var extension = rid.Contains("win") ? ".zip" : ".tar.gz";
                var latest = Directory.EnumerateFiles(_feed)
                    .Where(file => file.EndsWith(extension) && file.Contains(rid))
                    .OrderByDescending(f => f)
                    .FirstOrDefault();
                return latest;
            }
            else
            {
                Console.WriteLine($"{_feed} does not exist".Yellow());
                return string.Empty;
            }
        }

        public string GetRuntime(string runtimeName)
        {
            if (!Path.IsPathRooted(runtimeName))
            {
                runtimeName = Path.Combine(_feed, runtimeName);
            }

            if (!File.Exists(runtimeName))
            {
                Console.WriteLine("Path to runtime does not exist".Yellow());
                return string.Empty;
            }

            var downloadLocation = Path.Combine(Path.GetTempPath(), Path.GetFileName(runtimeName));
            File.Copy(runtimeName, downloadLocation);

            return downloadLocation;
        }
    }
}
