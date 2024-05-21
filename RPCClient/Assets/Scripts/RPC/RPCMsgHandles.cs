using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class RPCMsgHandles
    {
        #region 模拟客户端接收协议
        [Recv]
        private static void RecvAttack(int skillid, Attack attack, ItemList itemList)
        {
            LogHelper.Log($"Recv: skillid = {attack.Id}, targetId = {attack.TargetId}, itemList.Count = {itemList.Items.Count}");
        }

        [Recv]
        private static void RecvDelete(int msg)
        {
            LogHelper.Log($"Recv: state = {msg}");
        }

        [Recv]
        private static void RecvMove(int x, int y)
        {
            LogHelper.Log($"RecvMove: x = {x}, y = {y}");
        }
        #endregion
    }
}