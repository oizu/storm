﻿using System;
using System.Collections.Generic;
using System.Text;
using Dotnet.Storm.Adapter.Messaging;

namespace Dotnet.Storm.Adapter.Test
{
    public class TestOutput
    {
        public long Task { get; set; }

        public string Stream { get; set; }

        public List<object> Tuple { get; set; }

        public bool NeedTaskIds { get; set; }
    }
}
