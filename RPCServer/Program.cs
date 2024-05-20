using System;

class Program
{
    static void Main(string[] args)
    {
        Game.Game.Init();
        Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPressHandler);

        while (true)
        {
        }
    }

    // 窗口关闭事件的处理程序
    static void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
    {
    }
}