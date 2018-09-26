using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSProxy
{
    class ConsoleWriter
    {


        public static void ConsoleWriteQuery(string message)
        {
            ConsoleWrite(message, 0, 0);
        }
        public static void ConsoleWriteProcessed(string message)
        {
            ConsoleWrite(message, 20, 0);
        }

        public static void ConsoleWriteOnlineRules(string message)
        {
            ConsoleWrite(message, 50, 0);
        }

        public static void ConsoleWriteDateTime()
        {
            ConsoleWrite(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), 55, 2);
        }


        public static void ConsoleWriteIsWork(string message)
        {
            int i = 0;
            foreach(string str in message.Split(';').ToList())
            {
                i++;
                ConsoleWrite(str, 55, 4 + i);
            }
           
        }

        public static void ConsoleWriteMonitoring(string message)
        {
            //ConsoleWrite(message, Console.WindowWidth - 40, 3);
        }



        public static void ConsoleWrite(string message, int pos_left, int pos_top)
        {
            
            int cLeft = Console.CursorLeft;
            int cTop = 0;
            if (Console.CursorTop > 20 && pos_left == 0 && pos_top == 0)
            {
                cTop = 1;
                Console.Clear();
            }
            else
            {
                cTop = Console.CursorTop;
            }
            Console.SetCursorPosition(pos_left, pos_top);
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);

            Console.SetCursorPosition(cLeft, cTop);
            Console.ResetColor();
            // Console.BackgroundColor = ConsoleColor.Black;
        }

    }
}
