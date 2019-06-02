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
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTT.OpenFaas.Trigger
{
	public class MQTTSettings
	{
		/// <summary>
		/// Gets or sets the server.
		/// </summary>
		/// <value>
		/// The server.
		/// </value>
		public string Server { get; set; }
		/// <summary>
		/// Gets or sets the port.
		/// </summary>
		/// <value>
		/// The port.
		/// </value>
		public int Port { get; set; }

		/// <summary>
		/// Gets or sets the client identifier.
		/// </summary>
		/// <value>
		/// The client identifier.
		/// </value>
		public string ClientId { get; set; }
		/// <summary>
		/// Gets or sets the subscriptions.
		/// </summary>
		/// <value>
		/// The subscriptions.
		/// </value>
		public Subscription[] Subscriptions { get; set; }
	}
}
