using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Diit.CodeMetrics.Data.Source
{
    public class FileSourceViewModel :BaseSourceViewModel
    {
        public IReadOnlyList<IFormFile> Files { get; set; }
        public string FileType { get; set; }
    }
}