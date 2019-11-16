using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Diit.CodeMetrics.Data
{
    public class GraphEntity
    {
        /// <summary>
        /// Number of entity
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Number in source code for this entities
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Type of entity
        /// </summary>
        public GraphEntityTypesEnum Type { get; set; }

        /// <summary>
        /// Some comment 
        /// </summary>
        public String Comment { get; set; }

        /// <summary>
        /// List of numbers of graph sub entities entities
        /// </summary>
        public List<int> ConnectedTo { get; set; } = new List<int>();
    }
}
