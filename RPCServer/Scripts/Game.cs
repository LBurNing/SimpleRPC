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
            RPC.Register<Move>("ReqMove", ReqMove);
            BitConverterHelper.Init();
            RPC.Init();
            gateway = new Gateway();
            gateway.RunServer();

            Task.Run(Update);
            Console.ReadLine();
        }

        private static void ReqMove(Role role, Move move)
        {
            LogHelper.Log($"Recv: Move, x = {move.X}, y = {move.Y}, 同步给所有客户端");
            RPC.Call("OnMove", move);
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
                    LogHelper.Log(ex.ToString());
                }
            }
        }

        public static void Destroy()
        {
        }
    }
}
