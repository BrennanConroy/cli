// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.DotNet.Tools.Runtime
{
    public class Program
    {
        private static readonly string installLocation = "C:\\Users\\brecon\\AppData\\Local\\Microsoft\\dotnet\\cli\\runtime";
        private static readonly string defaultFeed = "C:\\Users\\brecon\\Downloads"; //https://azure.blob.storage
        public static int Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            var app = new CommandLineApplication();
            app.Name = "dotnet runtime";
            app.FullName = ".NET Runtime Manager";
            app.Description = "Runtime manager for the .NET Platform";
            app.HelpOption("-h|--help");

            app.Command("install", c =>
            {
                c.Description = "Runtime";

                var projectDirOrVersion = c.Argument("[project path]", "Path to project", multipleValues: false);

                //var optionAlias = c.Option("--alias <ALIAS>", "Alias", CommandOptionType.SingleValue);
                var optionArch = c.Option("--arch <ARCHITECTURE>", "Architecture [x86, x64, arm, arm64]", CommandOptionType.SingleValue);
                var optionFeed = c.Option("--feed", "Feed to install from", CommandOptionType.SingleValue);
                var optionGlobal = c.Option("--global", "Install globally", CommandOptionType.NoValue);
                c.HelpOption("-h|--help");
                var optionOs = c.Option("--os <OPERATING_SYSTEM>", "Operating System [win, osx, ubuntu, centos]", CommandOptionType.SingleValue);
                var optionRid = c.Option("--runtime-id <RID>", "Runtime ID e.g. win10-x64 or ubuntu.14.04-x64", CommandOptionType.SingleValue);

                c.OnExecute(() =>
                {
                    //var runtimeEnv = new DefaultRuntimeEnvironment();
                    var versionOrPath = string.IsNullOrEmpty(projectDirOrVersion.Value) ? Directory.GetCurrentDirectory() : projectDirOrVersion.Value;
                    var feed = optionFeed.HasValue() ? optionFeed.Value() : defaultFeed;
                    var rid = "win7-x64"; //runtimeEnv.GetRuntimeIdentifier();
                    var global = optionGlobal.HasValue() ? true : false;
                    if (optionRid.HasValue())
                    {
                        rid = optionRid.Value();
                    }
                    else
                    {
                        var os = optionOs.HasValue() ? optionOs.Value() : "win"; //runtimeEnv.OperatingSystem;
                        var arch = optionArch.HasValue() ? optionArch.Value() : "x64"; //runtimeEnv.GetArch();
                        //runtimeEnv.RuntimeArchitecture = arch;
                        //runtimeEnv.OperatingSystemPlatform = ;
                        //runtimeEnv.OperatingSystemVersion = ;
                        //rid = runtimeEnv.GetRuntimeIdentifier();
                    }
                    //var rid = optionRid.HasValue() ? optionRid.Value() : "win7-x64";
                    //var os = optionOs.HasValue() ? optionOs.Value() : "win"; //runtimeEnv.OperatingSystem;
                    //var arch = optionArch.HasValue() ? optionArch.Value() : "x64"; //runtimeEnv.GetArch()

                    IRuntimeProvider provider;
                    if (Path.IsPathRooted(feed))
                    {
                        provider = new FileSystemRuntimeProvider(feed);
                    }
                    else
                    {
                        provider = new UrlRuntimeProvider(feed);
                    }

                    string runtimeName = string.Empty;
                    if (versionOrPath == "latest")
                    {
                        // get latest version from feed
                        runtimeName = provider.GetLatest(rid);
                    }
                    else
                    {
                        // check if its a path or a runtime version
                        if (versionOrPath.EndsWith(".zip") || versionOrPath.EndsWith(".tar.gz"))
                        {
                            runtimeName = versionOrPath;
                        }
                        // path to project
                        else if (versionOrPath.Contains("project.json") || Directory.Exists(Path.GetDirectoryName(versionOrPath)))
                        {
                            throw new NotImplementedException("Haven't implemented project path yet");
                        }
                        // runtime version
                        else
                        {
                            var extension = rid.Contains("win") ? ".zip" : ".tar.gz";
                            runtimeName = $"coreclr_{versionOrPath}_{rid}{extension}";
                        }
                    }

                    if (string.IsNullOrEmpty(runtimeName))
                    {
                        Console.WriteLine("Could not find runtime to install".Yellow());
                        return 1;
                    }

                    string dotnetHomes;
                    if (global)
                    {
                        dotnetHomes = $"{Environment.GetEnvironmentVariable("PROGRAMDATA")}\\Microsoft\\dotnet\\cli\\runtime";
                    }
                    else
                    {
                        dotnetHomes = $"{Environment.GetEnvironmentVariable("LOCALAPPDATA")}\\Microsoft\\dotnet\\cli\\runtime";
                    }
                    // coreclr_1.0.0-alpha-0001_win7-x64.zip
                    var runtimeId = Path.GetFileNameWithoutExtension(runtimeName).Split('_');
                    var outputDir = Path.Combine(dotnetHomes, runtimeId[1], runtimeId[2]);

                    if (Directory.Exists(outputDir))
                    {
                        Console.WriteLine($"{outputDir} already exists".Yellow());
                        return 1;
                    }

                    var compressedRuntime = provider.GetRuntime(runtimeName);
                    return ExtractRuntime(compressedRuntime, outputDir);
                });
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.Error.WriteLine(ex);
#else
                Console.Error.WriteLine(ex.Message);
#endif
                return 1;
            }
        }

        private static int ExtractRuntime(string runtimePath, string outputDir)
        {
            if (string.IsNullOrEmpty(runtimePath) || !File.Exists(runtimePath))
            {
                return 1;
            }

            try
            {
                Directory.CreateDirectory(outputDir);

                using (var runtimeZip = ZipFile.OpenRead(runtimePath))
                {
                    foreach (var file in runtimeZip.Entries)
                    {
                        file.ExtractToFile(Path.Combine(outputDir, file.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                Directory.Delete(outputDir, true);
                throw ex;
            }
            finally
            {
                File.Delete(runtimePath);
            }

            Console.WriteLine($"Installed to {outputDir}");

            return 0;
        }
    }
}
