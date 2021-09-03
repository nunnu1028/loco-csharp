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
        private readonly ILocoSocket socket;
        private readonly Dictionary<int, TaskCompletionSource<LocoPacketResponse>> packetDict;
        private int currentPacketID;
        private Task listenTask;
        private CancellationTokenSource listenTokenSource;

        public LocoSession(ILocoSocket socket)
        {
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
            this.listenTask = Task.Factory.StartNew(() => this.ListenFunction(listenTokenSource.Token));
        }

        private void ListenFunction(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested) break;
                    LocoPacketResponse? response = this.socket.Receive();

                    if (response != null)
                    {
                        if (this.packetDict.TryGetValue(response.Value.packetID, out TaskCompletionSource<LocoPacketResponse> task))
                        {
                            task.SetResult(response.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    this.Close();
                    break;
                }
            }
        }
    }
}
