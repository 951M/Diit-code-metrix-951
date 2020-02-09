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
    public class SharpChepiraAnalyzer : ILexicalAnalyzer<IChepinaMetrics>
    {
        private readonly IHostingEnvironment _inv;

        int deliter = 1;

        static string[] s_operators = {
                                "-"
                                ,"+"
                                ,"*"
                                ,"/"
                                ,"%"
                                ,"--"
                                ,"++"
                                ,"+="
                                ,"-="
                                ,"*="
                                ,"/="
                                ,"%="
                                ,">"
                                ,">="
                                ,"<"
                                ,"<="
                                ,"=="
                                ,"="
                                ,"!="
                                ,"&&"
                                ,"!"
                                ,"&"
                                ,"|"
                                ,"^"
                                ,"~"
                                ,">>"
                                ,"<<"
                                ,"?"
                                ,","
                                ,"."
                                ,"if"
                                ,"else"
                                ,"while"
                                ,"for"
                                ,"foreach"
                                ,"break"
                                ,"continue"
                                ,"switch"
                                ,"case"
                                ,"return"
};

        public SharpChepiraAnalyzer(IHostingEnvironment inv)
        {

            types.Add("bool");
            types.Add("char");
            types.Add("double");
            types.Add("int");
            types.Add("long");
            types.Add("short");
            types.Add("float");
            _inv = inv;
            try
            {
                ReadReservChars();
                ReadReservWord();
            }
            catch
            {

            }
        }

        ReservedChar[] Chars;
        ReservedWord[] Words;
        string[] temp_types, temp_literals; //temp_for_syntax;
        string readText, resultText;
        int counterVariables, operatorsCounter, operandsCounter;
        ArrayList uniqueOperators = new ArrayList();
        ArrayList numberOperators = new ArrayList();
        ArrayList uniqueOperands = new ArrayList();
        ArrayList numberOperands = new ArrayList();
        int commentOperatorAllNumber = 0;
        int commentCounter = 0;
        List<int> commentBlockCounter = new List<int>();
        List<int> operatorsBlockCounter = new List<int>();
        int linesNumber = 0;

        ArrayList types = new ArrayList();
        public int[,] variables = new int[1, 2];
        int lengthString;

        //Holsted holsted = new Holsted();

        //--------------------------------------------------------------------------------------

        struct ReservedChar
        {
            public int _index;
            public string _char;
            public int _next;
        }
        //--------------------------------------------------------------------------------------

        struct ReservedWord
        {
            public int _index;
            public string _char;
            public int _next;
            public bool flag;
        }


        private void ReadReservChars()
        {
            lengthString = 0;
            string path = _inv.ContentRootPath;
            string filename = Path.Combine(path, "Analyzer", "Chars.txt");
            StreamReader sr = new StreamReader(filename);
            while (!sr.EndOfStream)
            {
                sr.ReadLine();
                lengthString++;
            }
            Chars = new ReservedChar[lengthString];
            sr = new StreamReader(filename);
            for (int i = 0; i < lengthString; i++)
            {
                string tmp = sr.ReadLine();
                string[] temp = tmp.Split(new Char[] { ' ', '\r', '\t', '\n' });
                Chars[i]._index = Convert.ToInt32(temp[0]);
                Chars[i]._char = temp[1];
                Chars[i]._next = Convert.ToInt32(temp[2]);
            }
        }
        //--------------------------------------------------------------------------------------

        private void ReadReservWord()
        {
            lengthString = 0;
            string path = _inv.ContentRootPath;
            string filename = Path.Combine(path, "Analyzer", "Words.txt");
            StreamReader sr = new StreamReader(filename);
            while (!sr.EndOfStream)
            {
                sr.ReadLine();
                lengthString++;
            }
            Words = new ReservedWord[lengthString];
            sr = new StreamReader(filename);
            for (int i = 0; i < lengthString; i++)
            {
                string tmp = sr.ReadLine();
                string[] temp = tmp.Split(new Char[] { ' ', '\r', '\t', '\n' });
                Words[i]._index = Convert.ToInt32(temp[0]);
                Words[i]._char = temp[1];
                Words[i]._next = Convert.ToInt32(temp[2]);
                if (i > 0)
                    if (Words[i]._index == Words[i - 1]._index)
                    {
                        Words[i - 1].flag = true;
                    }
            }
        }
        //--------------------------------------------------------------------------------------

        private void CompareUniqueOperators(int operatorIndex)
        {
            for (int i = 0; i < uniqueOperators.Count; i++)
            {
                if (uniqueOperators.Contains(operatorIndex))
                {
                    int index = uniqueOperators.IndexOf(operatorIndex);
                    numberOperators.Insert(index, (int)numberOperators[index] + 1);
                    return;
                }
            }
            uniqueOperators.Add(operatorIndex);
            numberOperators.Add(1);
        }
        //--------------------------------------------------------------------------------------

        private void CompareUniqueOperands(string value)
        {
            for (int i = 0; i < uniqueOperands.Count; i++)
            {
                if (uniqueOperands.Contains(value))
                {
                    int index = uniqueOperands.IndexOf(value);
                    numberOperands.Insert(index, (int)numberOperands[index] + 1);
                    return;
                }
            }
            uniqueOperands.Add(value);
            numberOperands.Add(1);
        }
        //--------------------------------------------------------------------------------------

        private string ParseCode(string source)
        {
            int i = 0;
            bool multi_comment = false;
            bool line_comment = false;

            source.Replace(" ", "");
            source.Replace("\t", "");

            while(i < source.Length)
            {
                if (i + 1 < source.Length)
                {
                    if (source[i].ToString() + source[i + 1].ToString() == "/*" && !line_comment)
                    {
                        multi_comment = true;
                    }
                    else if (source[i].ToString() + source[i + 1].ToString() == "*/" && multi_comment)
                    {
                        multi_comment = false;
                    }
                    else if (source[i].ToString() + source[i + 1].ToString() == "//" && !multi_comment)
                    {
                        line_comment = true;
                    }
                    else if (source[i].ToString() == "\n" && line_comment)
                    {
                        line_comment = false;
                    }
                }
                if (!line_comment && !multi_comment)
                {
                    if (i + 1 < source.Length &&
                        (source[i].ToString() + source[i + 1].ToString() == "&&"
                        || source[i].ToString() + source[i + 1].ToString() == "||"))
                    {
                        source = source.Substring(0, i + 2) + "\n" + source.Substring(i + 2);
                        i += 3;
                    }
                    else if (source[i].ToString() == "{" || source[i].ToString() == "," || source[i].ToString() == "}" || source[i].ToString() == ";"
                    || source[i].ToString() == "^" || source[i].ToString() == "&" || source[i].ToString() == "|")
                    {
                        source = source.Substring(0, i + 1) + "\n" + source.Substring(i + 1);
                        i += 2;
                    }
                }
                i += 1;
            }
            i = 0;
            while (i < source.Length)
            {
                if (i + 1 < source.Length)
                    if (source[i].ToString() + source[i + 1].ToString() == "\n\n")
                    {
                        source = source.Substring(0, i) + source.Substring(i + 1);
                        i -= 1;
                    }
                i += 1;
            }

            return source;
        }
        
        private void AnalysisComments(string source, ref bool isprime, ref double a_coef)
        {
            commentBlockCounter.Clear();
            operatorsBlockCounter.Clear();
            commentCounter = 0;
            var lines = source.Split('\n');
            linesNumber = lines.Length;
            bool isComment = false;

            int temp_line_counter = 0;
            int temp_commentCounter = 0;

            isprime = true;
            for (int i = 2; i < linesNumber; i++)
            {
                if (linesNumber % i == 0)
                {
                    isprime = false;
                    deliter = i;
                    if (i >= 10)
                    {
                        break;
                    }
                }
            }

            if (isprime)
            { 
                deliter = linesNumber; 
            }

            a_coef = 0f;

            if (deliter == linesNumber)
            {
                // This is true when line number is prime number
                a_coef = 1f / (deliter % 10) - 0.1f;
                deliter = 10;
            }
            else
                a_coef = 1f / (deliter) - 0.1f;

            foreach (var line in lines)
            {
                temp_line_counter++;
                for (int i = 0; i < line.Length - 1; ++i)
                {
                    if (isComment)
                    {
                        if (line[i] == '*' && line[i + 1] == '/')
                        {
                            isComment = false;
                        }
                    }
                    else
                    {
                        if (line[i] == '/' && line[i + 1] == '/')
                        {
                            commentCounter++;
                            temp_commentCounter++;
                            break;
                        }

                        if (line[i] == '/' && line[i + 1] == '*')
                        {
                            commentCounter++;
                            temp_commentCounter++;
                            isComment = true;
                        }
                    }
                }
                if (temp_line_counter == deliter)
                {
                    commentBlockCounter.Add(temp_commentCounter);
                    temp_line_counter = 0;
                    temp_commentCounter = 0;
                }
            }
        }

        private void AnalysisWordsAndChars(string source)
        {
            operatorsCounter = 0;
            operandsCounter = 0;
            resultText = "";
            readText = source;
            string temp = "";
            int indexChars = 1;
            int indexWords = 1;
            bool flag;
            bool isComment = false;
            int lineCounter = 1;
            int lastSlashNIndex = 0;
            int tempOperatorNumber = 0;
            for (int i = 0; i < readText.Length; i++) // проходим по всем символам одной строки
            {
                if (readText[i] == '\n' && i > lastSlashNIndex)
                {
                    lastSlashNIndex = i;
                    lineCounter++;
                }

                if (lineCounter == deliter + 1)
                {
                    operatorsBlockCounter.Add(tempOperatorNumber);
                    tempOperatorNumber = 0;
                    lineCounter = 1;
                }

                if (isComment)
                {
                    if (i != readText.Length - 1)
                    if (readText[i] == '*' && readText[i + 1] == '/')
                    {
                        isComment = false;
                    }

                    continue;
                }
                else
                {
                    if (i != readText.Length - 1)
                    if (readText[i] == '/' && readText[i + 1] == '/')
                    {
                        commentCounter++;
                            for (; i < readText.Length; i++)
                                if (readText[i] == '\n')
                                {
                                    i--;
                                    break;
                                }

                            if (i >= readText.Length)
                                break;
                    }

                    if (i < readText.Length)
                    if (readText[i] == '/' && readText[i + 1] == '*')
                    {
                        commentCounter++;
                        isComment = true;
                    }
                }

                flag = true;
                if (indexChars == 1)
                {
                    foreach (ReservedWord word in Words)
                    {
                        if (word._index == indexWords && word._next == 0 && !word.flag)
                        {
                            flag = true;
                            resultText += "(1/" + word._index + ")";
                            if (!types.Contains(temp))
                            {
                                CompareUniqueOperators(word._index);
                                operatorsCounter++;
                            }
                            if (s_operators.Contains(temp) && i != 0 && readText[i - 1] != '*' && readText[i - 1] != '/')
                            {
                                tempOperatorNumber++;
                                commentOperatorAllNumber++;
                            }
                            temp = "";
                            indexWords = 1;
                            break;
                        }
                        else if (word._index == indexWords && readText[i].Equals(Convert.ToChar(word._char)) && indexChars == 1)
                        {
                            flag = false;
                            temp += readText[i];
                            indexWords = word._next;
                            break;
                        }
                        else if (word._index == indexWords && !readText[i].Equals(Convert.ToChar(word._char)) && temp != "" && indexChars == 1 && word.flag)
                        {
                            continue;
                        }
                        else if (word._index == indexWords && !readText[i].Equals(Convert.ToChar(word._char)) && temp != "" && indexChars == 1)
                        {
                            flag = true;
                            resultText += temp;
                            if (s_operators.Contains(temp) && i != 0 && readText[i - 1] != '*' && readText[i - 1] != '/')
                            {
                                tempOperatorNumber++;
                                commentOperatorAllNumber++;
                            }
                            temp = "";
                            indexWords = 1;
                            break;
                        }
                    }
                }
                if (indexWords == 1)
                {
                    foreach (ReservedChar @char in Chars)
                    {
                        if (@char._index == indexChars && @char._next == 0)
                        {
                            flag = false;
                            resultText += "(2/" + @char._index + ")";
                            if (s_operators.Contains(temp) && i != 0 && readText[i - 1] != '*' && readText[i - 1] != '/')
                            {
                                tempOperatorNumber++;
                                commentOperatorAllNumber++;
                            }
                            temp = "";
                            indexChars = 1;
                            i--;
                            break;
                        }
                        else if (@char._index == indexChars && readText[i].Equals(Convert.ToChar(@char._char)) && indexWords == 1)
                        {
                            flag = false;
                            temp += readText[i];
                            indexChars = @char._next;
                            break;
                        }
                    }
                }

                if (readText[i].Equals('"'))
                {
                    resultText += readText[i];
                    i++;
                    while (!readText[i].Equals('"'))
                    {
                        resultText += readText[i];
                        i++;
                    }
                }
                if (readText[i].Equals('\''))
                {
                    resultText += readText[i];
                    i++;
                    while (!readText[i].Equals('\''))
                    {
                        resultText += readText[i];
                        i++;
                    }

                    if (i >= readText.Length)
                        break;
                }
                if (readText[i].Equals(Convert.ToChar('\r')) || readText[i].Equals(Convert.ToChar('\t')) || readText[i].Equals(Convert.ToChar('\n')))
                {
                    resultText += readText[i];
                }
                else if (flag)
                {
                    resultText += readText[i];
                }
            }

            operatorsBlockCounter.Add(tempOperatorNumber);
        }



        //--------------------------------------------------------------------------------------

        private static Array ResizeArray(Array arr, int[] newSizes)
        {
            if (newSizes.Length != arr.Rank)
                throw new ArgumentException("arr must have the same number of dimensions " +
                                            "as there are elements in newSizes", "newSizes");

            var temp = Array.CreateInstance(arr.GetType().GetElementType(), newSizes);
            int length = arr.Length <= temp.Length ? arr.Length : temp.Length;
            Array.ConstrainedCopy(arr, 0, temp, 0, length);
            return temp;
        }
        //--------------------------------------------------------------------------------------

        private void AnalysisVariables(string source)
        {
            counterVariables = 1;
            temp_types = source.Split(new Char[] { '\r', '\n', ' ', ',', ';', '(', ')', '[', ']', '{', '}' });
            for (int i = 0; i < temp_types.Length; i++)
            {
                for (int j = 0; j < types.Count; j++)
                {
                    if (types.Contains(temp_types[i]))
                    {
                        i++;
                        while (temp_types[i] == "")
                        { i++; }
                        if (counterVariables > 1)
                            variables = (int[,])ResizeArray(variables, new int[] { counterVariables, 2 });
                        variables[counterVariables - 1, 0] = i;
                        variables[counterVariables - 1, 1] = j;
                        counterVariables++;
                    }
                }
            }
            counterVariables--;

        }
        //--------------------------------------------------------------------------------------

        private void AnalysisLiteral()
        {
            int index1, index2;
            while ((index1 = resultText.IndexOf('"')) >= 0)
            {
                resultText = resultText.Remove(index1, 1);
                index2 = resultText.IndexOf('"');
                if (index1 == -1 || index2 == -1)
                    break;
                CompareUniqueOperands(resultText.Substring(index1, (index2 + 1 - index1)));
                operandsCounter++;
                resultText = resultText.Remove(index1, (index2 + 1 - index1));
                resultText = resultText.Insert(index1, "(4)");

            }
            while ((index1 = resultText.IndexOf("@")) >= 0)
            {
                resultText = resultText.Remove(index1, 1);
            }

            readText = resultText;
            while ((index1 = readText.IndexOf("(")) >= 0)
            {
                index2 = readText.IndexOf(")");
                if (index1 > index2)
                {
                    readText = readText.Remove(index2, 1);
                }
                else
                {
                    readText = readText.Remove(index1, (index2 + 1 - index1));
                    readText = readText.Insert(index1, " ");
                }
            }
            string str = "";
            bool flag;
            temp_literals = readText.Split(new Char[] { '\n', '\r', '\t', ' ' });
            readText = resultText;
            for (int i = 0; i < temp_literals.Length; i++)
            {
                if (temp_literals[i] != "")
                {
                    flag = true;
                    if ((index1 = readText.IndexOf(temp_literals[i])) >= 0)
                    {
                        if (readText[index1 - 1] == ' ' || resultText[index1 - 1] == ')')
                        {
                            for (int j = 0; j < counterVariables; j++)
                            {
                                if (temp_literals[i].Equals(temp_types[variables[j, 0]]))
                                {
                                    if (index1 >= 0)
                                    {
                                        flag = false;
                                        str = temp_types[variables[j, 0]];
                                        CompareUniqueOperands(str);
                                        operandsCounter++;
                                        resultText = resultText.Remove(index1, str.Length);
                                        resultText = resultText.Insert(index1, "(3/" + (variables[j, 1] + 1) + ")");
                                        readText = readText.Remove(index1, str.Length);
                                        readText = readText.Insert(index1, "(3/" + (variables[j, 1] + 1) + ")");
                                    }
                                }
                            }
                            if (flag)
                            {
                                str = temp_literals[i];
                                CompareUniqueOperands(resultText.Substring(index1, str.Length));
                                operandsCounter++;
                                resultText = resultText.Remove(index1, str.Length);
                                resultText = resultText.Insert(index1, "(4)");
                                readText = readText.Remove(index1, str.Length);
                                readText = readText.Insert(index1, "(4)");
                            }
                            if ((index1 = readText.IndexOf(temp_literals[i])) >= 0)
                            {
                                i--;
                            }
                        }
                        else
                        {
                            while ((index1 = readText.IndexOf(temp_literals[i])) >= 0 &&
                                (readText[index1 - 1] != ' ' && resultText[index1 - 1] != ')'))
                            {
                                str = "";
                                readText = readText.Remove(index1, temp_literals[i].Length);
                                for (int j = 0; j < temp_literals[i].Length; j++)
                                {
                                    str += "`";
                                }
                                readText = readText.Insert(index1, str);
                            }
                        }
                        if (index1 >= 0)
                            i--;
                    }
                }
            }
        }

        public IEnumerable<AnalyzerItem> AnalyzeSource(string source)
        {
            commentOperatorAllNumber = 0;
            var goodsource = ParseCode(source);
            bool isprime = false;
            double a_coef = 0f;
            AnalysisComments(goodsource, ref isprime, ref a_coef);
            AnalysisWordsAndChars(goodsource);
            AnalysisVariables(goodsource);
            AnalysisLiteral();
            numberOperators.Sort();
            numberOperands.Sort();
            AnalyzerItem item = new AnalyzerItem
            {
                UniqueOperators = uniqueOperators.Count,
                UniqueOperands = uniqueOperands.Count,
                OperatorsCounter = commentOperatorAllNumber,
                OperandsCounter = operandsCounter,
                TeoryOperators = numberOperators.Count != 0 ? Convert.ToDouble(numberOperators[numberOperators.Count - 1]) : 0,
                TeoryOperands = numberOperators.Count != 0 ? Convert.ToDouble(numberOperands[numberOperands.Count - 1]) : 0,
                CommentCounter = commentCounter / 2,
                CommentBlockCounter = commentBlockCounter,
                OperatorsBlockCounter = operatorsBlockCounter,
                LineNumber = linesNumber,
                A_coef = a_coef,
                Isprime = isprime
            };
            List<AnalyzerItem> IEN = new List<AnalyzerItem>();
            IEN.Add(item);
            return IEN;

        }
    }
}
