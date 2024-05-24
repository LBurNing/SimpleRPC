using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class RPCMsgHandles
    {
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
    }
}