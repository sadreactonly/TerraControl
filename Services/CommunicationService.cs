using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Bluetooth;
using Android.Views;
using System.Threading;
using System.Linq;
using Android.Util;
using System.Collections.Generic;
using Android.Content;
using System.Text;
using Android.Graphics;
using System.Reflection;
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
		ProgressBar progressBar;

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
			///listenThread.Start();

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
						var result = Encoding.ASCII.GetString(buffer.ToArray());
						activity.RunOnUiThread(() =>
						{
							if (result[0] == 'T')
							{
								temperatureText.Text = "\t" + (int)result[1] + " °C";
								temperatureButton.SetBackgroundColor(BackgroundConverter.GetFromTemperature((int)result[1]));
								buffer = new List<byte>();
							}
							else if (result[0] == 'H')
							{
								humidityText.Text = "\t" + (int)result[1] + " %";
								buffer = new List<byte>();
								humidityButton.SetBackgroundColor(BackgroundConverter.GetFromHumidity((int)result[1]));

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