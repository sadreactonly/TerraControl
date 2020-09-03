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

		public delegate void Result(List<byte> resultBuffer);
		public event Result ResultEvent;
		public event EventHandler BluetoothDisabledEvent;

		public CommunicationService()
		{
		}

		public bool Connect()
		{
			if (!adapter.IsEnabled)
			{
				BluetoothDisabledEvent.Invoke(this, new EventArgs());
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

		private void Listener()
		{
			byte[] read = new byte[1];

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
						ResultEvent?.Invoke(buffer);
					}

				}
				catch (Exception exception) { Log.Debug(TAG, exception.ToString()); }

			}
		}
	}
}