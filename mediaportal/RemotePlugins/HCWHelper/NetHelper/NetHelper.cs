#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using MediaPortal.GUI.Library;

namespace NetHelper
{
  public class Connection
  {
    bool logVerbose;

    Socket socket;
    IPAddress hostIP = IPAddress.Parse("127.0.0.1");

    public delegate void ReceiveEventHandler(string strReceive);
    public event ReceiveEventHandler ReceiveEvent;


    protected virtual void OnReceive(string strReceive)
    {
      if (ReceiveEvent != null)
      {
        ReceiveEvent(strReceive);
      }
    }


    class UdpState
    {
      public IPEndPoint EndPoint;
      public UdpClient UdpClient;
    }


    public Connection(bool log)
    {
      logVerbose = log;
      socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }


    public bool Send(int udpPort, string strType, string strSend, DateTime timeStamp)
    {
      try
      {
        byte[] sendbuf = Encoding.UTF8.GetBytes(string.Format("{0}|{1}|{2}~", strType, strSend, timeStamp.ToBinary()));
        IPEndPoint endPoint = new IPEndPoint(hostIP, udpPort);
        socket.SendTo(sendbuf, endPoint);
        return true;
      }
      catch (SocketException se)
      {
        Log.Write("NetHelper: Send port {0}: {1} - {2}", udpPort, se.ErrorCode, se.Message);
        return false;
      }
    }


    public bool Start(int udpPort)
    {
      try
      {
        if (logVerbose) Log.Write("NetHelper: starting listener on port {0}", udpPort);

        // Port already used?
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
        foreach (TcpConnectionInformation c in connections)
          if (c.RemoteEndPoint.Port == udpPort)
          {
            Log.Write("NetHelper: udp port {0} is already in use", udpPort);
            return false;
          }
        IPAddress hostIP = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(hostIP, udpPort);
        UdpClient udpClient = new UdpClient(endPoint);
        UdpState state = new UdpState();
        state.EndPoint = endPoint;
        state.UdpClient = udpClient;
        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), state);
        if (logVerbose) Log.Write("NetHelper: listening for messages on port {0}", udpPort);
        return true;
      }
      catch (SocketException se)
      {
        Log.Write("NetHelper: Start port {0}: {1} - {2}", udpPort, se.ErrorCode, se.Message);
        return false;
      }
    }


    public void ReceiveCallback(IAsyncResult ar)
    {
      UdpClient udpClient = (UdpClient)((UdpState)(ar.AsyncState)).UdpClient;
      IPEndPoint endPoint = (IPEndPoint)((UdpState)(ar.AsyncState)).EndPoint;
      Byte[] bytesReceived = udpClient.EndReceive(ar, ref endPoint);
      string strReceived = Encoding.ASCII.GetString(bytesReceived);
      OnReceive(strReceived);
      udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), (UdpState)(ar.AsyncState));
    }

  }
}
