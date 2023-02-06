using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
    internal class CPU
    {
        public byte[] RAM = new byte[4096];
        public byte[] V = new byte[0x10];   // REGISTERS
        public ushort I = 0;                // ADDRESSESS

        public byte DelayTimer = 0;
        public byte SoundTimer = 0;

        public ushort PC = 0;                   // Program Counter
        public byte SP = 0;                     // Stack pointer

        public ushort[] Stack = new ushort[16];

        public ushort Keypad = 0;

        public byte[] Display = new byte[32 * 64];

        public void Run(byte[] program)
        {
            // load program into ram
            RAM = new byte[4096];
            for (int i = 0; i < program.Length; i++)
            {
                RAM[512 + i] = program[i];
            }

            while (true)
            {
                ushort opcode = (ushort)(program[PC] << 8 | program[PC + 1]);

                ushort significant = (ushort)(opcode & 0xF000);
                switch(significant)
                {
                    case 0x0000:
                        byte least = (byte)((opcode & 0x00FF));
                        switch(least)
                        {
                            case 0xE0:
                                Display = new byte[32 * 64];
                                break;
                            case 0xEE:
                                PC = Stack[SP];
                                SP--;
                                break;
                            default:
                                throw new Exception($"Unknown opcode: {opcode.ToString("X4")}");
                        }
                        break;
                    case 0x1000:
                        PC = (ushort)((opcode & 0x0FFF));
                        break;
                    case 0x2000:
                        SP++;
                        Stack[SP] = PC;
                        PC = (ushort)((opcode & 0x0FFF));
                        break;
                    case 0x3000:
                        if (V[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
                            PC += 2;
                        break;
                    case 0x4000:
                        if (V[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF))
                            PC += 2;
                        break;
                    case 0x5000:
                        if (V[(opcode & 0x0F00) >> 8] == (opcode & 0x00F0) >> 4)
                            PC += 2;
                        break;
                    case 0x6000:
                        V[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);
                        break;
                    case 0x7000:
                        V[opcode & 0x0F00 >> 8] = (byte)(V[opcode & 0x0F00 >> 8] + (opcode & 0x00FF));
                        break;
                }

                PC += 2;
            }
        }
    }
}
