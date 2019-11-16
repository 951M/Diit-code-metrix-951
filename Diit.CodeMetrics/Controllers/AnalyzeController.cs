using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;
using Diit.CodeMetrics.Services;
using Diit.CodeMetrics.Services.Source;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Diit.CodeMetrics.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AnalyzeController : Controller
    {
        private readonly ISourceLoader<StringSourceViewModel> _stringSourceLoader;
        private readonly ISourceLoader<FileSourceViewModel> _fileSourceLoader;
        private readonly IMetricsCreator<IMetrics> _metricsCreator;

        public AnalyzeController(ISourceLoader<StringSourceViewModel> stringSourceLoader, ISourceLoader<FileSourceViewModel> fileSourceLoader, IMetricsCreator<IMetrics> metricsCreator)
        {
            _stringSourceLoader = stringSourceLoader;
            _fileSourceLoader = fileSourceLoader;
            _metricsCreator = metricsCreator;
        }


        /// <summary>
        /// Load source from string to server and analyze code metrics
        /// </summary>
        /// <param name="stringSourceViewModel">Input source</param>
        /// <returns>Metrics and graph entities in JSON</returns>
        [HttpPost]
        public  async Task<IActionResult> LoadStringAndAnalyze([FromBody]StringSourceViewModel stringSourceViewModel)
        {
            try
            {
                var memorySource = await _stringSourceLoader.LoadData(stringSourceViewModel);
                var metrics = _metricsCreator.CreateMetrics(memorySource);
                return new JsonResult(new { success = true, metrics},new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>()
                    {
                        new StringEnumConverter()
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new {success = false, exception = ex.ToString()});
            }

        }
        
        /// <summary>
        /// Load source from files (or zip archive) to server and analyze code metrics
        /// </summary>
        /// <param name="fileSourceViewModel">Input source</param>
        /// <returns>Metrics and graph entities in JSON</returns>
        [HttpPost]
        public  async Task<IActionResult> LoadFileAndAnalyze([FromForm] FileSourceViewModel fileSourceViewModel)
        {
            try
            {
                var memorySource = await _fileSourceLoader.LoadData(fileSourceViewModel);
                var metrics = _metricsCreator.CreateMetrics(memorySource);
                return new JsonResult(new { success = true, metrics},new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>()
                    {
                        new StringEnumConverter()
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new {success = false, exception = ex.ToString()});
            }

        }
    }
}