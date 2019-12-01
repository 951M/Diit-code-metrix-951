using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Services;
using Microsoft.AspNetCore.Mvc;

namespace Diit.CodeMetrics.Controllers
{
    public class SaveController : Controller
    {
        private readonly IProjectSaver<IMetrics> _projectSaver;

        public SaveController(IProjectSaver<IMetrics> saver)
        {
            _projectSaver = saver;
        }

        public IActionResult SaveProject([FromBody]string name)
        {
            try
            {
                _projectSaver.SaveLastByName(name);
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, exception = ex.ToString() });
            }
        }
    }
}