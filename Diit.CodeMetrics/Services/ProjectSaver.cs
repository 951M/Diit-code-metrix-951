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

            List<ProjectEntity> list = new List<ProjectEntity>();
            if (File.Exists(_pathToDB))
            {
                using (StreamReader sr = new StreamReader(_pathToDB))
                {
                    string jsonObj = sr.ReadToEnd();
                    list = JsonConvert.DeserializeObject<List<ProjectEntity>>(jsonObj);
                }
            }

            list.Add(project);

            string output = JsonConvert.SerializeObject(list);
            using (StreamWriter sw = new StreamWriter(_pathToDB, false, Encoding.Default))
            {
                sw.Write(output);
            }
        }

        public List<string> GetAllNamesFromDB()
        {
            List<ProjectEntity> list = new List<ProjectEntity>();
            using (StreamReader sr = new StreamReader(_pathToDB))
            {
                string jsonObj = sr.ReadToEnd();
                list = JsonConvert.DeserializeObject<List<ProjectEntity>>(jsonObj);
            }
            List<string> names = new List<string>();
            foreach(var obj in list)
            {
                names.Add(obj.Name);
            }
            return names;
        }

        public ProjectEntity GetByNameFromDB(string name)
        {
            ProjectEntity project;

            List<ProjectEntity> list = new List<ProjectEntity>();
            using (StreamReader sr = new StreamReader(_pathToDB))
            {
                string jsonObj = sr.ReadToEnd();
                list = JsonConvert.DeserializeObject<List<ProjectEntity>>(jsonObj);
            }

            project = list.Where(item => item.Name == name).First();

            return project;
        }
    }
}
