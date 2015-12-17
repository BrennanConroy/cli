// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.IO.Compression;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Microsoft.Extensions.PlatformAbstractions;
using System.Linq;

namespace Microsoft.DotNet.Tools.Runtime
{
    public class Program
    {
        private static readonly string installLocation = "C:\\Users\\brecon\\AppData\\Local\\Microsoft\\dotnet\\cli\\runtime";
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
                    var versionOrPath = string.IsNullOrEmpty(projectDirOrVersion.Value) ? Directory.GetCurrentDirectory() : projectDirOrVersion.Value;
                    var feed = optionFeed.HasValue() ? optionFeed.Value() : "C:\\Users\\brecon\\Downloads";

                    if (versionOrPath == "latest")
                    {
                        //get latest version from feed

                        // resolve environment specific defaults
                        //var runtimeEnv = new DefaultRuntimeEnvironment();
                        //var rid = runtimeEnv.GetRuntimeIdentifier();
                        if (Directory.Exists(feed))
                        {
                            //feed is a filesystem
                            //Directory.EnumerateFiles(feed, "*.zip");
                        }
                    }
                    else
                    {
                        //check if its a path or a runtime version
                        if (versionOrPath.EndsWith(".zip") || versionOrPath.EndsWith(".tar.gz"))
                        {
                            // versionOrPath is a path
                            if (!File.Exists(versionOrPath))
                            {
                                Console.WriteLine("Path to runtime does not exist".Yellow());
                                return 1;
                            }

                            //var dotnetHomeEnv = Environment.GetEnvironmentVariable("DOTNET_HOME");
                            //if (string.IsNullOrEmpty(dotnetHomeEnv))
                            //{
                            //    Console.WriteLine("DOTNET_HOME environment variable not set".Yellow());
                            //    return 1;
                            //}
                            //var dotnetHomes = dotnetHomeEnv.Split(';');
                            var dotnetHomes = new string[] { installLocation };

                            // coreclr_1.0.0-alpha-0001_win7-x64.zip
                            var runtimeId = Path.GetFileNameWithoutExtension(versionOrPath).Split('_');
                            var outputDir = Path.Combine(dotnetHomes[dotnetHomes.Length - 1], runtimeId[1], runtimeId[2]);

                            if (Directory.Exists(outputDir))
                            {
                                Console.WriteLine($"Runtime {runtimeId[1]}/{runtimeId[2]} already exists".Yellow());
                                return 1;
                            }

                            try
                            {
                                Directory.CreateDirectory(outputDir);

                                var runtimeZip = ZipFile.OpenRead(versionOrPath);
                                foreach (var file in runtimeZip.Entries)
                                {
                                    file.ExtractToFile(Path.Combine(outputDir, file.Name));
                                }
                            }
                            catch(Exception ex)
                            {
                                Directory.Delete(outputDir, true);
                                throw ex;
                            }

                            Console.WriteLine($"Installed to {outputDir}");

                            return 0;
                        }
                    }
                    Console.WriteLine($"Using feed: {feed}");
                    Console.WriteLine($"versionOrPath: {versionOrPath}");
                    return 0;
                });
            });

            var framework = string.Empty;
            //var framework = app.Argument("install", "Runtime", false);
            //var framework = app.Option("install", "path to project.json or runtime file or version number", CommandOptionType.SingleValue);
            //var project = app.Argument("<PROJECT>", "The project to publish, defaults to the current directory. Can be a path to a project.json or a project directory");

            app.OnExecute(() =>
            {
                //NuGetFramework nugetframework = null;

                if (string.IsNullOrEmpty(framework))
                {
                    //nugetframework = NuGetFramework.Parse(framework.Value());

                    //if (nugetframework.IsUnsupported)
                    {
                        Reporter.Output.WriteLine($"Unsupported framework {framework}.".Red());
                        return 1;
                    }
                }

                // TODO: Remove this once xplat publish is enabled.
                //if (!runtime.HasValue())
                {
                    //runtime.Values.Add(RuntimeIdentifier.Current);
                }

                // Locate the project and get the name and full path
                //var path = project.Value;
                //if (string.IsNullOrEmpty(path))
                //{
                //    path = Directory.GetCurrentDirectory();
                //}

                //var projectContexts = ProjectContext.CreateContextForEachTarget(path);
                //projectContexts = GetMatchingProjectContexts(projectContexts, nugetframework, runtime.Value());

                //if (projectContexts.Count() == 0)
                //{
                //    string errMsg = $"'{project.Value}' cannot be published";
                //    if (framework.HasValue() || runtime.HasValue())
                //    {
                //        errMsg += $" for '{framework.Value()}' '{runtime.Value()}'";
                //    }

                //    Reporter.Output.WriteLine(errMsg.Red());
                //    return 1;
                //}

                //int result = 0;
                //foreach (var projectContext in projectContexts)
                //{
                //    result += Publish(projectContext, output.Value(), configuration.Value() ?? Constants.DefaultConfiguration);
                //}

                //return result;
                return 0;
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
    }
}
