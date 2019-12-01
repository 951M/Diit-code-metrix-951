using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Diit.CodeMetrics.Controllers
{
    public class CompareController : Controller
    {

        private IProjectSaver<IMetrics> _projectSaver;

        public CompareController(IProjectSaver<IMetrics> saver)
        {
            _projectSaver = saver;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<string> list = _projectSaver.GetAllNamesFromDB();
            return View(list);
        }

        [HttpPost]
        public IActionResult GetProject([FromBody]string name)
        {
            ProjectEntity project = _projectSaver.GetByNameFromDB(name);
            return new JsonResult(new { project });
        }
    }
}