using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Diit.CodeMetrics.Data
{
    public enum GraphEntityTypesEnum
    {
        Empty,
        If,
        Cycle,
        StartProcedure,
        EndProcedure,
        Nested
    }
}
