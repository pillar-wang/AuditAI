﻿using System;
using System.IO;
using System.Threading.Tasks;
using Leqisoft.DTO;

namespace Leqisoft.Util;

public static class Program
{
	private class TestUser
	{
		public long Id { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public Guid TeamId { get; set; }
	}

	public static void Main(string[] args)
	{
		Run().Wait();
	}

	private static async Task Run()
	{
		Random random = new Random();
		using StreamWriter sw = new StreamWriter("d:\\test\\1.json", append: true);
		sw.AutoFlush = true;
		for (int team = 351; team <= 450; team++)
		{
			int member2 = 1;
			string password2 = random.Next(1000000).ToString("D6");
			User user2 = new User
			{
				UserName = $"leqi{team:D3}{member2:D2}",
				Password = password2,
				Name = $"leqi{team:D3}{member2:D2}"
			};
			sw.Write(await WebApiClient.SingleRegister(user2));
			sw.Write("," + user2.UserName + "," + password2 + ",");
			await WebApiClient.AccountLogin(user2.UserName, Encrypts.SHA256Encrypt(password2, isUrl: true));
			Guid guid = (Guid)(await WebApiClient.CreateTeam(null, 1))["TeamId"];
			sw.Write(guid);
			await WebApiClient.CreateDemo();
			for (member2 = 2; member2 <= 20; member2++)
			{
				password2 = random.Next(1000000).ToString("D6");
				user2 = new User
				{
					UserName = $"leqi{team:D3}{member2:D2}",
					Password = password2,
					Name = $"leqi{team:D3}{member2:D2}"
				};
				await WebApiClient.SingleRegister(user2);
				await WebApiClient.AddUserToTeam(user2.UserName);
				sw.Write("," + user2.UserName + "," + password2);
			}
			sw.WriteLine();
			await WebApiClient.ClientQuit();
		}
	}
}
