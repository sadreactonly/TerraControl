using System;
using Android.App;
using Android.Widget;
using Android.Bluetooth;
using System.Threading;
using System.Linq;
using Android.Util;
using System.Collections.Generic;
using Android.Graphics;

namespace TerraControl.Services
{
	public class CommunicationService
	{
		static readonly string TAG = "X:" + typeof(CommunicationService).Name;
		Thread listenThread;

		BluetoothSocket socket;
		BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
		BluetoothDevice device;
		Activity activity;
		
		public CommunicationService(Activity ac)
		{
			activity = ac;
		}

		public bool Connect()
		{
			if (!adapter.IsEnabled)
			{
				BluetoothDisabledAlert();
				return false;
			}

			adapter.StartDiscovery();

			try
			{
				device = adapter.BondedDevices.Where(x => x.Name == "HC-06").FirstOrDefault();
				device.SetPairingConfirmation(false);
				device.Dispose();
				device.SetPairingConfirmation(true);
				device.CreateBond();


			}
			catch (Exception exception)
			{
				Log.Debug(TAG, exception.ToString());
				//return false;
			}

			adapter.CancelDiscovery();


			socket = device.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));


			try
			{
				socket.Connect();
				listenThread = new Thread(Listener);
				if (listenThread.IsAlive == false)
				{
					listenThread.Start();
				}
			}
			catch (Exception exception)
			{
				Toast.MakeText(Application.Context, "Cannot connect to HC-06.", ToastLength.Short).Show();
				Log.Debug(TAG, exception.ToString());
				return false;
			}

			return true;
		}

		public void Disconnect()
		{
			try
			{
				listenThread.Abort();
				listenThread = null;

				device.Dispose();

				socket.OutputStream.WriteByte(187);
				socket.OutputStream.Close();

				socket.Close();

				socket = null;
			}
			catch (Exception exception)
			{
				Log.Debug(TAG, exception.ToString());
			};
		}

		public void Write(byte[] bytes)
		{
			socket.OutputStream.Write(bytes, 0, bytes.Length);
			socket.OutputStream.Close();
		}
		private async void Listener()
		{
			byte[] read = new byte[1];
			var temperatureText = activity.FindViewById<TextView>(Resource.Id.textView1);
			var humidityText = activity.FindViewById<TextView>(Resource.Id.textView2);
			var temperatureButton = activity.FindViewById<ImageButton>(Resource.Id.imageButton2);
			var humidityButton = activity.FindViewById<ImageButton>(Resource.Id.imageButton1);
			var lightButton = activity.FindViewById<ImageButton>(Resource.Id.imageButton3);
			var colorDialogButton = activity.FindViewById<ImageButton>(Resource.Id.imageButton4);
			var switchBT = activity.FindViewById<Switch>(Resource.Id.switch1);
			var fanButton = activity.FindViewById<ImageButton>(Resource.Id.imageButton5);

			List<byte> buffer = new List<byte>();
			while (true)
			{

				try
				{
					if (socket.InputStream.Read(read, 0, read.Length) > 0)
					{
						if (read.Count() == 1 && read[0] != 0xFF)
							buffer.AddRange(read);
					}

					if (read[0] == 0xFF)
					{
						socket.InputStream.Close();
						activity.RunOnUiThread(() =>
						{
							if (buffer[0] == 'T')
							{
								temperatureText.Text = "\t" + (int)buffer[1] + " °C";
								temperatureButton.SetBackgroundColor(BackgroundConverter.GetFromTemperature(buffer[1]));
								buffer = new List<byte>();
							}
							else if (buffer[0] == 'H')
							{
								humidityText.Text = "\t" + (int)buffer[1] + " %";
								humidityButton.SetBackgroundColor(BackgroundConverter.GetFromHumidity(buffer[1]));
								buffer = new List<byte>();
							}
							else if (buffer[0] == 'C')
							{
								temperatureText.Text = "\t" + (int)buffer[1] + " °C";
								humidityText.Text = "\t" + (int)buffer[2] + " %";
								temperatureButton.SetBackgroundColor(BackgroundConverter.GetFromTemperature(buffer[1]));
								humidityButton.SetBackgroundColor(BackgroundConverter.GetFromHumidity((int)buffer[2]));
								if (buffer[3] == 0)
								{
									fanButton.SetBackgroundColor(BackgroundConverter.GetFromBool(false));
								}
								else
								{
									fanButton.SetBackgroundColor(BackgroundConverter.GetFromBool(true));
								}
								if (buffer[4] == 0)
								{
									lightButton.SetBackgroundColor(BackgroundConverter.GetFromBool(false));
								}
								else
								{
									lightButton.SetBackgroundColor(BackgroundConverter.GetFromBool(true));
								}
								colorDialogButton.SetBackgroundColor(new Color(buffer[5], buffer[6], buffer[7]));
								(activity as MainActivity).Color = new Color(buffer[5], buffer[6], buffer[7]);

								buffer = new List<byte>();
							}

						});

					}

				}
				catch (Exception exception) { Log.Debug(TAG, exception.ToString()); }

			}
		}

		private void BluetoothDisabledAlert()
		{
			Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(activity);
			Android.App.AlertDialog alert = dialog.Create();
			alert.SetTitle("Bluetooth alert");
			alert.SetMessage("Turn on bluetooth.");
			alert.SetButton("OK", (c, ev) =>
			{
				// Ok button click task  
			});
			alert.Show();
		}
	}
}