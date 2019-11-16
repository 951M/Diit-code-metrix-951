using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data.Source;

namespace Diit.CodeMetrics.Services.Source
{
    /// <summary>
    /// Create a one page module from input string
    /// </summary>
    public class StringSourceLoader : ISourceLoader<StringSourceViewModel>
    {
        public Task<List<Module>> LoadData(StringSourceViewModel stringSource)
        {
            if (stringSource.Source.Length > 0)
            {
                var data = new List<Module>()
                {
                    new Module()
                    {
                        Path = stringSource.Name,
                        Source = Encoding.UTF8.GetBytes(stringSource.Source)
                    }
                };
                return Task.FromResult(data);
            }
            else return Task.FromResult(new List<Module>());
        }
    }
}