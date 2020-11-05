﻿using Common;
using Common.Resolvers;
using EuNet.Core;
using EuNet.Unity;
using System;
using System.Threading.Tasks;

public class GameClient : Singleton<GameClient>
{
    private NetClientP2pBehaviour _client;

    // Rpcs
    public LoginRpc LoginRpc { get; private set; }
    public ShopRpc ShopRpc { get; private set; }

    public NetClientP2p Client => _client.ClientP2p;

    protected override void Awake()
    {
        base.Awake();

        _client = GetComponent<NetClientP2pBehaviour>();

        Client.OnConnected = OnConnected;
        Client.OnClosed = OnClosed;
        Client.OnReceived = OnReceive;
        Client.OnP2pGroupLeaved = OnP2pGroupLeave;

        // 자동으로 생성된 Rpc 서비스를 사용하기 위해 등록함
        Client.AddRpcService(new ActorViewRpcServiceView());

        CustomResolver.Register(GeneratedResolver.Instance);
    }

    private void OnConnected()
    {
        LoginRpc = new LoginRpc(_client.Client, null, TimeSpan.FromSeconds(10));
        ShopRpc = new ShopRpc(_client.Client, null, TimeSpan.FromSeconds(10));
    }

    private void OnClosed()
    {
        LoginRpc = null;
        ShopRpc = null;
    }

    private Task OnReceive(NetDataReader reader)
    {
        return Task.CompletedTask;
    }

    private void OnP2pGroupLeave(ushort sessionId, bool isMine)
    {
        // 다른유저가 그룹에서 떠나면
        if (isMine == false)
        {
            // 해당 유저의 액터를 삭제한다
            ActorManager.Instance.ActorList.ForEach((Actor actor) =>
            {
                if (actor.View.OwnerSessionId == sessionId)
                {
                    NetClientGlobal.Instance.Destroy(actor.View.ViewId);
                }
            });
        }
    }
}
