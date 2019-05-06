using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IrcClientMinimal
{
	public class IrcClient
	{
		private TcpClient client;
		private string channel = "";

		public IrcClient(string nick, string domain, int port)
		{
			Console.WriteLine("Connecting to " + domain + ":" + port);

			client = new TcpClient(OnReceive, null, OnDisconnect);
			client.Connect(new DnsEndPoint(domain, port));

			//Send NICK and USER messages
			EncodeAndSend("NICK " + nick + "\r\n");
			EncodeAndSend("USER " + nick + " 8 * " + nick + "\r\n"); //https://tools.ietf.org/html/rfc2812#section-3.1.3

			//Start receiving
			client.Receive();
		}

		public void EncodeAndSend(string message)
		{
			Console.Write(message);

			//IRC uses UTF-8 encoding
			client.Send(Encoding.UTF8.GetBytes(message));
		}

		private void OnDisconnect(byte[] data, int numBytes)
		{
			Console.WriteLine("Disconnected");
		}

		private void OnReceive(byte[] data, int numBytes)
		{
			//IRC uses UTF-8 encoding
			string message = Encoding.UTF8.GetString(data, 0, numBytes);
			Console.Write(message);

			//Reply to PING with a PONG
			if (message.Length >= 4 && message.Substring(0, 4) == "PING") //e.g. PING :irc.x2x.cc
				EncodeAndSend("PONG" + message.Substring(4));

			//Keep on receiving
			client.Receive();
		}

		public void EnterInputLoop()
		{
			while (client.Connected)
			{
				string message = Console.ReadLine();
				if (message.Length == 0)
					break;

				if (message.Length >= 4 && message.Substring(0, 4).ToUpper() == "JOIN")
				{
					//Set channel for convenience
					channel = message.Substring(5);
					EncodeAndSend(message + "\r\n");
				}
				else if (message.Length >= 4 && message.Substring(0, 4).ToUpper() == "NICK")
				{
					//Send NICK message
					EncodeAndSend(message + "\r\n");
				}
				else if (message.Length >= 4 && message.Substring(0, 4).ToUpper() == "/MSG")
				{
					//Parse "/msg <nick> <message>" command
					string[] args = message.Split(' ');
					EncodeAndSend("PRIVMSG " + args[1] + " :" + message.Substring(args[1].Length + 5) + "\r\n");
				}
				else
				{
					//Send message to selected channel
					EncodeAndSend("PRIVMSG " + channel + " :" + message + "\r\n");
				}
			}
		}
	}
}
