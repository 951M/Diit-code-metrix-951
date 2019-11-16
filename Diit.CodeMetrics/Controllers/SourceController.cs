using System;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data.Source;
using Diit.CodeMetrics.Services;
using Diit.CodeMetrics.Services.Source;
using Microsoft.AspNetCore.Mvc;

namespace Diit.CodeMetrics.Controllers
{
    [ApiController]    
    [Route("[controller]/[action]")]
    public class SourceController : Controller
    {
        private readonly IFileProjectWorker _fileProjectWorker;
        private readonly ISourceLoader<StringSourceViewModel> _stringSourceLoader;
        private readonly ISourceLoader<FileSourceViewModel> _fileSourceLoader;

        /// <summary>
        /// Controller for working with source files
        /// </summary>
        public SourceController(IFileProjectWorker fileProjectWorker,
            ISourceLoader<StringSourceViewModel> stringSourceLoader,
            ISourceLoader<FileSourceViewModel> fileSourceLoader)
        {
            _fileProjectWorker = fileProjectWorker;
            _stringSourceLoader = stringSourceLoader;
            _fileSourceLoader = fileSourceLoader;
        }

        /// <summary>
        /// Load projects from string
        /// </summary>
        /// <param name="source">Input source</param>
        /// <returns>Success or fail in JSON</returns>
        [HttpPost]
        public async Task<IActionResult> LoadFromString([FromBody] StringSourceViewModel source)
        {
            try
            {
                var memorySource = await _stringSourceLoader.LoadData(source);
                await _fileProjectWorker.SaveProject(memorySource, source.Project);
                return new JsonResult(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    exception = ex.ToString()
                });
            }
        }
        
        /// <summary>
        /// Load project files (or zip archive)
        /// </summary>
        /// <param name="source">Source files</param>
        /// <returns>Success or fail in JSON</returns>
        [HttpPost]
        public async Task<IActionResult> LoadFromFile([FromBody] FileSourceViewModel source)
        {
            try
            {
                var memorySource = await _fileSourceLoader.LoadData(source);
                await _fileProjectWorker.SaveProject(memorySource, source.Project);
                return new JsonResult(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    exception = ex.ToString()
                });
            }
        }
        
        /// <summary>
        /// Delete project directory
        /// </summary>
        /// <param name="project">Project name</param>
        /// <returns>Success or fail in JSON</returns>
        [HttpPost]
        public IActionResult DeleteProject([FromBody] BaseSourceViewModel project)
        {
            try
            {
               _fileProjectWorker.DeleteProject(project.Project);
                return new JsonResult(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    exception = ex.ToString()
                });
            }
        }
        
        /// <summary>
        /// Check project existing
        /// </summary>
        /// <param name="project">Project name</param>
        /// <returns>Exist or not or fail in JSON</returns>
        [HttpPost]
        public IActionResult CheckProject([FromBody] BaseSourceViewModel project)
        {
            try
            {
                bool exist = _fileProjectWorker.CheckExistProject(project.Project);
                return new JsonResult(new
                {
                    success = true,
                    exist
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    exist = false,
                    exception = ex.ToString()
                });
            }
        }
    }
}