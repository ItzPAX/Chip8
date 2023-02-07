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
        public byte[] V = new byte[0x10];    // REGISTERS
        public ushort I = 0;                 // ADDRESSESS

        public byte DelayTimer = 0;
        public byte SoundTimer = 0;

        public ushort PC = 512;                   // Program Counter
        public byte SP = 0;                       // Stack pointer

        public ushort[] Stack = new ushort[16];

        public ushort Keypad = 0;

        public byte[] Display = new byte[32 * 64];

        Random random = new Random();

        public void ShowDisplay()
        {
            Console.Clear();
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (Display[x + y * 64] == 0) Console.Write("");
                    else Console.Write("*");
                }
                Console.WriteLine();
            }
        }

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
                ushort opcode = (ushort)(RAM[PC] << 8 | RAM[PC + 1]);

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
                    case 0x8000:
                        byte lower = (byte)(opcode & 0x000F);
                        byte vx = (byte)((opcode & 0x0F00) >> 8);
                        byte vy = (byte)((opcode & 0x00F0) >> 4);
                        switch (lower)
                        {
                            case 0:
                                V[vx] = V[vy];
                                break;
                            case 1:
                                V[vx] = (byte)(V[vx] | V[vy]);
                                break;
                            case 2:
                                V[vx] = (byte)(V[vx] & V[vy]);
                                break;
                            case 3:
                                V[vx] = (byte)(V[vx] ^ V[vy]);
                                break;
                            case 4:
                                V[15] = (byte)(V[vx] + V[vy] > 255 ? 1 : 0);
                                V[vx] = (byte)((V[vx] + V[vy]) & 0x00FF);
                                break;
                            case 5:
                                V[15] = (byte)(V[vx] > V[vy] ? 1 : 0);
                                V[vx] = (byte)(V[vx] - V[vy]);
                                break;
                            case 6:
                                V[15] = (byte)(V[vx] & 0x0001);
                                V[vx] = (byte)(V[vx] >> 1);
                                break;
                            case 7:
                                V[15] = (byte)(V[vy] > V[vx] ? 1 : 0);
                                V[vx] = (byte)(V[vy] - V[vx]);
                                break;
                            case 0xE:
                                V[15] = (byte)((V[vx] & 0x80) == 0x80 ? 1 : 0);
                                V[vx] = (byte)(V[vx] << 1);
                                break;
                            default:
                                throw new Exception($"Unknown opcode: {opcode.ToString("X4")}");
                        }
                        break;
                    case 0x9000:
                        if (V[(byte)((opcode & 0x0F00) >> 8)] != V[(byte)((opcode & 0x00F0) >> 4)])
                            PC += 2;
                        break;
                    case 0xA000:
                        I = (ushort)(opcode & 0x0FFF);
                        break;
                    case 0xB000:
                        PC = (ushort)((opcode & 0x0FFF) + V[0]);
                        break;
                    case 0xC000:
                        V[(byte)((opcode & 0x0F00) >> 8)] = (byte)((byte)(random.Next()) & (byte)(opcode & 0x00FF));
                        break;
                    case 0xD000:
                        int x = V[(opcode & 0x0F00) >> 8];
                        int y = V[(opcode & 0x00F0) >> 4];
                        int n = opcode & 0x000F;

                        V[15] = 0;

                        for (int i = 0; i < n; i++)
                        {
                            byte mem = RAM[I + i];
                            for (int j = 0; j < 8; j++)
                            {
                                byte pixel = (byte)((mem >> (7 - j)) & 0x01);
                                int index = x + j + (y + i) * 64;

                                if (pixel == 1 && Display[index] != 0) V[15] = 1;

                                Display[index] = (byte)(Display[index] ^ pixel);
                            }
                        }
                        break;
                    case 0xE000:
                        if ((opcode & 0x00FF) == 0x009E)
                        {
                            if (((Keypad >> V[(opcode & 0x0F00) >> 8]) & 0x01) == 0x01) PC += 2;
                            break;
                        }
                        else if ((opcode & 0x00FF) == 0x00A1)
                        {
                            if (((Keypad >> V[(opcode & 0x0F00) >> 8]) & 0x01) != 0x01) PC += 2;
                            break;
                        }
                        else
                            throw new Exception($"Unknown opcode: {opcode.ToString("X4")}");
                    case 0xF000:
                        int vx1 = (opcode & 0x0F00) >> 8;
                        switch (opcode & 0x00FF)
                        {
                            case 0x07:
                                V[vx1] = DelayTimer;
                                break;
                            case 0x0A:
                                /* TODO */
                                break;
                            case 0x15:
                                DelayTimer = V[vx1];
                                break;
                            case 0x18:
                                SoundTimer = V[vx1];
                                break;
                            case 0x1E:
                                I = (ushort)(I + V[vx1]);
                                break;
                            case 0x29:
                                I = (ushort)(V[vx1] * 5);
                                break;
                            case 0x33:
                                RAM[I] = (byte)(V[vx1] / 100);
                                RAM[I + 1] = (byte)((V[vx1] % 100) / 10);
                                RAM[I + 2] = (byte)(V[vx1] % 10);
                                break;
                            case 0x55:
                                for (int i = 0; i <= vx1; i++)
                                {
                                    RAM[I + i] = V[i];
                                }
                                break;
                            case 0x65:
                                for (int i = 0; i <= vx1; i++)
                                {
                                    V[i] = RAM[I + i];
                                }
                                break;
                            default:
                                throw new Exception($"Unknown opcode: {opcode.ToString("X4")}");
                        }
                        break;
                    default:
                        throw new Exception($"Unknown opcode: {opcode.ToString("X4")}");
                }
                PC += 2;
            }
        }
    }
}
