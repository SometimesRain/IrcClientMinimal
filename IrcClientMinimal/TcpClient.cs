using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IrcClientMinimal
{
	public delegate void IOCallback(byte[] data, int numBytes);
	public class TcpClient
	{
		private byte[] recvBuffer;
		private Socket socket;

		private IOCallback onReceive;
		private IOCallback onSend;
		private IOCallback onDisconnect;

		public bool Connected => socket.Connected;

		public TcpClient(IOCallback onReceive, IOCallback onSend, IOCallback onDisconnect)
		{
			this.onReceive = onReceive;
			this.onSend = onSend;
			this.onDisconnect = onDisconnect;

			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.NoDelay = true;
			recvBuffer = new byte[1024];
		}

		//########################################## Operations ########################################

		public void Connect(EndPoint endPoint)
		{
			socket.Connect(endPoint);
		}

		public void Disconnect()
		{
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();

			onDisconnect?.Invoke(null, 0);
		}

		public void Receive()
		{
			socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
		}

		public void Send(byte[] data)
		{
			socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), data);
		}

		//########################################## Callbacks #########################################

		private void OnReceive(IAsyncResult ar)
		{
			try
			{
				int dataAmount = socket.EndSend(ar);

				//Receiving 0 bytes via TCP means a disconnect
				if (dataAmount == 0)
					Disconnect();
				else
					onReceive?.Invoke(recvBuffer, dataAmount);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void OnSend(IAsyncResult ar)
		{
			try
			{
				int dataAmount = socket.EndSend(ar);
				byte[] data = (byte[])ar.AsyncState;

				onSend?.Invoke(data, dataAmount);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
