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
        public static Gateway gateway { get; set; }


        public static void Init()
        {
            BitConverterHelper.Init();
            RPC.Init();
            gateway = new Gateway();
            gateway.RunServer();

            Task.Run(Update);
            Console.ReadLine();
        }

        public static async Task Update()
        {
            while (true)
            {
                try
                {
                    gateway?.Update();
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
