using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;
using Diit.CodeMetrics.Services.Analyzer;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;

namespace Diit.CodeMetrics.Services
{
    public class McCeibMetricsCreator : IMetricsCreator<IMcCeibMetrics>
    {
        private readonly ILexicalAnalyzer<IMcCeibMetrics> _lexicalAnalyzer;

        public McCeibMetricsCreator(ILexicalAnalyzer<IMcCeibMetrics> lexicalAnalyzer)
        {
            _lexicalAnalyzer = lexicalAnalyzer;
        }

        public IMcCeibMetrics CreateMetrics(List<Module> source)
        {
            IMcCeibMetrics metrics = new Metrics();
            metrics.GraphEntities = new Dictionary<string, List<GraphEntity>>();
            Stack<GraphEntity> innert = new Stack<GraphEntity>();
            foreach (var module in source)
            {
                List<GraphEntity> graphEntities = new List<GraphEntity>();
                int number = -1;
                var analyzed = _lexicalAnalyzer.AnalyzeSource(Encoding.UTF8.GetString(module.Source)).ToArray();
                for (int i = 0; i < analyzed.Length; i++)
                {
                    var item = analyzed[i];
                    GraphEntity lastInline = null;
                    var ge = new GraphEntity()
                    {
                        LineNumber = item.LineNumber,
                        Number = i,
                        Comment = item.Comment
                    };
                    if (item.IsProcedure == true)
                    {
                        ge.Type = GraphEntityTypesEnum.StartProcedure;
                    }
                    else if (item.IsProcedure == false)
                    {
                        ge.Type = GraphEntityTypesEnum.EndProcedure;
                    }
                    else if (item.BranchType == BranchType.If)
                    {
                        innert.Push(ge);
                        ge.Type = GraphEntityTypesEnum.If;
                        var connects = analyzed
                            .Where(x => x.ConnectedFrom.Any(y => y == item.LineNumber))?
                            .OrderBy(x => x.BranchType)
                            .ToList();
                        if (connects.Count == 2 || connects.Count == 1 && connects[0].ConnectedFrom.Count == 2)
                        {
                            var empty = new GraphEntity()
                            {
                                Type = GraphEntityTypesEnum.Empty,
                                LineNumber = item.LineNumber + 1,
                                Number = number
                            };
                            number--;
                            ge.ConnectedTo.Add(connects.Count == 2
                                ? analyzed.IndexOf(connects[1])
                                : analyzed.IndexOf(connects[0]));
                            ge.ConnectedTo.Add(empty.Number);
                            if (analyzed[i + 1].BranchType == BranchType.IfEnd || analyzed[i + 1].BranchType == BranchType.Else)
                            {
                                empty.ConnectedTo.Add(analyzed.IndexOf(connects[0]));
                            }
                            else
                            {
                                empty.ConnectedTo.Add(i + 1);
                                //TODO RETURN
                            }
                            graphEntities.Add(empty);
                        }
                    }
                    else if (item.BranchType == BranchType.Else)
                    {

                        ge.Type = GraphEntityTypesEnum.Else;
                        var empty = new GraphEntity()
                        {
                            Type = GraphEntityTypesEnum.Empty,
                            LineNumber = item.LineNumber + 1,
                            Number = number
                        };
                        number--;
                        ge.ConnectedTo.Add(empty.Number);
                        empty.ConnectedTo.AddRange(analyzed
                            .Where(x => x.ConnectedFrom.Any(y => y == item.LineNumber))?
                            .Select(analyzed.IndexOf));
                        graphEntities.Add(empty);
                    }
                    else if (item.BranchType == BranchType.ForOrWhile)
                    {
                        innert.Push(ge);

                        ge.Type = GraphEntityTypesEnum.Cycle;
                        var empty = new GraphEntity()
                        {
                            Type = GraphEntityTypesEnum.Empty,
                            LineNumber = item.LineNumber + 1,
                            Number = number
                        };
                        number--;
                        ge.ConnectedTo.Add(empty.Number);
                        if (analyzed[i + 1].BranchType == BranchType.ForOrWhileEnd)
                            empty.ConnectedTo.Add(ge.Number);
                        else
                        {
                            empty.ConnectedTo.Add(i + 1);
                            //TODO: ADD RETURN CONNECTION
                        }
                        ge.ConnectedTo.AddRange(analyzed
                            .Where(x => x.ConnectedFrom.Any(y => y == item.LineNumber))?
                            .Select(analyzed.IndexOf));
                        graphEntities.Add(empty);
                    }
                    else if (item.BranchType == BranchType.DoWhile)
                    {
                        innert.Push(ge);
                        ge.Type = GraphEntityTypesEnum.Cycle;
                        ge.Comment = "do";
                    }
                    else if (item.BranchType == BranchType.IfEnd)
                    {
                        innert.Pop();
                        ge.Type = GraphEntityTypesEnum.Empty;
                    }
                    else if (item.BranchType == BranchType.DoWhileEnd)
                    {
                        innert.Pop();

                        ge.Type = GraphEntityTypesEnum.Empty;
                        ge.ConnectedTo.AddRange(analyzed
                            .Where(x => x.LineNumber == item.ConnectedFrom[0])
                            ?.Select(analyzed.IndexOf));
                    }
                    else if (item.BranchType == BranchType.ForOrWhileEnd)
                    {
                        lastInline = innert.Pop();
                        if (graphEntities.Last() != lastInline)
                        {
                            graphEntities.Last().ConnectedTo = new List<int>()
                                {
                                    lastInline.Number
                                };
                        }
                        ge.Type = GraphEntityTypesEnum.Empty;
                    }

                    var emptyEnt = new GraphEntity()
                    {
                        Type = GraphEntityTypesEnum.Empty,
                        LineNumber = item.LineNumber + 1,
                        Number = number,
                    };
                    if (i + 1 < analyzed.Length)
                    {
                        emptyEnt.ConnectedTo.Add(i + 1);
                    }

                    if (item.BranchType != BranchType.If && item.BranchType != BranchType.Else && item.BranchType != BranchType.ForOrWhile &&
                        item.IsProcedure != false)
                    {
                        number--;
                        ge.ConnectedTo.Add(emptyEnt.Number);
                    }

                    graphEntities.Add(ge);

                    if (item.BranchType != BranchType.If && item.BranchType != BranchType.Else && item.BranchType != BranchType.ForOrWhile &&
                        item.IsProcedure != false)
                        graphEntities.Add(emptyEnt);
                }

                metrics.GraphEntities.Add(
                    module.Path, graphEntities
                );
            }

            metrics.ComplexityNumber = metrics.GraphEntities.Values.SelectMany(x => x).Count(x =>

                                           x.Type == GraphEntityTypesEnum.If ||

                                           x.Type == GraphEntityTypesEnum.Cycle) + 1;
            return metrics;
        }
    }
}