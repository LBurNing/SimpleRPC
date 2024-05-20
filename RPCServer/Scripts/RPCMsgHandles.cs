using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public class RPCMsgHandles
    {
        [Recv]
        public static void ReqAttack(int skillId, string sklillDesc, float cd, Attack attack, double pubCd)
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

            RPC.Call("RecvAttack", 10086, attack, itemList);
        }

        [Recv]
        public static void ReqDelete(ItemList itemList)
        {
            Console.WriteLine($"Recv: ItemList.Len = {itemList.Items.Count}");
            RPC.Call("RecvDelete", 0);
        }
    }
}