using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game;

namespace UI
{
    public class SocketUI : MonoBehaviour
    {
        public TMP_InputField _ip;
        public TMP_InputField _port;
        public Button _connect;
        public Button _send;
        public Button _send1;
        public TextMeshProUGUI _log;

        void Start()
        {
            _connect.onClick.AddListener(OnConnect);
            _send.onClick.AddListener(OnSend);
            _send1.onClick.AddListener(OnSend1);
        }

        void Update()
        {
            _log.text = LogHelper.LogInfo;
        }

        private void OnConnect()
        {
            Main.Socket.Connect(_ip.text, int.Parse(_port.text));
        }

        private void OnSend()
        {
            Attack attack = new Attack();
            attack.Id = 10;
            attack.TargetId = 1001;
            RPCMoudle.Call("ReqAttack", 125, "À×öªÍòÀ¤", 5.21f, attack, 10.2563);
        }

        private void OnSend1()
        {
            ItemList itemList = new ItemList();
            for (int i = 0; i < 200; i++)
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

            RPCMoudle.Call("ReqDelete", itemList);
        }
    }
}