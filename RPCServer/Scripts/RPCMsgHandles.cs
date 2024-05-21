using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public class RPCMsgHandles
    {
        [Recv]
        public static void ReqAttack(Role role, int skillId, string sklillDesc, float cd, Attack attack, double pubCd)
        {
            Console.WriteLine($"Recv: skillId = {skillId}, sklillDesc = {sklillDesc}");
            ItemList itemList = new ItemList();
            for (int i = 0; i < 100; i++)
            {
                Item item = new Item();
                item.Id = i;
                item.Name = i.ToString();
                MyColor color = new MyColor();
                color.Red = 10;
                color.Green = 20;
                color.Blue = 30;
                item.Color = color;

                ItemBind itemBind = new ItemBind();
                itemBind.Bind = i % 2 == 0 ? 0 : 1;
                itemList.Items.Add(item);
                itemList.ItemBinds.Add(itemBind);
            }

            RPC.Call(role, "RecvAttack", 10086, attack, itemList);
        }

        [Recv]
        public static void ReqDelete(Role role, ItemList itemList)
        {
            Console.WriteLine($"Recv: ItemList.Len = {itemList.Items.Count}");
            RPC.Call(role, "RecvDelete", 0);
        }

        [Recv]
        public static void ReqMove(Role role, int x, int y)
        {
            Console.WriteLine($"Recv: Move, x = {x}, y = {y}, 同步给所有客户端");
            RPC.Call("RecvMove", x, y);
        }
    }
}