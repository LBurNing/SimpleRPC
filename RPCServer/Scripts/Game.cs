using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Game
    {
        private static int FrameCount = 60;
        public static Socket Socket { get; set; }


        public static void Init()
        {
            BitConverterHelper.Init();
            RPC.Init();
            Socket = new Socket();
            Socket.RunServer();

            Task.Run(Update);
            Console.ReadLine();
        }

        public static async Task Update()
        {
            while (true)
            {
                try
                {
                    Socket?.Update();
                    await Task.Delay(1000 / FrameCount);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Destroy()
        {
        }
    }
}
