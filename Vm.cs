namespace Compiler;

/*Memory layout
 Static variables 0x0 ... 0xFFF
 Local Scope 0xFFF ... 0xFFFF
 Heap 0xFFFF ... 0xFFFFFF
*/
public class Vm
{
    private readonly byte[] _memory = new byte[0xFFFFFF];
    private readonly uint[] _stack = new uint[0xFF];
    private byte _stackPointer;
    private readonly Dictionary<string, (string type, uint offset)> _identifiers;

    public Vm(List<(string, string)> instructions, Dictionary<string, (string, uint)> identifiers)
    {
        _identifiers = identifiers;
        string? error = Run(instructions);
        if (error != null)
        {
            Console.WriteLine(error);
        }
    }

    private string? Run(List<(string, string)> instructions)
    {
        foreach ((string instruction, string operand) in instructions)
        {
            switch (instruction)
            {
                case "PUSH":
                    string? res = Push(uint.Parse(operand));
                    if (res != null) { return res; }
                    break;
                case "ADD":
                case "MULT":
                case "DIV":
                case "SUB":
                case "CMP":
                    res = ExecuteInfixExpressions(instruction);
                    if (res != null) { return res; }
                    break;
                case "NEG":
                    res = Push(~Pop() + 1);
                    if (res != null) { return res;}
                    break;
                case "NOT":
                    res = Push(Pop() ^ 1);
                    if (res != null) { return res; }
                    break;
                case "STA":
                    (string type, uint offset) = _identifiers[operand];
                    uint num = Pop();
                    switch (type)
                    {
                        case "int":
                            _memory[offset] = (byte)(num & 0b11111111);
                            _memory[offset + 1] = (byte)((num & 0b11111111_00000000) >> 8);
                            _memory[offset + 2] = (byte)((num & 0b11111111_00000000_00000000) >> 16);
                            _memory[offset + 3] = (byte)((num & 0b11111111_00000000_00000000_00000000) >> 24);
                            break;
                        case "bool":
                            _memory[offset] = (byte)num;
                            break;
                    }
                    break;
                case "LDA":
                    (type, offset) = _identifiers[operand];
                    num = type switch
                          {
                              "int" => (uint) (_memory[offset] | (_memory[offset + 1] << 8) |
                                               (_memory[offset + 2] << 16) | (_memory[offset + 3] << 24)),
                              "bool" => _memory[offset],
                              _      => 0
                          };

                    res = Push(num);
                    if (res != null) { return res; }
                    break;
            }
        }
        return null;
    }

    private string? Push(uint num)
    {
        if (_stackPointer == _stack.Length)
        {
            return "Stack overflow occured";
        }
        _stack[_stackPointer] = num;
        _stackPointer++;

        return null;
    }
    
    private uint Pop()
    {
        _stackPointer--;
        uint tmp = _stack[_stackPointer];
        _stack[_stackPointer] = 0;
        return tmp;
    }

    private string? ExecuteInfixExpressions(string @operator)
    {
        uint num1 = Pop();
        uint num2 = Pop();
        
        string? res;
        switch (@operator)
        {
            case "CMP":
                res = Push((uint)(num2 == num1 ? 1 : 0));
                break;
            case "ADD":
                res = Push(num2 + num1);
                break;
            case "MULT":
                res = Push(num2 * num1);
                break;
            case "DIV":
                res = Push(num2 / num1);
                break;
            default:
                return $"'{@operator}' isn't implemented";
        }
        return res;
    }

    public uint Top() => _stack[0];
}
