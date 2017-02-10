using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CallService
{
	public sealed class CallService
	{
		public static async void callService(string command)
		{
			using (var client = new HttpClient())
			{
				string strUri = "http://helloclassroom.azurewebsites.net/api/go/" + command;
				
				var result = await client.GetAsync(strUri);
				string resultContent = await result.Content.ReadAsStringAsync();
				System.Diagnostics.Debug.WriteLine(resultContent);
			}
		}
	}
}