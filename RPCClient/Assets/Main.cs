using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

namespace Game
{
    public class Main : MonoBehaviour
    {
        private static Main _instance;
        private Socket _socket;
        public static Socket Socket { get { return _instance._socket; } }

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this);
            GameFrame.Init();
            BitConverterHelper.Init();
            Profiler.BeginSample("Init RPCMoudle");
            RPCMoudle.Init();
            Profiler.EndSample();

            InitManager();
        }

        private void Start()
        {
        }

        private void Update()
        {
            _socket?.Update();
            GameFrame.UpdateMoudle();
        }

        private void InitManager()
        {
            _socket = new Socket();
        }

        public void OnDestroy()
        {
            Socket?.Dispose();
            GameFrame.UnInitMoudle();
        }
    }
}