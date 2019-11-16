using System.Collections.Generic;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data.Source;

namespace Diit.CodeMetrics.Services
{
    public interface IFileProjectWorker
    {
        /// <summary>
        /// Save project into hosted directory
        /// </summary>
        /// <param name="modules">Modules to save</param>
        /// <param name="name">project name</param>
        /// <remarks>Throws io exception when project already exist</remarks>
        Task SaveProject(List<Module> modules, string name);
        bool CheckExistProject(string name);
        void DeleteProject(string name);
        Task<List<Module>> LoadProject(string name);
    }
}