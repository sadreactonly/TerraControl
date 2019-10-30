//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;

//namespace TerraControl.Services
//{
//	public class ResponseEngine
//	{


//		void ParseRecieved(byte[] buffer)
//		{

//					if (buffer[0] == 'T')
//					{
//						temperatureText.Text = "\t" + (int)buffer[1] + " °C";
//						temperatureButton.SetBackgroundColor(BackgroundConverter.GetFromTemperature(buffer[1]));
//						buffer = new List<byte>().ToArray();
//					}
//					else if (buffer[0] == 'H')
//					{
//						humidityText.Text = "\t" + (int)buffer[1] + " %";
//						humidityButton.SetBackgroundColor(BackgroundConverter.GetFromHumidity(buffer[1]));
//						buffer = new List<byte>().ToArray();

//			}
//			else if (buffer[0] == 'C')
//					{
//						temperatureText.Text = "\t" + (int)buffer[1] + " °C";
//						humidityText.Text = "\t" + (int)buffer[2] + " %";
//						temperatureButton.SetBackgroundColor(BackgroundConverter.GetFromTemperature(buffer[1]));
//						humidityButton.SetBackgroundColor(BackgroundConverter.GetFromHumidity((int)buffer[2]));
//						if (buffer[3] == 0)
//						{
//							fanButton.SetBackgroundColor(BackgroundConverter.GetFromBool(false));
//						}
//						else
//						{
//							fanButton.SetBackgroundColor(BackgroundConverter.GetFromBool(true));
//						}
//						if (buffer[4] == 0)
//						{
//							lightButton.SetBackgroundColor(BackgroundConverter.GetFromBool(false));
//						}
//						else
//						{
//							lightButton.SetBackgroundColor(BackgroundConverter.GetFromBool(true));
//						}
//						colorDialogButton.SetBackgroundColor(new Color(buffer[5], buffer[6], buffer[7]));
//						(activity as MainActivity).Color = new Color(buffer[5], buffer[6], buffer[7]);

//						buffer = new List<byte>();
//					}

//			}

//		}
//	}
//}