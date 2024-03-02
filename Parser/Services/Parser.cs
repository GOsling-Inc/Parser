using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;
using System.Collections.Immutable;

namespace Parser;

public class Item(string index, string name, string count)
{
    public string Index { get; set; } = index;
    public string Name { get; set; } = name;
    public string Count { get; set; } = count;
}

class Parser
{
    readonly string[] allOperators = [";", "+", "-", "*", "/", "%", "**", "==", "!=", ">", "<", ">=", "<=", "&&", "||", "!", "&", "|", "=", "+=", "-=", "*=", "/=", "%=", "**=", "<<=", ">>=", "&=", "^=", "|=", "^", "<<", ">>", "~", ">>>"];
    readonly string[] keyWords = ["print", "println", "case", "def", "do", "extends", "for", "forSome", "if", "import", "lazy", "match", "new", "package", "return", "sealed", "throw", "try", "type", "val", "var", "while", "with", "yield"];
    readonly Dictionary<string, int> operators = [];
    readonly Dictionary<string, int> operands = [];
    public List<List<Item>> ParseScala(string code)
    {
        Console.WriteLine(code);

        code = RemoveStrings(code);
        CountBrackets(code);
        code = RemoveUseless(code);

        CountNumbers(code);
        CountFunctions(code);
        CountVariables(code);
        CountKeyWords(code);

        CountDefaultOperators(code);



        var op1 = new List<Item>
        {
            new("индекс", "оператор", "количество")
        };
        var newoperators = from entry in operators orderby entry.Value descending select entry;
        int counter = 0, prog_dict = 0, prog_len = 0;
        for (int i = 0; i < newoperators.Count(); i++)
            if (newoperators.ElementAt(i).Value > 0)
            {
                op1.Add(new Item((i + 1).ToString(), newoperators.ElementAt(i).Key, newoperators.ElementAt(i).Value.ToString()));
                counter += newoperators.ElementAt(i).Value;
            }
        prog_dict += op1.Count();
        prog_len += counter;
        var statOperators = new Item($"n1 = {op1.Count() - 1}", "", $"N1 = {counter}");
        counter = 0;
        var op2 = new List<Item>
        {
            new("индекс", "операнд", "количество")
        };
        var newoperands = from entry in operands orderby entry.Value descending select entry;
        for (int i = 0; i < newoperands.Count(); i++)
            if (newoperands.ElementAt(i).Value > 0)
            {
                op2.Add(new Item((i + 1).ToString(), newoperands.ElementAt(i).Key, newoperands.ElementAt(i).Value.ToString()));
                counter += newoperands.ElementAt(i).Value;
            }
        prog_dict += op2.Count();
        prog_len += counter;
        var statOperands = new Item($"n2 = {op2.Count() - 1}", "", $"N2 = {counter}");
        var lists = SameLen(op1, op2);
        op1 = lists[0];
        op1.Add(statOperators);
        op2 = lists[1];
        op2.Add(statOperands);
        return [op1, op2, [new Item(prog_dict.ToString(), prog_len.ToString(), (((int)(prog_len*Math.Log2(prog_dict)))).ToString())]];
    }

    private static List<List<Item>> SameLen(List<Item> op1, List<Item> op2)
    {
        if(op1.Count < op2.Count)
        {
            var items = SameLen(op2, op1);
            op2 = items[0];
            op1 = items[1];
        }
        while (op2.Count != op1.Count)
            op2.Add(new Item("", "", ""));
        return [op1, op2];
    }

    private static string RemoveStrings(string code)
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

    private static string RemoveUseless(string code)
    {
        Regex reg = new("else if");
        code = reg.Replace(code, "");
        Regex reg2 = new(@"//.+");
        return reg2.Replace(code, "");
    }

    private static List<string> GetVariables(string code)
    {
        Regex reg = new("(var|val)( +)([a-zA-Z_0-9]+)([ :])");
        var matches = reg.Matches(code);
        return (from elem in matches select elem.Groups[3].ToString()).ToList();
    }

    private static List<string> GetFunctions(string code)
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
            Regex reg = new($@"\W*({item})\W");
            var matches = reg.Matches(code);
            operators[item] = 0;
            operands[item] = 0;
            foreach (var match in matches)
            {
                if (allOperators.Any(op => match.ToString().Contains(op)))
                {
                    ++operands[item];
                }
                ++operators[item];

            }
            if (operands[item] == 0) operands.Remove(item);
        }
    }

    private void CountVariables(string code)
    {
        var vars = Parser.GetVariables(code);
        foreach (var item in vars)
        { 
            Regex reg = new Regex($@"\W({item})\W");
            operands[item] = reg.Matches(code).Count();
        }
    }

    private void CountNumbers(string code)
    {
        Regex reg = new Regex($@"[0-9]+[.e-]*[0-9]*");
        foreach (Match match in reg.Matches(code))
        {
            if (!operands.ContainsKey(match.Value)) operands[match.Value] = 0;
            operands[match.Value]++;
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
        Regex point = new Regex(@"[a-zA-Z]+[a-zA-Z0-9_]*(\.)[a-zA-Z]+[a-zA-Z0-9_]*");
        operators["."] = point.Matches(code).Count();
    }

    private void CountKeyWords(string code)
    {
        foreach (var item in keyWords)
        {
            Regex operatorsReg = new Regex($@"\W{item}\W");
            operators[item] = operatorsReg.Matches(code).Count();
        }
    }

}
