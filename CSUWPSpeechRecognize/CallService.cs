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
				string strUri = "http://localhost:51636/api/go/" + command;
				
				var result = await client.GetAsync(strUri);
				string resultContent = await result.Content.ReadAsStringAsync();
				System.Diagnostics.Debug.WriteLine(resultContent);
			}
		}
		public static async void callPostService(string command)
		{
			using (var client = new HttpClient())
			{
				string strUri = "http://localhost:51636/";
				client.BaseAddress = new Uri(strUri);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("text", command)
				});

				// TODO: Check this.
				//StringContent content = new StringContent(command);
				var result = await client.PostAsync("api/go", content);
				string resultContent = await result.Content.ReadAsStringAsync();
				System.Diagnostics.Debug.WriteLine(resultContent);
			}
		}
	}
}