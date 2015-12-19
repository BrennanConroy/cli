// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Tools.Runtime
{
    internal interface IRuntimeProvider
    {
        string GetLatest(string rid);
        string GetRuntime(string runtimeName);
    }
}
