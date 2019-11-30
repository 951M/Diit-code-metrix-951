using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Diit.CodeMetrics.Data
{

    public interface ICommentMetrics :IBaseMetrics
    {
        Dictionary<string,double> CMetrics { get; set; }
        
    }
}
