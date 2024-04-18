namespace Compiler;

/*Memory layout
 Static variables 0x0 ... 0xFFF
 Local Scope 0xFFF ... 0xFFFF
 Heap 0xFFFF ... 0xFFFFFF
*/
public class Vm
{
    private readonly byte[] _memory = new byte[0xFFFFFF];
    private readonly uint[] _stack = new uint[0xFFFF];
    private byte _stackPointer;
    private int _programCounter;

    public Vm(List<(string, string)> instructions, int programCounter)
    {
        _programCounter = programCounter;
        string error = Run(instructions);
        if (error != "")
        {
            Console.WriteLine(error);
        }
    }

    private string Run(List<(string, string)> instructions)
    {
        while(true)
        {
            (string instruction, string operand) = instructions[_programCounter];
            switch (instruction)
            {
                case "BRK":
                    return "";
                case "PUSH":
                    string? res = Push(uint.Parse(operand));
                    if (res != null) { return res; }
                    break;
                case "ADD":
                case "MULT":
                case "DIV":
                case "SUB":
                case "CMP":
                case "LCMP": 
                case "GCMP":
                    res = ExecuteInfixExpressions(instruction);
                    if (res != null) { return res; }
                    break;
                case "RTSV":
                    uint tmp = Pop();
                    uint jmp = Pop();
                    res = Push(tmp);
                    _programCounter = (int)jmp;
                    break;
                case "BRZ":
                    _programCounter = Pop() == 0 ? int.Parse(operand) : _programCounter;
                    break;
                case "BRNZ":
                    _programCounter = Pop() != 0 ? int.Parse(operand) : _programCounter;
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
                    string[] operands = operand.Split(",");
                    int offset = int.Parse(operands[0]);
                    uint num = Pop();
                    switch (operands[1])
                    {
                        case "4":
                            _memory[offset] = (byte)(num & 0b11111111);
                            _memory[offset + 1] = (byte)((num & 0b11111111_00000000) >> 8);
                            _memory[offset + 2] = (byte)((num & 0b11111111_00000000_00000000) >> 16);
                            _memory[offset + 3] = (byte)((num & 0b11111111_00000000_00000000_00000000) >> 24);
                            break;
                        case "1":
                            _memory[offset] = (byte)num;
                            break;
                    }
                    break;
                case "JMP":
                    _programCounter = int.Parse(operand);
                    continue;
                case "LDA":
                    operands = operand.Split(","); 
                    offset = int.Parse(operands[0]);
                    num = operands[1] switch
                          {
                              "4" => (uint) (_memory[offset] | (_memory[offset + 1] << 8) |
                                               (_memory[offset + 2] << 16) | (_memory[offset + 3] << 24)),
                              "1" => _memory[offset],
                              _      => 0
                          };

                    res = Push(num);
                    if (res != null) { return res; }
                    break;
            }

            _programCounter++;
        }
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
            case "LCMP":
                res = Push((uint) (num2 < num1 ? 1 : 0));
                break;
            case "GCMP":
                res = Push((uint) (num2 > num1 ? 1 : 0));
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
