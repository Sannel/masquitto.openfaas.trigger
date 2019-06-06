/* Copyright 2019 Sannel Software, L.L.C.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
      http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;

namespace MQTT.OpenFaas.Trigger
{
	public class Subscription
	{
		private IManagedMqttClient mqttClient;
		private ILogger logger;

		protected ILogger Logger
			=> logger ?? (logger = Program.ServiceProvider.GetService<ILogger<Subscription>>());

		/// <summary>
		/// Gets or sets the topic.
		/// </summary>
		/// <value>
		/// The topic.
		/// </value>
		public string[] Topics
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the functions.
		/// </summary>
		/// <value>
		/// The functions.
		/// </value>
		public string[] Functions { get; set; }


		/// <summary>
		/// Gets or sets the type of the MIME.
		/// </summary>
		/// <value>
		/// The type of the MIME.
		/// </value>
		public string MimeType { get; set; } = "text/plain";


		/// <summary>
		/// Setups the and start client asynchronous.
		/// </summary>
		/// <returns></returns>
		public async Task SetupAndStartClientAsync()
		{
			// Setup and start a managed MQTT client.
			var options = new ManagedMqttClientOptionsBuilder()
				.WithAutoReconnectDelay(TimeSpan.FromMilliseconds(250))
				.WithClientOptions(new MqttClientOptionsBuilder()
					.WithClientId(Program.MQTTSettings.ClientId)
					.WithTcpServer(Program.MQTTSettings.Server, Program.MQTTSettings.Port)
					.Build())
				.Build();

			mqttClient = new MqttFactory().CreateManagedMqttClient();

			await mqttClient.SubscribeAsync(
				Topics?.Select(i => new TopicFilterBuilder().WithTopic(i).Build()).ToArray()
			);

			mqttClient.UseApplicationMessageReceivedHandler(this.MessageReceivedAsync);

			await mqttClient.StartAsync(options);
		}

		/// <summary>
		/// Messages the received asynchronous.
		/// </summary>
		/// <param name="message">The <see cref="MqttApplicationMessageReceivedEventArgs"/> instance containing the event data.</param>
		/// <returns></returns>
		protected async Task MessageReceivedAsync(MqttApplicationMessageReceivedEventArgs message)
		{
			if(Logger.IsEnabled(LogLevel.Debug))
			{
				Logger.LogDebug("Class Hash: {0}", this.GetHashCode());
				Logger.LogDebug("Topic: {0}", message.ApplicationMessage.Topic);
				Logger.LogDebug("Message: {0}", message.ApplicationMessage.ConvertPayloadToString());
			}

			if (Functions != null)
			{
				var client = Program.Factory.CreateClient(Topics.FirstOrDefault());
				foreach(var func in Functions)
				{
					if(!string.IsNullOrWhiteSpace(func))
					{
						using(var httpMessage = new HttpRequestMessage(HttpMethod.Post, $"/async-functions/{Uri.EscapeUriString(func)}"))
						{
							httpMessage.Content = new ByteArrayContent(message.ApplicationMessage.Payload);
							httpMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(MimeType);
							httpMessage.Headers.Add("Topic", message.ApplicationMessage.Topic);

							var response = await client.SendAsync(httpMessage);

							if(response.IsSuccessStatusCode)
							{
								if(Logger.IsEnabled(LogLevel.Debug))
								{
									Logger.LogDebug("Call to {0} successful", httpMessage.RequestUri);
								}
							}
							else
							{
								if(logger.IsEnabled(LogLevel.Error))
								{
									Logger.LogError("Call to {0} failed {1}", httpMessage.RequestUri, response.StatusCode);
								}
							}
						}
					}
				}
			}
		}
	}
}
