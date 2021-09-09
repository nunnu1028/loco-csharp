using KakaoLoco.Network.Socket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static KakaoLoco.Network.Packet.Packet;

namespace KakaoLoco.Network
{
    public class LocoSession
    {
        public readonly Dictionary<string, Action<LocoPacketResponse>> handlerDict;
        private readonly ILocoSocket socket;
        private readonly Dictionary<int, TaskCompletionSource<LocoPacketResponse>> packetDict;
        private int currentPacketID;
        private CancellationTokenSource listenTokenSource;

        public LocoSession(ILocoSocket socket)
        {
            this.handlerDict = new();
            this.socket = socket;
            this.packetDict = new();
            this.currentPacketID = -1;

            this.Listen();
        }

        public LocoPacketResponse Request(string method, JObject body)
        {
            byte[] requestPacket = ToLocoPacketRequest(++this.currentPacketID, method, body);
            TaskCompletionSource<LocoPacketResponse> task = new();
            this.packetDict.Add(this.currentPacketID, task);
            this.socket.Send(requestPacket);
            task.Task.Wait();

            return task.Task.Result;
        }

        public void Close()
        {
            this.listenTokenSource.Cancel();
            this.socket.Close();
        }

        private void Listen()
        {
            this.listenTokenSource = new();
            Task.Factory.StartNew(() => this.ListenFunction(listenTokenSource.Token));
        }

        private void ListenFunction(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested)
                        break;
                    LocoPacketResponse? response = this.socket.Receive();

                    if (response != null)
                    {
                        if (this.handlerDict.TryGetValue(response.Value.method, out Action<LocoPacketResponse> value))
                            value.Invoke(response.Value);
                        if (this.packetDict.TryGetValue(response.Value.packetID, out TaskCompletionSource<LocoPacketResponse> task))
                            task.SetResult(response.Value);
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "Unable to read data from the transport connection: 현재 연결은 사용자의 호스트 시스템의 소프트웨어의 의해 중단되었습니다..")
                        Console.WriteLine(e.Message);
                    this.Close();
                    break;
                }
            }
        }
    }
}
