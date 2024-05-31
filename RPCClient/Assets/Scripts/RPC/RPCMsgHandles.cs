using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public sealed class RPCMsgDefine
    {
        public const string REQ_MOVE = "OnMove";
    }

    /// <summary>
    /// 消息处理
    /// 必须为private
    /// </summary>
    public sealed class RPCMsgHandles
    {
        private static void RecvAttack(int skillid, Attack attack, ItemList itemList)
        {
            LogHelper.Log($"Recv: skillid = {attack.Id}, targetId = {attack.TargetId}, itemList.Count = {itemList.Items.Count}");
        }

        private static void RecvDelete(int msg)
        {
            LogHelper.Log($"Recv: state = {msg}");
        }

        private static void RecvReflectMove(Move move)
        {
            LogHelper.Log($"move reflect sync: x:{move.X}, y:{move.Y}, speed:{move.Speed}, dir:{move.Dir}");
        }
    }
}