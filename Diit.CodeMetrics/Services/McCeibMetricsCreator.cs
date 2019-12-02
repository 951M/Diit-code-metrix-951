using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
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
        private Stack<int> nextEndIf = new Stack<int>();
        private Stack<int> lastForOrWhile = new Stack<int>();
        int additionEntityIndex = -1;

        public McCeibMetricsCreator(ILexicalAnalyzer<IMcCeibMetrics> lexicalAnalyzer)
        {
            _lexicalAnalyzer = lexicalAnalyzer;
        }

        public IMcCeibMetrics CreateMetrics(List<Module> source)
        {
            IMcCeibMetrics metrics = new Metrics();
            metrics.GraphEntities = new Dictionary<string, List<GraphEntity>>();
            foreach (var module in source)
            {
                metrics.GraphEntities.Add(
                    module.Path, BuildGraphForModule(module)
                );
            }

            metrics.ComplexityNumber = metrics.GraphEntities.Values.SelectMany(x => x).Count(x =>
                                           x.Type == GraphEntityTypesEnum.If ||
                                           x.Type == GraphEntityTypesEnum.Cycle) + 1;
            return metrics;
        }        

        private List<GraphEntity> BuildGraphForModule(Module module)
        {
            AnalyzerItem[] analyzed = _lexicalAnalyzer.AnalyzeSource(Encoding.UTF8.GetString(module.Source)).ToArray();
            List<GraphEntity> graphEntities = new List<GraphEntity>();
            additionEntityIndex = -1;
            nextEndIf.Clear();
            lastForOrWhile.Clear();

            for (int i = 0; i < analyzed.Length; i++)
            {
                if (analyzed[i].IsProcedure != null)
                {
                    graphEntities.Add(GetProcedureGraphEntity(analyzed[i], i, analyzed.Length));
                    continue;
                }

                switch(analyzed[i].BranchType) 
                {
                    case BranchType.If:
                        List<GraphEntity> ifEntities = GetIfGraphEntities(analyzed, i);
                        foreach (var entity in ifEntities)
                        {
                            graphEntities.Add(entity);
                        }
                        break;
                    case BranchType.Else:
                        Debug.Assert(i + 1 < analyzed.Length);
                        GraphEntity elseGraphEntity = GetElseGraphEntity(analyzed[i], i, analyzed.Length, analyzed[i + 1].BranchType);
                        if (elseGraphEntity != null)
                        {
                            graphEntities.Add(elseGraphEntity);
                        }
                        break;
                    case BranchType.IfEnd:
                        Debug.Assert(i + 1 < analyzed.Length);
                        graphEntities.Add(GetEndIfGraphEntity(analyzed[i], i, analyzed.Length, analyzed[i + 1].BranchType));
                        break;
                    case BranchType.ForOrWhile:
                        List<GraphEntity> cycleEntities = GetPreConditionCycleGraphEntities(analyzed, i);
                        foreach (var entity in cycleEntities)
                        {
                            graphEntities.Add(entity);
                        }
                        break;
                    case BranchType.ForOrWhileEnd:
                        //no need to process this item
                        break;
                    case BranchType.DoWhile:
                    case BranchType.DoWhileEnd:
                        //TODO: process post condition cycle 
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            return graphEntities;
        }

        /* Creates and returns graphEntity for procedure begin (if item.isProcedure is true)
        * or end (if item.isProcedure is false) with unique number
        */
        private GraphEntity GetProcedureGraphEntity(AnalyzerItem item, int number, int total)
        {
            Debug.Assert(item.IsProcedure != null);

            GraphEntity newGraphEntity = new GraphEntity()
            {
                LineNumber = item.LineNumber,
                Number = number,
            };

            if (item.IsProcedure == false)
            {
                newGraphEntity.Type = GraphEntityTypesEnum.EndProcedure;
                newGraphEntity.Comment = "return";
            }
            else
            {
                newGraphEntity.Type = GraphEntityTypesEnum.StartProcedure;
                newGraphEntity.Comment = item.Comment;
                if (number + 1 < total)
                {
                    newGraphEntity.ConnectedTo.Add(number + 1);
                }
            }
            return newGraphEntity;
        }

        /*
         * Creates if entity and trueif entity if there is no control construction in it
         * TODO: pass connects and its idicies in analyzed instead of analyzed
         * */
        private List<GraphEntity> GetIfGraphEntities(AnalyzerItem[] analyzed, int number)
        {
            List<GraphEntity> ifEntities = new List<GraphEntity>();

            GraphEntity newGraphEntity = new GraphEntity()
            {
                LineNumber = analyzed[number].LineNumber,
                Number = number,
            };

            newGraphEntity.Type = GraphEntityTypesEnum.If;

            /*IF-block is connected to one item if there is no ELSE-block 
             * or to 2 items in the other case*/
            List<AnalyzerItem> connects = analyzed
                .Where(x => x.ConnectedFrom.Any(y => y == analyzed[number].LineNumber))?
                .OrderBy(x => x.BranchType)
                .ToList();

            Debug.Assert(connects.Count == 2 || connects.Count == 1 && connects[0].ConnectedFrom.Count == 2);

            //else-node (connects[1]) or endif-node (connects[0])
            int nextItemIndex = analyzed.IndexOf(connects[connects.Count - 1]);

            //connect with trueif-block
            if (nextItemIndex - number == 1) //that means that there are no operators between if and next node
            {
                GraphEntity trueIfActions = new GraphEntity()
                {
                    Type = GraphEntityTypesEnum.Nested,
                    LineNumber = analyzed[number].LineNumber + 1,
                    Comment = "True if block",
                    Number = additionEntityIndex--
                };
                newGraphEntity.ConnectedTo.Add(trueIfActions.Number);
                trueIfActions.ConnectedTo.Add(analyzed.IndexOf(connects[0]));
                ifEntities.Add(trueIfActions);
            }
            else
            {
                newGraphEntity.ConnectedTo.Add(number + 1);
            }

            //connect to else or endif
            if (connects.Count == 2 && analyzed[nextItemIndex + 1].BranchType != BranchType.IfEnd)
            {
                newGraphEntity.ConnectedTo.Add(nextItemIndex + 1);
            }
            else
            {
                newGraphEntity.ConnectedTo.Add(nextItemIndex);
            }

            nextEndIf.Push(analyzed.IndexOf(connects[0]));
            ifEntities.Add(newGraphEntity);
            return ifEntities;
        }

        /*
         * Creates else entity if there is no control construction in it
         * */
        private GraphEntity GetElseGraphEntity(AnalyzerItem item, int number, int total, BranchType nextItemType)
        {
            if (nextItemType == BranchType.IfEnd)
            {
                GraphEntity newGraphEntity = new GraphEntity()
                {
                    LineNumber = item.LineNumber,
                    Number = number,
                    Type = GraphEntityTypesEnum.Nested,
                    Comment = "Else block"
                };
                if (number + 1 < total)
                {
                    newGraphEntity.ConnectedTo.Add(number + 1);
                }
                return newGraphEntity;
            }
            return null;
        }

        private GraphEntity GetEndIfGraphEntity(AnalyzerItem item, int number, int total, BranchType nextItemType)
        {
            GraphEntity newGraphEntity = new GraphEntity()
            {
                LineNumber = item.LineNumber,
                Number = number,
            };
            nextEndIf.Pop();
            newGraphEntity.Type = GraphEntityTypesEnum.Empty;
            newGraphEntity.Comment = "End if";

            if (number + 1 < total)
            {
                if (nextItemType == BranchType.Else)
                {
                    int temp = nextEndIf.Pop();
                    nextEndIf.Push(temp);
                    newGraphEntity.ConnectedTo.Add(temp);
                }
                else if (nextItemType == BranchType.ForOrWhileEnd)
                {
                    newGraphEntity.ConnectedTo.Add(lastForOrWhile.Pop());
                }
                else
                {
                    newGraphEntity.ConnectedTo.Add(number + 1);
                }
            }
            return newGraphEntity;
        }

        /*
         * Creates cycle entity and body entity if there is no control construction in it
         * TODO: pass connects and its idicies in analyzed instead of analyzed
         * */
        private List<GraphEntity> GetPreConditionCycleGraphEntities(AnalyzerItem[] analyzed, int number)
        {
            List<GraphEntity> cycleEntities = new List<GraphEntity>();

            GraphEntity newGraphEntity = new GraphEntity()
            {
                LineNumber = analyzed[number].LineNumber,
                Number = number,
            };
            newGraphEntity.Type = GraphEntityTypesEnum.Cycle;
            List<AnalyzerItem> connects = analyzed
               .Where(x => x.ConnectedFrom.Any(y => y == analyzed[number].LineNumber))?
               .OrderBy(x => x.BranchType)
               .ToList();
            Debug.Assert(connects.Count == 1);
            lastForOrWhile.Push(number);

            if (analyzed[analyzed.IndexOf(connects[0]) + 1].BranchType == BranchType.Else)
            {
                int temp = nextEndIf.Pop();
                nextEndIf.Push(temp);
                newGraphEntity.ConnectedTo.Add(temp);
            }
            else
            {
                newGraphEntity.ConnectedTo.Add(analyzed.IndexOf(connects[0]) + 1);
            }

            //create new entity it there is no control constructions inside cycle
            if (analyzed[number + 1].BranchType == BranchType.ForOrWhileEnd)
            {
                GraphEntity cycleActions = new GraphEntity()
                {
                    Type = GraphEntityTypesEnum.Nested,
                    LineNumber = analyzed[number].LineNumber + 1,
                    Number = additionEntityIndex--,
                    Comment = "Cycle's body"
                };
                newGraphEntity.ConnectedTo.Add(cycleActions.Number);
                cycleActions.ConnectedTo.Add(newGraphEntity.Number);
                cycleEntities.Add(cycleActions);
            }
            else
            {
                newGraphEntity.ConnectedTo.Add(number + 1);
            }
            cycleEntities.Add(newGraphEntity);
            return cycleEntities;
        }



        //public IMcCeibMetrics CreateMetrics(List<Module> source)
        //{
        //    IMcCeibMetrics metrics = new Metrics();
        //    metrics.GraphEntities = new Dictionary<string, List<GraphEntity>>();
        //    foreach (var module in source)
        //    {
        //        buildGraphForModule(module);
        //        var analyzed = _lexicalAnalyzer.AnalyzeSource(Encoding.UTF8.GetString(module.Source)).ToArray();
        //        List<GraphEntity> graphEntities = new List<GraphEntity>();
        //        Stack<GraphEntity> innert = new Stack<GraphEntity>();
        //        Stack<int> nextEndIf = new Stack<int>();
        //        Stack<int> lastForOrWhile = new Stack<int>();
        //        int entityIndex = 0;
        //        int additionEntityIndex = -1;
        //        for (int i = 0; i < analyzed.Length; i++)
        //        {
        //            if (analyzed[i].BranchType == BranchType.Else && analyzed[i + 1].BranchType != BranchType.IfEnd)
        //            {
        //                continue;
        //            }
        //            AnalyzerItem item = analyzed[i];

        //            GraphEntity newGraphEntity = new GraphEntity()
        //            {
        //                LineNumber = item.LineNumber,
        //                Number = i,
        //            };


        //            if (item.IsProcedure == true)
        //            {
        //                newGraphEntity.Type = GraphEntityTypesEnum.StartProcedure;
        //                newGraphEntity.Comment = item.Comment;
        //            }
        //            else if (item.IsProcedure == false)
        //            {
        //                newGraphEntity.Type = GraphEntityTypesEnum.EndProcedure;
        //            }
        //            else if (item.BranchType == BranchType.If)
        //            {
        //                newGraphEntity.Type = GraphEntityTypesEnum.If;

        //                /*IF-block is connected to one item if there is no ELSE-block 
        //                 * or to 2 items in the other case*/
        //                List<AnalyzerItem> connects = analyzed
        //                    .Where(x => x.ConnectedFrom.Any(y => y == item.LineNumber))?
        //                    .OrderBy(x => x.BranchType)
        //                    .ToList();

        //                Debug.Assert(connects.Count == 2 || connects.Count == 1 && connects[0].ConnectedFrom.Count == 2);

        //                //else-node (connects[1]) or endif-node (connects[0])
        //                int nextItemIndex = analyzed.IndexOf(connects[connects.Count - 1]);

        //                //connect with trueif-block
        //                if (nextItemIndex - i == 1) //that means that there are no operators between if and next node
        //                {
        //                    GraphEntity trueIfActions = new GraphEntity()
        //                    {
        //                        Type = GraphEntityTypesEnum.Nested,
        //                        LineNumber = item.LineNumber + 1,
        //                        Comment = "True if block",
        //                        Number = additionEntityIndex--
        //                    };
        //                    newGraphEntity.ConnectedTo.Add(trueIfActions.Number);
        //                    trueIfActions.ConnectedTo.Add(analyzed.IndexOf(connects[0]));
        //                    graphEntities.Add(trueIfActions);
        //                }
        //                else
        //                {
        //                    newGraphEntity.ConnectedTo.Add(i + 1);
        //                }

        //                //connect to else or endif
        //                if (connects.Count == 2 && analyzed[nextItemIndex + 1].BranchType != BranchType.IfEnd)
        //                {
        //                    newGraphEntity.ConnectedTo.Add(nextItemIndex + 1);
        //                }
        //                else
        //                {
        //                    newGraphEntity.ConnectedTo.Add(nextItemIndex);
        //                }

        //                // innert.Push(newGraphEntity);
        //                //save connected 0
        //                nextEndIf.Push(analyzed.IndexOf(connects[0]));
        //                graphEntities.Add(newGraphEntity);
        //                continue;
        //            }
        //            else if (item.BranchType == BranchType.Else)
        //            {
        //                newGraphEntity.Type = GraphEntityTypesEnum.Else;
        //                newGraphEntity.Comment = "Else block";
        //            }
        //            else if (item.BranchType == BranchType.IfEnd)
        //            {
        //                nextEndIf.Pop();
        //                newGraphEntity.Type = GraphEntityTypesEnum.Empty;
        //                newGraphEntity.Comment = "End if";
        //            }
        //            else if (item.BranchType == BranchType.ForOrWhile)
        //            {
        //                newGraphEntity.Type = GraphEntityTypesEnum.Cycle;
        //                List<AnalyzerItem> connects = analyzed
        //                   .Where(x => x.ConnectedFrom.Any(y => y == item.LineNumber))?
        //                   .OrderBy(x => x.BranchType)
        //                   .ToList();
        //                Debug.Assert(connects.Count == 1);
        //                lastForOrWhile.Push(i);

        //                if (analyzed[analyzed.IndexOf(connects[0]) + 1].BranchType == BranchType.Else)
        //                {
        //                    int temp = nextEndIf.Pop();
        //                    nextEndIf.Push(temp);
        //                    newGraphEntity.ConnectedTo.Add(temp);
        //                }
        //                else
        //                {
        //                    newGraphEntity.ConnectedTo.Add(analyzed.IndexOf(connects[0]) + 1);
        //                }

        //                if (analyzed[i + 1].BranchType == BranchType.ForOrWhileEnd)
        //                {
        //                    GraphEntity cycleActions = new GraphEntity()
        //                    {
        //                        Type = GraphEntityTypesEnum.Nested,
        //                        LineNumber = item.LineNumber + 1,
        //                        Number = additionEntityIndex--
        //                    };
        //                    newGraphEntity.ConnectedTo.Add(cycleActions.Number);
        //                    cycleActions.ConnectedTo.Add(newGraphEntity.Number);
        //                    graphEntities.Add(cycleActions);
        //                    i++;
        //                }
        //                else
        //                {
        //                    newGraphEntity.ConnectedTo.Add(i + 1);
        //                }
        //                graphEntities.Add(newGraphEntity);
        //                continue;
        //            }
        //            //else if (item.BranchType == BranchType.DoWhile)
        //            //{
        //            //    innert.Push(newGraphEntity);
        //            //    newGraphEntity.Type = GraphEntityTypesEnum.Cycle;
        //            //    newGraphEntity.Comment = "do";
        //            //}
        //            //else if (item.BranchType == BranchType.DoWhileEnd)
        //            //{
        //            //    innert.Pop();

        //            //    newGraphEntity.Type = GraphEntityTypesEnum.Empty;
        //            //    newGraphEntity.ConnectedTo.AddRange(analyzed
        //            //        .Where(x => x.LineNumber == item.ConnectedFrom[0])
        //            //        ?.Select(analyzed.IndexOf));
        //            //}



        //            if (i + 1 < analyzed.Length)
        //            {
        //                if (analyzed[i + 1].BranchType == BranchType.Else)
        //                {
        //                    int temp = nextEndIf.Pop();
        //                    nextEndIf.Push(temp);
        //                    newGraphEntity.ConnectedTo.Add(temp);
        //                }
        //                else if (analyzed[i + 1].BranchType == BranchType.ForOrWhileEnd)
        //                {
        //                    newGraphEntity.ConnectedTo.Add(lastForOrWhile.Pop());
        //                    i++;
        //                }
        //                else
        //                {
        //                    newGraphEntity.ConnectedTo.Add(i + 1);
        //                }
        //            }
        //            graphEntities.Add(newGraphEntity);


        //            //var emptyEnt = new GraphEntity()
        //            //{
        //            //    Type = GraphEntityTypesEnum.Empty,
        //            //    LineNumber = item.LineNumber + 1,
        //            //    Number = number,
        //            //};
        //            //     if (i + 1 < analyzed.Length)
        //            //    {
        //            // emptyEnt.ConnectedTo.Add(i + 1);
        //            // newGraphEntity.ConnectedTo.Add(i + 1);
        //            //   }

        //            //if (item.BranchType != BranchType.If && item.BranchType != BranchType.Else && item.BranchType != BranchType.ForOrWhile &&
        //            //    item.IsProcedure != false)
        //            //{
        //            //    number--;
        //            //    newGraphEntity.ConnectedTo.Add(emptyEnt.Number);
        //            //}



        //            //if (item.BranchType != BranchType.If && item.BranchType != BranchType.Else && item.BranchType != BranchType.ForOrWhile &&
        //            //    item.IsProcedure != false)
        //            //    graphEntities.Add(emptyEnt);
        //        }

        //        metrics.GraphEntities.Add(
        //            module.Path, graphEntities
        //        );
        //    }

        //    metrics.ComplexityNumber = metrics.GraphEntities.Values.SelectMany(x => x).Count(x =>

        //                                   x.Type == GraphEntityTypesEnum.If ||

        //                                   x.Type == GraphEntityTypesEnum.Cycle) + 1;
        //    return metrics;
        //}

    }
}