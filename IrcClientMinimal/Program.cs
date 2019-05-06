using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrcClientMinimal
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Enter nickname: ");
			string nick = Console.ReadLine();

			IrcClient client = new IrcClient(nick, "irc.rizon.net", 6667);
			client.EnterInputLoop();
		}
	}
}
