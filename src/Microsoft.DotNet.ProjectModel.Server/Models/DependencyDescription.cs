﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.ProjectModel.Server.Models
{
    public class DependencyDescription
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Version { get; set; }

        public string Path { get; set; }

        public string Type { get; set; }

        public bool Resolved { get; set; }

        public IEnumerable<DependencyItem> Dependencies { get; set; }

        public IEnumerable<DiagnosticMessageView> Errors { get; set; }

        public IEnumerable<DiagnosticMessageView> Warnings { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as DependencyDescription;

            return other != null &&
                   Resolved == other.Resolved &&
                   string.Equals(Name, other.Name) &&
                   object.Equals(Version, other.Version) &&
                   string.Equals(Path, other.Path) &&
                   string.Equals(Type, other.Type) &&
                   Enumerable.SequenceEqual(Dependencies, other.Dependencies) &&
                   Enumerable.SequenceEqual(Errors, other.Errors) &&
                   Enumerable.SequenceEqual(Warnings, other.Warnings);
        }

        public override int GetHashCode()
        {
            // These objects are currently POCOs and we're overriding equals
            // so that things like Enumerable.SequenceEqual just work.
            return base.GetHashCode();
        }
    }
}