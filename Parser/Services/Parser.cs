using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;

namespace Parser;

public class ParserEntry
{
    public static void Parse()
    {
        string path = @"D:\LabsC#\parser\Parser\Services\test.txt";
        Debug.WriteLine(path);
        string text = "";
        using (FileStream fstream = File.OpenRead(path))
        {
            byte[] buffer = new byte[fstream.Length];
            fstream.Read(buffer, 0, buffer.Length);
            text = Encoding.Default.GetString(buffer);
        }
        Parser p = new();
        p.ParseScala(text);
    }


}

class Parser
{
    string[] allOperators = [";", ".", "+", "-", "*", "/", "%", "**", "==", "!=", ">", "<", ">=", "<=", "&&", "||", "!", "&", "|", "=", "+=", "-=", "*=", "/=", "%=", "**=", "<<=", ">>=", "&=", "^=", "|=", "^", "<<", ">>", "~", ">>>"];
    string[] keyWords = ["case", "def", "do", "extends", "for", "forSome", "if", "import", "lazy", "match", "new", "package", "return", "sealed", "throw", "try", "type", "val", "var", "while", "with", "yield"];
    Dictionary<string, int> operators = new();
    Dictionary<string, int> operands = new();
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



        Debug.WriteLine("ALL OPERATORS");
        foreach (var item in operators)
        {
            Debug.WriteLine($"{item.Key} : {item.Value}");
        }
        Debug.WriteLine("\nALL OPERANDS");
        foreach (var item in operands)
        {
            Debug.WriteLine($"{item.Key} : {item.Value}");
        }
    }

    private string RemoveStrings(string code)
    {
        var newStr = "";
        var index = 0;
        Regex reg = new("[\"\'`].*[\"\'`]");
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
        Regex reg = new("else if");
        return reg.Replace(code, "");
    }

    private List<string> GetVariables(string code)
    {
        Regex reg = new("(var|val)( +)(.+)( *):");
        var matches = reg.Matches(code);
        return (from elem in matches select elem.Groups[3].ToString()).ToList();
    }

    private List<string> GetFunctions(string code)
    {
        Regex reg = new(@"(def)( +)(.+)( *)\(");
        var matches = reg.Matches(code);
        return (from elem in matches select elem.Groups[3].ToString()).ToList();
    }

    private void CountFunctions(string code)
    {
        var funcs = GetFunctions(code);
        foreach (var item in funcs)
        {
            Regex reg = new($@"\W*({item})\W*");
            var matches = reg.Matches(code);
            operators[item] = 0;
            operands[item] = 0;
            foreach(var match in matches)
            {
                if (allOperators.Any(op => match.ToString().Contains(op)))
                {
                   ++operands[item];
                }
                ++operators[item];
            }
            if (operands[item] == 0) operands.Remove(item);
            /*if (count > 0)
            {
                operators[item] = 1;
                if (count > 1)
                {
                    operands[item] = count - 1;
                }
            }*/
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
        Regex reg = new(@"(\S*)( *)(\(+)");
        Regex onlyBrackets = new(@"^\(+$");
        foreach (Match item in reg.Matches(code))
        {
            var bebra = item.Groups[1].Value;
            Debug.WriteLine($"----------- {bebra}");
            if (bebra == "" || allOperators.Contains(bebra) || onlyBrackets.IsMatch(bebra))
            {
                Debug.WriteLine($"=========== {item.Value}");
                operators["()"] += item.Groups[3].Length;
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
