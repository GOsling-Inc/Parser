using System.Text.RegularExpressions;
using System;
using System.Text;
using System.Linq;

namespace Program;

class Program
{
    static void Main(string[] args)
    {
        string path = @"D:\исп\parser\parser\parser\test.txt";
        string text = "";
        using (FileStream fstream = File.OpenRead(path))
        {
            byte[] buffer = new byte[fstream.Length];
            fstream.Read(buffer, 0, buffer.Length);
            text = Encoding.Default.GetString(buffer);
        }
        Parser p = new Parser();
        p.ParseScala(text);
    }


}

class Parser
{
    string[] allOperators = { ";", ".", "+", "-", "*", "/", "%", "**", "==", "!=", ">", "<", ">=", "<=", "&&", "||", "!", "&", "|", "=", "+=", "-=", "*=", "/=", "%=", "**=", "<<=", ">>=", "&=", "^=", "|=", "^", "<<", ">>", "~", ">>>" };
    string[] keyWords = { "case", "def", "do", "extends", "for", "forSome", "if", "import", "lazy", "match", "new", "package", "return", "sealed", "throw", "try", "type", "val", "var", "while", "with", "yield" };
    Dictionary<string, int> operators = new Dictionary<string, int>();
    Dictionary<string, int> operands = new Dictionary<string, int>();
    public void ParseScala(string code)
    {
        Console.WriteLine(code);

        code = RemoveStrings(code);
        CountBrackets(code);
        code = RemoveUseless(code);


        CountFunctions(code);
        CountVariables(code);
        CountKeyWords(code);

        CountDefaultOperators(code);
        
        

        Console.WriteLine("ALL OPERATORS");
        foreach (var item in operators)
        {
            Console.WriteLine($"{item.Key} : {item.Value}");
        }
        Console.WriteLine("\nALL OPERANDS");
        foreach (var item in operands)
        {
            Console.WriteLine($"{item.Key} : {item.Value}");
        }
    }

    private string RemoveStrings(string code)
    {
        var newStr = "";
        var index = 0;
        Regex reg = new Regex("[\"\'`].*[\"\'`]");
        var matches = reg.Matches(code);
        if (matches.Count == 0) return code;
        foreach (Match match in matches)
        {
            newStr += code.Substring(index, match.Index - index);
            index = match.Index + match.Length;
        }
        newStr += code.Substring(matches[matches.Count - 1].Index + matches[matches.Count - 1].Length);

        return newStr;
    }

    private string RemoveUseless(string code)
    {
        Regex reg = new Regex("else if");
        return reg.Replace(code, "");
    }

    private List<string> GetVariables(string code)
    {
        Regex reg = new Regex("(var|val)( +)(.+)( *):");
        var matches = reg.Matches(code);
        return (from elem in matches select elem.Groups[3].ToString()).ToList();
    }

    private List<string> GetFunctions(string code)
    {
        Regex reg = new Regex(@"(def)( +)(.+)( *)\(");
        var matches = reg.Matches(code);
        return (from elem in matches select elem.Groups[3].ToString()).ToList();
    }

    private void CountFunctions(string code)
    {
        var funcs = GetFunctions(code);
        foreach (var item in funcs)
        {
            Regex reg = new Regex($@"\W({item})\W");
            var count = reg.Matches(code).Count();
            if (count > 0)
            {
                operators[item] = 1;
                if (count > 1)
                {
                    operands[item] = count - 1;
                }
            }
        }
    }

    private void CountVariables(string code)
    {
        var vars = GetVariables(code);
        foreach (var item in vars)
        { 
            Regex reg = new Regex($@"\W({item})\W");
            operands[item] = reg.Matches(code).Count();
        }
    }

    private void CountBrackets(string code)
    {
        operators["()"] = 0;
        Regex reg = new Regex(@"(\S*)( *)(\()");
        Regex onlyBrackets = new Regex(@"[ \W]?\(+");
        foreach (Match item in reg.Matches(code))
        {
            var bebra = item.Groups[1].Value;
            if (bebra == "" || allOperators.Contains(bebra) || onlyBrackets.IsMatch(bebra))
            {
                Console.WriteLine($"=========== {item.Value}");
                Console.WriteLine($"----------- {bebra}");
                operators["()"]++;
            }

        }

    }

    private void CountDefaultOperators(string code)
    {
        foreach (var item in allOperators)
        {
            var newItem = "";
            foreach (var i in item)
            {
                newItem += $"\\{i}";
            }
            Regex operatorsReg = new Regex($"[0-9a-zA-Z\r\n ]({newItem})[0-9a-zA-Z\r\n ]");
            operators[item] = operatorsReg.Matches(code).Count();
        }
    }

    private void CountKeyWords(string code)
    {
        var pattern = "";
        foreach (var item in allOperators)
        {
            foreach (var i in item)
            {
                pattern += $"\\{i}";
            }
            pattern += "|";
        }
        foreach (var item in keyWords)
        {
            Regex operatorsReg = new Regex($"[{pattern}| ]({item})[{pattern}| ]");
            operators[item] = operatorsReg.Matches(code).Count();
        }
    }

}
