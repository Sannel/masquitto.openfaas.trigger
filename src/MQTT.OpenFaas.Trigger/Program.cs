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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace MQTT.OpenFaas.Trigger
{
	class Program
	{
		private static IConfiguration configuration;
		private static ILogger logger;
		internal static IHttpClientFactory Factory;
		internal static MQTTSettings MQTTSettings;
		internal static IServiceProvider ServiceProvider;

		static async Task Main(string[] args)
		{
			IConfigurationBuilder builder = new ConfigurationBuilder();
			builder.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			builder.AddYamlFile("app_config/appsettings.yml");

			var v = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

			configuration = builder.Build();
			v.AddSingleton<IConfiguration>(configuration);
			v.AddLogging(b =>
			{
				b.AddConfiguration(configuration);
				b.AddConsole();
			});
			v.AddTransient<MQTTSettings>();
			v.AddTransient<Subscription>();

			MQTTSettings = configuration.GetSection("MQTT").Get<MQTTSettings>();

			foreach(var sub in MQTTSettings.Subscriptions)
			{
				v.AddHttpClient(sub.Topics.FirstOrDefault(), c =>
				{
					c.BaseAddress = new Uri(configuration["OpenFaas:gateway"]);
				});
			}

			ServiceProvider = v.BuildServiceProvider();

			Factory = ServiceProvider.GetService<IHttpClientFactory>();

			logger = ServiceProvider.GetService<ILogger<Program>>();


			foreach(var sub in MQTTSettings.Subscriptions)
			{
				await sub.SetupAndStartClientAsync();
			}

			while (true)
			{
				if(logger.IsEnabled(LogLevel.Debug))
				{
					logger.LogDebug("Main Loop tick");
				}
				await Task.Delay(TimeSpan.FromSeconds(5));
			}
		}
	}
}
