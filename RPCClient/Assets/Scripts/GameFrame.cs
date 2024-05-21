using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameFrame
    {
        public static MyRole myRole;
        public static Message message;
        private static List<IMoudle> _moudles;

        public static void Init()
        {
            _moudles = new List<IMoudle>();
            myRole = AddMoudle<MyRole>();
            message = AddMoudle<Message>();

            InitMoudle();
        }

        private static void InitMoudle()
        {
            foreach (var item in _moudles)
            {
                item.Init();
            }
        }

        private static T AddMoudle<T>() where T : IMoudle, new()
        {
            IMoudle moudle = new T();
            _moudles.Add(moudle);
            return (T)moudle;
        }

        public static void UpdateMoudle()
        {
            foreach (var item in _moudles)
            {
                item.Update();
            }
        }

        public static void UnInitMoudle()
        {
            foreach (var item in _moudles)
            {
                item.UnInit();
            }
        }
    }
}