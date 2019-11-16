using System.Collections.Generic;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data.Source;

namespace Diit.CodeMetrics.Services.Source
{
    public interface ISourceLoader<in T> where T :BaseSourceViewModel  
    {
        /// <summary>
        /// Get project from viewmodel
        /// <param name="sourceViewModel">Uploaded source</param>
        /// <returns>List of source file</returns>
        /// </summary>
        Task<List<Module>> LoadData(T sourceViewModel);
        
    }
}