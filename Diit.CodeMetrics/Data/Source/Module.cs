using System;
using System.IO;

namespace Diit.CodeMetrics.Data.Source
{
    public class Module
    {
        public string Path { get; set; }

        public byte[] Source { get; set; }
    }
}