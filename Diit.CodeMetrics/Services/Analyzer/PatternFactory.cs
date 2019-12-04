using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data;

namespace Diit.CodeMetrics.Services.Analyzer
{
    public class PatternFactory : IPatternFactory
    {
        public Regex GetPattern(PatterTypesEnum patternType)
        {
            switch (patternType)
            {
                case PatterTypesEnum.ConditionOperatorsPatter:
                    return new Regex(@"^(if|else)$");
                case PatterTypesEnum.DividersPattern:
                    return new Regex(@"[\{\}\(\)\;\s]");
                case PatterTypesEnum.LinesToDelete:
                    return new Regex(@"(//(.*))|(""(.*)"")");
                case PatterTypesEnum.BlocksToDelete:
                    return new Regex(@"(\/\*.*\*/)", RegexOptions.Singleline);
                case PatterTypesEnum.OperatorsPattern:
                    return new Regex(@"(^(if|else|while|for$|foreach|dowhile|break|continue|throw|return)$)|(\+|-|=|\*|\/|\!|\&\&|\|\|<>)");
                case PatterTypesEnum.Comments:
                    return new Regex(@"(^(/\*|//|\*/))");
                default:
                    return null;
            }
        }
    }
}
