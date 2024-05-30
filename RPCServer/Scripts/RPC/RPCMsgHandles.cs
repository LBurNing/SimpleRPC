using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public class RPCMsgHandles
    {
        public static void ReqUdp(Role role, int index)
        {
            LogHelper.Log($"udp test: index = {index}");
            RPCMouble.TcpCall(role, "RecvUdp", index);
        }

        public static void ReqAttack(Role role, int skillId, string sklillDesc, float cd, Attack attack, double pubCd)
        {
            LogHelper.Log($"Recv: skillId = {skillId}, sklillDesc = {sklillDesc}");
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

            RPCMouble.UdpCall(role, "RecvAttack", 10086, attack, itemList);
        }

        public static void ReqDelete(Role role, ItemList itemList)
        {
            LogHelper.Log($"Recv: ItemList.Len = {itemList.Items.Count}");
            RPCMouble.TcpCall(role, "RecvDelete", 0);
        }

        private static void ReqReflectMove(Role role, Move move)
        {
            LogHelper.Log($"Recv Reflect: Move, x = {move.X}, y = {move.Y}, 同步给所有客户端");
            RPCMouble.TcpCall("RecvReflectMove", move);
        }
    }
}