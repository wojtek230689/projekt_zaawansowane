﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using PROJEKT.Classes.Services;
using PROJEKT.Classes;
using PROJEKT.Interfaces;

using static PROJEKT.Interfaces.INetworkAction;
using System.Threading.Tasks;

namespace PROJEKT
{
    public class TestServer : INetworkAction
    {
        private static readonly object LOCKOBJECT = new object();

        public ServerService<ClientService> Server;

        public void StateChanged(State a_eState, StateObject a_oStateObj = null)
        {



            lock (LOCKOBJECT)
            {

                switch (a_eState)
                {
                    case State.Sending:
                        break;

                    case State.Sent:
                        break;

                    case State.Listening:
                        break;

                    case State.Established:
                        OnEstablished(a_oStateObj);
                        break;

                    case State.Connecting:
                        break;

                    case State.Connected:                        
                        break;

                    case State.Receiving:
                        break;

                    case State.Received:
                        OnReceived(a_oStateObj);
                        break;

                    case State.Error:
                        break;
                }
            }
        }

        protected void OnReceived(StateObject a_oStateObj)
        {

            var _client = a_oStateObj.GetObject<ClientService>();

            var _message = MessageFactory.Instance.Create(_client.Data.BufferWithData) as IMessage;


            if (_message.ProcessRequest(a_oStateObj) != null)
            {
                _client.AsyncSend(_message.AsNetworkData());
            }

            _client.AsyncReceive();
        }

        protected void OnEstablished(StateObject a_oStateObj)
        {

            a_oStateObj.GetData<ClientService>().AsyncReceive();
        }

        public virtual void Run()
        {

            Server = new ServerService<ClientService>(IPAddress.Loopback, 1000)
            {
                NetworkAction = this
            };

            Server.Establish();

            Console.ReadKey();
        }
    }
}
