﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diit.CodeMetrics.Data
{
    public class ProjectEntity
    {
        public string Name { get; set; }

        public Metrics Metrics { get; set; }

        public string Code { get; set; }
    }
}
