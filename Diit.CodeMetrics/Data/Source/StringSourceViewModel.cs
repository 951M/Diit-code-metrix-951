using System;

namespace Diit.CodeMetrics.Data.Source
{
    public class StringSourceViewModel : BaseSourceViewModel
    {
        public string Source { get; set; }
        public string Name { get; set; } = "Main";
    }
}