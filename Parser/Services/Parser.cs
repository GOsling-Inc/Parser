using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;
using System.Collections.Immutable;

namespace Parser;

public class Item
{
    public string Index { get; set; }
    public string Name { get; set; }
    public string Count { get; set; }
    public Item(string index, string name, string count)
    {
        Name = name;
        Index = index;
        Count = count;
    }
}

class Parser
{
    string[] allOperators = [";", ".", "+", "-", "*", "/", "%", "**", "==", "!=", ">", "<", ">=", "<=", "&&", "||", "!", "&", "|", "=", "+=", "-=", "*=", "/=", "%=", "**=", "<<=", ">>=", "&=", "^=", "|=", "^", "<<", ">>", "~", ">>>"];
    string[] keyWords = ["case", "def", "do", "extends", "for", "forSome", "if", "import", "lazy", "match", "new", "package", "return", "sealed", "throw", "try", "type", "val", "var", "while", "with", "yield"];
    Dictionary<string, int> operators = new();
    Dictionary<string, int> operands = new();
    public List<List<Item>> ParseScala(string code)
    {
        Console.WriteLine(code);

        code = RemoveStrings(code);
        CountBrackets(code);
        code = RemoveUseless(code);


        CountFunctions(code);
        CountVariables(code);
        CountKeyWords(code);

        CountDefaultOperators(code);



        var op1 = new List<Item>();
        op1.Add(new Item("индекс", "оператор", "количество"));
        var newoperators = from entry in operators orderby entry.Value descending select entry;
        for (int i = 0; i < newoperators.Count(); i++)
        {
            if (newoperators.ElementAt(i).Value > 0) op1.Add(new Item((i+1).ToString(), newoperators.ElementAt(i).Key, newoperators.ElementAt(i).Value.ToString()));
        }

        var op2 = new List<Item>();
        op2.Add(new Item("индекс", "операнд", "количество"));
        var newoperands = from entry in operands orderby entry.Value descending select entry;
        for (int i = 0; i < newoperands.Count(); i++)
        {
            if (newoperands.ElementAt(i).Value > 0) op2.Add(new Item((i+1).ToString(), newoperands.ElementAt(i).Key, newoperands.ElementAt(i).Value.ToString()));
        }
        return new List<List<Item>> { op1, op2 };
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
            if (bebra == "" || allOperators.Contains(bebra) || onlyBrackets.IsMatch(bebra))
            {
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
