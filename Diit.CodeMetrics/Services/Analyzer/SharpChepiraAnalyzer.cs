using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data;
using Microsoft.AspNetCore.Hosting;

namespace Diit.CodeMetrics.Services.Analyzer
{
    public class SharpChepinaAnalyzer : ILexicalAnalyzer<IChepinaMetrics>
    {
        private readonly IHostingEnvironment _inv;

        public SharpChepinaAnalyzer(IHostingEnvironment inv)
        {

        }


        public IEnumerable<AnalyzerItem> AnalyzeSource(string source)
        {
            AnalyzerItem item = new AnalyzerItem
            {
            };
            List<AnalyzerItem> IEN = new List<AnalyzerItem>();
            IEN.Add(item);
            return IEN;

        }

        public int findVariableTypeP() {
            Random rnd = new Random();
            return rnd.Next(1, 3);
        }
        public int findVariableTypeM()
        {
            Random rnd = new Random();
            return rnd.Next(1, 3);
        }
        public int findVariableTypeC()
        {
            Random rnd = new Random();
            return rnd.Next(1, 3);
        }
        public int findVariableTypeT()
        {
            Random rnd = new Random();
            return rnd.Next(1, 3);
        }
    }
}
