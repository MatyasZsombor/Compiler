namespace Compiler;

/*Memory layout
    Stack 0x00 .. 0xFF
    Global Variables 0x100 .. 0xFFF 
    Heap 0x1000 .. 0xFFFFFF
*/
public class Vm
{
    private readonly int[] _memory = new int[0xFFFFFF];
    private readonly Dictionary<string, int> _functions;

    private readonly Dictionary<string, int> _registers = new()
    {
        {"eax", 0},
        {"ecx", 0},
        {"ebp", 0xff},
        {"eip", 0},
        {"esp", 0xff},
        {"status", 0}
    };

    public Vm(List<(string, string)> instructions, Dictionary<string, int> functions, int start)
    {
        _registers["eip"] = start;
        _functions = functions;
        Run(instructions);
        Console.WriteLine(_registers["eax"]);
    }

    private void Run(List<(string, string)> instructions)
    {
        while(_registers["eip"] < instructions.Count)
        {
            (string instruction, string operand) = instructions[_registers["eip"]];
            switch (instruction)
            {
                case "push":
                    string? res = Push(_registers[operand]);
                    if (res != null)
                    {
                        Console.WriteLine(res);
                    }
                    break;
                case "pop":
                    if (operand != "")
                    {
                        _registers[operand] = Pop();
                        break;
                    }
                    Pop();
                    break;
                case "call":
                    Push(_registers["eip"]);
                    _registers["eip"] = _functions[operand] - 1;
                    Push(_registers["ebp"]);
                    _registers["ebp"] = _registers["esp"];
                    break;
                case "mov":
                    string[] operands = operand.Split(",");
                    _registers[operands[1]] = int.Parse(operands[0]);
                    break;
                case "sta":
                    _memory[int.Parse(operand)] = _registers["eax"];
                    break;
                case "neg":
                    _registers["eax"] = ~_registers["eax"] + 1;
                    break;
                case "ret":
                    _registers["esp"] = _registers["ebp"];
                    _registers["ebp"] = Pop();
                    _registers["eip"] = Pop();
                    break;
                case "jne":
                    _registers["eip"] = _registers["status"] == 1 ? _registers["eip"] : int.Parse(operand);
                    break;
                case "not":
                    _registers["eax"] = _registers["eax"] == 0 ? 1 : 0; 
                    break;
                case "lda":
                    _registers["eax"] = _memory[GetAddress(operand)];
                    break;
                case "add":
                case "sub":
                case "mul":
                case "div":
                case "cmp":
                case  "lcmp":
                case  "gcmp":
                    operands = operand.Split(",");
                    res = ExecuteInfixExpressions(instruction, operands[0], operands[1]);
                    if (res != null)
                    {
                        Console.WriteLine(res);
                    }
                    break;
                default:
                    Console.WriteLine("Unimplemented instruction");
                    break;
            }

            _registers["eip"]++;
        }
    }

    private string? Push(int num)
    {
        if (_registers["esp"] == 0)
        {
            return "Stack overflow occured";
        }
        _memory[_registers["esp"]] = num;
        _registers["esp"]--;

        return null;
    }
    
    private int Pop()
    {
        _registers["esp"]++;
        int tmp = _memory[_registers["esp"]];
        _memory[_registers["esp"]] = 0;
        return tmp;
    }

    private int GetAddress(string address)
    {
        string[] tmp;
        if (address.Contains('-'))
        {
            tmp = address.Split('-');
            return _registers[tmp[0]] - int.Parse(tmp[1]);
        } 
        tmp = address.Split('+');
        return _registers[tmp[0]] + int.Parse(tmp[1]); 
    }

    private string? ExecuteInfixExpressions(string @operator, string source, string destination)
    {
        int left = _registers[source];
        int right = _registers[destination];

        int res;
        switch (@operator)
        {
            case "cmp":
                res = left == right ? 1 : 0;
                _registers["status"] = res;
                break;
            case "lcmp":
                res = left < right ? 1 : 0;
                _registers["status"] = res;
                break;
            case "gcmp":
                res = left > right ? 1 : 0;
                _registers["status"] = res;
                break;
            case "add":
                res = left + right;
                break;
            case "sub":
                res = left - right;
                break;
            case "mul":
                res = left * right;
                break;
            case "div":
                res = left / right;
                break;
            default:
                return $"'{@operator}' isn't implemented";
        }

        _registers[destination] = res;
        return null;
    }

    public int Top() => _memory[0xff];
}
