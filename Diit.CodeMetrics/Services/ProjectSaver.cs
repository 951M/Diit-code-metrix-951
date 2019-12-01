using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diit.CodeMetrics.Services
{
    public class ProjectSaver : IProjectSaver<IMetrics>
    {
        private string _pathToTemp = "App_Data\\temp.json";
        private string _pathToDB = "App_Data\\database.json";

        public void SaveToTemp(IMetrics metrics, Module source)
        {
            ProjectEntity project = new ProjectEntity { Code = Encoding.UTF8.GetString(source.Source), Metrics = metrics as Metrics };
            string output = JsonConvert.SerializeObject(project);
            using (StreamWriter sw = new StreamWriter(_pathToTemp, false, Encoding.Default))
            {
                sw.Write(output);
            }
        }

        public void SaveLastByName(string name)
        {
            ProjectEntity project = new ProjectEntity();
            using (StreamReader sr = new StreamReader(_pathToTemp))
            {
                string jsonObj = sr.ReadToEnd();
                project = JsonConvert.DeserializeObject<ProjectEntity>(jsonObj);
            }
            project.Name = name;

            string output = JsonConvert.SerializeObject(project);
            using (StreamWriter sw = new StreamWriter(_pathToDB, true, Encoding.Default))
            {
                sw.Write(output);
            }
        }
    }
}
