using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game;
using Google.Protobuf;

namespace UI
{
    public class SocketUI : MonoBehaviour
    {
        public TMP_InputField _ip;
        public TMP_InputField _port;
        public TMP_InputField _user;
        public Button _connect;
        public Button _reqAttack;
        public Button _reqItem;
        public Button _reqMove;
        public TextMeshProUGUI _log;

        void Start()
        {
            Debug.unityLogger.logEnabled = false;
            _connect.onClick.AddListener(OnConnect);
            _reqAttack.onClick.AddListener(OnReqAttack);
            _reqItem.onClick.AddListener(OnReqItem);
            _reqMove.onClick.AddListener(OnReqMove);
            RPCMoudle.Register<Move>("OnMove", OnMove);
        }

        void Update()
        {
            _log.text = LogHelper.LogInfo;
        }

        private void OnConnect()
        {
            Main.Socket.Connect(_ip.text, int.Parse(_port.text));
            GameFrame.myRole.Create(_user.text);
        }

        private void OnMove(Move move)
        {
            LogHelper.Log($"move sync: x:{move.X}, y:{move.Y}, speed:{move.Speed}, dir:{move.Dir}");
        }

        private void OnReqAttack()
        {
            Attack attack = new Attack();
            attack.Id = 10;
            attack.TargetId = 1001;
            RPCMoudle.Call("ReqAttack", 125, "À×öªÍòÀ¤", 5.21f, attack, 10.2563);
        }

        private void OnReqItem()
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

        private void OnReqMove()
        {
            Move move = new Move();
            move.X = 10;
            move.Y = 20;
            move.Speed = 100;
            move.Dir = 20;
            RPCMoudle.Call("ReqMove", move);
        }

        private void OnDestroy()
        {
            RPCMoudle.Unregister("OnMove");
        }
    }
}