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
        int _nUnUsedVariables;
        int _nUsedVariables;
        int _nConfigVariables;
        int _nReadOnlyVariables;

        public SharpChepinaAnalyzer(IHostingEnvironment inv)
        {

        }


        public IEnumerable<AnalyzerItem> AnalyzeSource(string source)
        {
            findVariableTypeP();
            findVariableTypeM();
            findVariableTypeC();
            findVariableTypeT();
            AnalyzerItem item = new AnalyzerItem
            {
                nUnUsedVariables = _nUnUsedVariables,
            nUsedVariables = _nUsedVariables,
            nConfigVariables = _nConfigVariables,
            nReadOnlyVariables = _nReadOnlyVariables,
        };
            List<AnalyzerItem> IEN = new List<AnalyzerItem>();
            IEN.Add(item);
            return IEN;

        }

        //read only  variables
        public void findVariableTypeP() {
            Random rnd = new Random();
            _nReadOnlyVariables =  rnd.Next(1, 3);
        }

        // used variables
        public void findVariableTypeM()
        {
            Random rnd = new Random();
            _nUsedVariables = rnd.Next(1, 3);
        }

        //config variables
        public void findVariableTypeC()
        {
            Random rnd = new Random();
            _nConfigVariables = rnd.Next(1, 3);
        }

        //unused  variables
        public void findVariableTypeT()
        {
            Random rnd = new Random();
            _nUnUsedVariables = rnd.Next(1, 3);
        }
    }
}
