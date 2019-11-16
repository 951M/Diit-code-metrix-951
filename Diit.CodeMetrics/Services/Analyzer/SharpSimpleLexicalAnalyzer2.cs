using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Diit.CodeMetrics.Data;

namespace Diit.CodeMetrics.Services.Analyzer
{
    public class SharpSimpleLexicalAnalyzer2 : ILexicalAnalyzer<IMcCeibMetrics>
    {
        private static readonly Regex startProcedure =
            new Regex("(^|;)\\s*((private|public|protected)\\s+)?((virtual)\\s+)?((async)\\s+)?([\\w\\d-_<>]+)\\s+([\\w\\d-_]+)\\s*\\(.*\\)");

        public IEnumerable<AnalyzerItem> AnalyzeSource(string source)
        {
            List<AnalyzerBlock> blockStack = new List<AnalyzerBlock>();
            var lines = source.Split("\r\n")
                .SelectMany(x=>x.Split("\n"))
                .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].TrimStart().StartsWith("//") || lines[i].TrimStart().StartsWith("/*"))
                    continue;
                var line = lines[i].Split('\t', ' ', '(', ')', '+',',','-','*','/');
                var matches = startProcedure.Matches(lines[i]);
                var last = blockStack.LastOrDefault();
                    if (line.Contains("if"))
                {
                    var block = new AnalyzerBlock
                    {
                        BranchType = BranchType.If,
                        LineNumber = i
                    };
                    blockStack.Add(block);
                    yield return new AnalyzerItem {BranchType = BranchType.If, LineNumber = i};
                }
                else if (line.Contains("for"))
                {
                    var block = new AnalyzerBlock
                    {
                        BranchType = BranchType.ForOrWhile,
                        LineNumber = i
                    };
                    blockStack.Add(block);
                    yield return new AnalyzerItem {BranchType = BranchType.ForOrWhile, LineNumber = i, Comment = "for"};
                }

                else if (line.Contains("while"))
                {
                    var block = new AnalyzerBlock
                        {
                            BranchType = BranchType.ForOrWhile,
                            LineNumber = i
                        };
                        blockStack.Add(block);
                        yield return new AnalyzerItem {BranchType = BranchType.ForOrWhile, LineNumber = i, Comment = "while"};
                    
                }
                else if (matches.Count > 0)
                {
                    if (matches[0].Groups?[8]?.Value == "new") continue;
                    var block = new AnalyzerBlock
                    {
                        IsStart = true,
                        LineNumber = i
                    };
                    blockStack.Add(block);
                    yield return new AnalyzerItem {IsProcedure = true, LineNumber = i, Comment = matches[0].Groups?[9]?.Value ?? String.Empty };
                }
                else if (line.Contains("do"))
                {
                    var block = new AnalyzerBlock
                    {
                        BranchType = BranchType.DoWhile,
                        LineNumber = i
                    };
                    blockStack.Add(block);
                    yield return new AnalyzerItem {BranchType = BranchType.DoWhile, LineNumber = i};
                }
                else if (line.Contains("{"))
                {
                    if (last == null || last.IsStart == false && last.BranchType == BranchType.None)
                        blockStack.Add(new AnalyzerBlock() {LineNumber = i});
                }
                else if (line.Contains("}"))
                {
                    if (last != null)
                    {
                        blockStack.Remove(last);
                        if (last.BranchType == BranchType.If)
                        {
                            if (lines[i + 1].Contains("else"))
                            {
                                i++;
                                blockStack.Add(new AnalyzerBlock()
                                {
                                    BranchType = BranchType.Else,
                                    LineNumber = i,
                                    ConnectedFromLine = new List<int>() {last.LineNumber}
                                });
                                yield return new AnalyzerItem()
                                {
                                    BranchType = BranchType.Else,
                                    LineNumber = i,
                                    ConnectedFrom = new List<int>() {last.LineNumber}
                                };
                            }
                            else
                            {
                                yield return new AnalyzerItem()
                                {
                                    BranchType = BranchType.IfEnd,
                                    LineNumber = i,
                                    ConnectedFrom = new List<int>() {last.LineNumber, last.LineNumber}
                                };
                            }
                        }
                        else if (last.BranchType == BranchType.DoWhile)
                        {
                            if (lines[i+1].Contains("while"))
                            {
                                i++;
                                yield return new AnalyzerItem
                                {
                                    BranchType = BranchType.DoWhileEnd, LineNumber = i,
                                    ConnectedFrom = new List<int>{last.LineNumber},
                                    Comment = "while"
                                };
                            }
                        }
                        else if (last.BranchType == BranchType.Else)
                        {
                            yield return new AnalyzerItem()
                            {
                                BranchType = BranchType.IfEnd,
                                LineNumber = i,
                                ConnectedFrom = new List<int>() {last.LineNumber, last.ConnectedFromLine[0]}
                            };
                        }
                        else if (last.BranchType == BranchType.ForOrWhile)
                        {
                            yield return new AnalyzerItem()
                            {
                                BranchType = BranchType.ForOrWhileEnd,
                                LineNumber = i,
                                ConnectedFrom = new List<int>() {last.LineNumber, last.LineNumber}
                            };
                        }
                        else if (last.IsStart)
                        {
                            yield return new AnalyzerItem()
                            {
                                LineNumber = i,
                                IsProcedure = false,
                            };
                        }
                    }
                }
            }
        }
    }
}