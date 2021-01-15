﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using PROJEKT.Classes.Exceptions;
using PROJEKT.Interfaces;

using static PROJEKT.Interfaces.INetworkAction;

namespace PROJEKT.Classes
{
    public abstract class NetworkService
    {
        public const int BUFFER_SIZE = 1000000;

        public enum ModeEnum
        {
            Client = 0x001,
            Server = 0x002
        }        
        public string               Identifier { get; set; }
        public ModeEnum             Mode { get; protected set; }
        public INetworkAction       NetworkAction { get; set; }
        public IPAddress            Address { get; protected set; }
        public int                  Port { get; protected set; }
        public NetworkData          Data { get; protected set; }
        public NetworkService       RegisteredServer { get; set; }

        public NetworkService(ModeEnum Mode, int a_iBufferSize = BUFFER_SIZE)
        {
            this.Identifier = GetHashCode().ToString("X8");
            this.Mode = Mode;
            this.Data = new NetworkData(a_iBufferSize);
            this.NetworkAction = null;
            this.RegisteredServer = null;
        }
        public NetworkService(ModeEnum Mode, IPAddress Address, int Port, int a_iBufferSize = BUFFER_SIZE) : 
            this(Mode,a_iBufferSize)
        {
            this.Address = Address;
            this.Port = Port;
        }
        public abstract bool IsConnected { get; }
        public abstract Socket NetworkSocket { get; }
        public abstract void Establish();
        public virtual void AsyncSend(NetworkData a_oData)
        {
            try
            {
                NetworkAction?.StateChanged(State.Sending, new StateObject(this,a_oData));

                NetworkSocket?.BeginSend(a_oData.Buffer, 0, a_oData.DataLength(true), SocketFlags.None, new AsyncCallback(SendCallback), this);
            }
            catch (Exception)
            {
                NetworkAction?.StateChanged(State.Error,new StateObject(this));
            }
        }

        public virtual void AsyncReceive()
        {
            Data?.Clear();

            try
            {
                NetworkAction?.StateChanged(State.Receiving, new StateObject(this));

                NetworkSocket?.BeginReceive(Data.Buffer, 0, Data.BufferLength, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);
            }
            catch (Exception)
            {
                NetworkAction?.StateChanged(State.Error,new StateObject(this));
            }
        }

        public virtual StateObject SyncReceive()
        {
            try
            {
                Data?.Clear();

                NetworkSocket?.Receive(Data.Buffer, SocketFlags.None) ;

                return new StateObject(this, Data);
            }
            catch (Exception)
            {
            }

            NetworkAction?.StateChanged(State.Error);

            return null;            
        }

        protected virtual void SendCallback(IAsyncResult ar)
        {
            NetworkService _obj = ar.AsyncState as NetworkService;

            try
            {
                if (_obj.NetworkSocket.EndSend(ar) > 0)
                {
                    _obj.NetworkAction?.StateChanged(State.Sent,new StateObject(this));

                    return;
                }                
            }
            catch (Exception)
            {
            }

            _obj.NetworkAction?.StateChanged(State.Error);
        }

        protected virtual void ReceiveCallback(IAsyncResult ar)
        {
            NetworkService _obj = ar.AsyncState as NetworkService;

            try
            {
                int _iSize = _obj.NetworkSocket.EndReceive(ar);

                if (_iSize > 0 && (_obj.Data?.HasAnyData ?? false))
                {
                    _obj.NetworkAction?.StateChanged(State.Received,new StateObject(this, Data));

                    return;
                }
            }
            catch (Exception)
            {               
            }

            _obj.NetworkAction?.StateChanged(State.Error,new StateObject(this));
        }
        public override string ToString() => $"Identifier={Identifier}\n[{GetType().Name.CleanType()}={NetworkSocket?.LocalEndPoint}]";
        public virtual bool HasRegisteredServer => RegisteredServer != null;
        public virtual T GetRegisteredServer<T>() where T : NetworkService
        {
            return (T)RegisteredServer;
        }
    }
}
