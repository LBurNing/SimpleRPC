using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

namespace Game
{
    public class Main : MonoBehaviour
    {
        private static Main _instance;
        private Tcp _tcp;
        private Udp _udp;
        public static Main Instance {  get { return _instance; } }
        public static Tcp Tcp { get { return _instance._tcp; } }
        public static Udp Udp { get { return _instance._udp; } }

        private void Awake()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.unityLogger.logEnabled = true;
#else
            UnityEngine.Debug.unityLogger.logEnabled = false;
#endif
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
            _tcp?.Update();
            _udp?.Update();
            GameFrame.UpdateMoudle();
        }

        private void InitManager()
        {
            _tcp = new Tcp();
            _udp = new Udp();
        }

        public void Send(BuffMessage message, ProtocolType type = ProtocolType.Tcp)
        {
            if (type == ProtocolType.Tcp)
            {
                _tcp.Send(message);
            }
            else if (type == ProtocolType.Udp)
            {
                _udp.Send(message);
            }
        }

        public void OnDestroy()
        {
            _tcp?.Dispose();
            _udp?.Dispose();
            GameFrame.UnInitMoudle();
        }
    }
}