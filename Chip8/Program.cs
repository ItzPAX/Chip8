using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CPU cpu = new CPU();
            byte[] program = File.ReadAllBytes("chipquarium.ch8");
            try
            {
                cpu.Run(program);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadKey();
        }
    }
}