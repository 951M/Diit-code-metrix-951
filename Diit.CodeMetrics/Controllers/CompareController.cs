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
        private IHMetricsComparer _hMetricsComparer;

        public CompareController(IProjectSaver<IMetrics> saver, IHMetricsComparer comparer)
        {
            _projectSaver = saver;
            _hMetricsComparer = comparer;
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

        [HttpPost]
        public IActionResult GetCompareResult([FromBody]TwoNamesVM names)
        {
            ProjectEntity project1 = _projectSaver.GetByNameFromDB(names.Name1);
            ProjectEntity project2 = _projectSaver.GetByNameFromDB(names.Name2);
            CompareHMetricsVM compare2 = _hMetricsComparer.CompareTwoProjects(project1, project2);
            CompareHMetricsVM compare1 = _hMetricsComparer.CompareTwoProjects(project2, project1);
            return new JsonResult(new { compare1, compare2 });
        }
    }
}