using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diit.CodeMetrics.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProjectSaver<T> where T : IBaseMetrics
    {
        void SaveToTemp(T metrics, Module source);

        void SaveLastByName(string name);
    }
}
