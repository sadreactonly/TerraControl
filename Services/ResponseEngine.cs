using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TerraControl.Services
{
	public class ResponseEngine
	{
		public delegate void HandleConfigurationParsed(int temperature, int humidity, bool isFanOn, bool isLightOn, int R, int G, int B);
		public delegate void HandleTemperture(int temp);
		public delegate void HandleHumidity(int humidity);

		public event HandleConfigurationParsed HandleConfigurationParsedEvent;
		public event HandleTemperture HandleTempertureEvent;
		public event HandleHumidity HandleHumidityEvent;

		public void ParseResultBuffer(byte[] buffer)
		{

			if (buffer[0] == 'T')
			{
				var temptext = (int)buffer[1];
				HandleTempertureEvent.Invoke(temptext);
			}
			else if (buffer[0] == 'H')
			{
				var humtext = (int)buffer[1] ;
				HandleHumidityEvent.Invoke(humtext);
			}
			else if (buffer[0] == 'C')
			{
				var temperatureText = (int)buffer[1] ;
				var humidityText =  (int)buffer[2] ;
				var isFanOn = buffer[3] == 0 ? false : true;
				var isLightOn = buffer[4] == 0 ? false : true;

				int R = buffer[5];
				int G = buffer[6];
				int B = buffer[7];

				HandleConfigurationParsedEvent?.Invoke(temperatureText, humidityText, isFanOn, isLightOn, R, G, B);
			}

		}

	}
}
