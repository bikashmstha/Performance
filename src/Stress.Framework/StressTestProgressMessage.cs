// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using LongLivedMessage = Xunit.Sdk.LongLivedMarshalByRefObject;

namespace Stress.Framework
{
    public class StressTestProgressMessage : LongLivedMessage, ITestProgressMessage, IMessageSinkMessageWithTypes
    {
        private static readonly HashSet<string> _interfaceTypes =
            new HashSet<string>(typeof(StressTestProgressMessage).GetInterfaces().Select(type => type.FullName));

        public StressTestProgressMessage(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }

        public HashSet<string> InterfaceTypes => _interfaceTypes;
    }
}
