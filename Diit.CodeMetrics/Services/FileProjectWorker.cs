using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data.Source;
using Microsoft.AspNetCore.Hosting;

namespace Diit.CodeMetrics.Services
{
    public class FileProjectWorker : IFileProjectWorker
    {
        private readonly string _hostPath;

        public FileProjectWorker(IHostingEnvironment hostingEnvironment)
        {
            _hostPath = Path.Combine(hostingEnvironment.ContentRootPath, "Projects");
        }

        public async Task SaveProject(List<Module> modules, string name)
        {
            string projPath = Path.Combine(_hostPath, name);
            if (CheckExistProject(name))
            {
                throw new IOException($"Project {name} already exist");
            }

            Directory.CreateDirectory(projPath);
            foreach (var module in modules)
            {
                using (FileStream fs = new FileStream(Path.Combine(projPath, module.Path), FileMode.Create))
                {
                    await fs.WriteAsync(module.Source);
                }
            }
        }

        public bool CheckExistProject(string name)
        {
            string projPath = Path.Combine(_hostPath, name);
            return Directory.Exists(projPath) && Directory.GetFiles(projPath).Length > 0;
        }

        public void DeleteProject(string name)
        {
            string projPath = Path.Combine(_hostPath, name);
            Directory.Delete(projPath, true);
        }

        public Task<List<Module>> LoadProject(string name)
        {
            if (!CheckExistProject(name))
            {
                throw new IOException($"Project {name} not exist");
            }
            else
            {
                string projPath = Path.Combine(_hostPath, name);
                return LoadFileRec(projPath, projPath);
            }
        }

        private async Task<List<Module>> LoadFileRec(string nowPath, string projPath)
        {
            List<Module> modules = new List<Module>();
            foreach (var directory in Directory.GetDirectories(nowPath))
            {
                modules.AddRange(await LoadFileRec(directory, projPath));
            }

            foreach (var file in Directory.GetFiles(nowPath))
            {
                Module fileModule = new Module {Path = file.Replace(projPath + "/", "")};
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    await fs.ReadAsync(fileModule.Source);
                }

                modules.Add(fileModule);
            }

            return modules;
        }
    }
}