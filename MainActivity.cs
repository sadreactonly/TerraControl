using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Bluetooth;
using System.Threading;
using System.Linq;
using System;
using Android.Util;
using System.Collections.Generic;
using Android.Content;
using System.Text;
using Android.Graphics;
using System.Reflection;

namespace TerraControl
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
		static readonly string TAG = "X:" + typeof(MainActivity).Name;
		bool isSendClicked = false;
		Thread listenThread;

		BluetoothSocket socket;
		BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
		BluetoothDevice device;

		Button buttonConnect;
		Button buttonDisconnect;
		Button buttonSendCommand;
		Button getTemperature;
		Button getHumidity;

		TextView temperatureText;
		TextView humidityText;
		List<Color> colors = new List<Color>();
		SeekBar seekBarColor;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			SetContentView(Resource.Layout.activity_main);

			GetUIComponents();
			SetUIComponents();

			colors = GetAllColors();

			SetUIComponentsHandlers();

		}
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		#region UI
		private void SetUIComponents()
		{
			buttonDisconnect.Enabled = false;
			getTemperature.Enabled = false;
			getHumidity.Enabled = false;
			buttonSendCommand.Enabled = false;
			seekBarColor.Max = colors.Count();
		}

		private void SetUIComponentsHandlers()
		{
			buttonConnect.Click += ButtonConnect_Click;
			buttonDisconnect.Click += ButtonDisconnect_Click;
			buttonSendCommand.Click += ButtonSendCommand_Click;
			getTemperature.Click += GetTemperature_Click;
			getHumidity.Click += GetHumidity_Click;
			seekBarColor.ProgressChanged += SeekBarColor_ProgressChanged;
		}

		private void GetUIComponents()
		{
			buttonConnect = FindViewById<Button>(Resource.Id.button1);
			buttonDisconnect = FindViewById<Button>(Resource.Id.button2);
			buttonSendCommand = FindViewById<Button>(Resource.Id.button3);
			getTemperature = FindViewById<Button>(Resource.Id.button4);
			getHumidity = FindViewById<Button>(Resource.Id.button5);
			temperatureText = FindViewById<TextView>(Resource.Id.textView1);
			humidityText = FindViewById<TextView>(Resource.Id.textView2);
			seekBarColor = FindViewById<SeekBar>(Resource.Id.seekBar1);
		}
		#endregion


		private void SetColor(int r, int g, int b)
		{
			var stringToSend = "<"+"C"+r+","+g+","+b+">";
			byte[] send = Encoding.ASCII.GetBytes(stringToSend);

			socket.OutputStream.Write(send, 0, send.Length);
			socket.OutputStream.Close();

		}
		private List<Color> GetAllColors()
		{
			List<Color> allColors = new List<Color>();

			foreach (PropertyInfo property in typeof(Color).GetProperties())
			{
				if (property.PropertyType == typeof(Color))
				{
					allColors.Add((Color)property.GetValue(null));
				}
			}

			return allColors;
		}

		#region Handlers
		private void GetHumidity_Click(object sender, EventArgs e)
		{
			var stringToSend = "<H>";
			byte[] send = Encoding.ASCII.GetBytes(stringToSend);

			socket.OutputStream.Write(send, 0, send.Length);
			socket.OutputStream.Close();
		}

		private void GetTemperature_Click(object sender, EventArgs e)
		{
			var stringToSend = "<T>";
			byte[] send = Encoding.ASCII.GetBytes(stringToSend);

			socket.OutputStream.Write(send, 0, send.Length);
			socket.OutputStream.Close();

		}

		private void BluetoothDisabledAlert()
		{

			Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
			Android.App.AlertDialog alert = dialog.Create();
			alert.SetTitle("Bluetooth alert");
			alert.SetMessage("Turn on bluetooth.");
			alert.SetButton("OK", (c, ev) =>
			{
				// Ok button click task  
			});
			alert.Show();

		}

		private void ButtonConnect_Click(object sender, EventArgs e)
		{
			if(!adapter.IsEnabled)
			{
				BluetoothDisabledAlert();
				return;
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
				Log.Debug(TAG,exception.ToString());
			}

			adapter.CancelDiscovery();

			
			socket = device.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));


			try
			{
				socket.Connect();
				buttonDisconnect.Enabled = true;
				buttonConnect.Enabled = false;
				getTemperature.Enabled = true;
				getHumidity.Enabled = true;
				buttonSendCommand.Enabled = true;

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
		

		}

		private void ButtonSendCommand_Click(object sender, System.EventArgs e)
		{
			try
			{
				
				if(isSendClicked == false)
				{
					var stringToSend = "<L>";
					byte[] send = Encoding.ASCII.GetBytes(stringToSend);

					socket.OutputStream.Write(send, 0, send.Length);
					socket.OutputStream.Close();

					isSendClicked = true;
					buttonSendCommand.Text = "Light off.";

				}
				else
				{
					var stringToSend = "<N>";
					byte[] send = Encoding.ASCII.GetBytes(stringToSend);

					socket.OutputStream.Write(send, 0, send.Length);
					socket.OutputStream.Close();

					isSendClicked = false;
					buttonSendCommand.Text = "Light on.";
				}

			}
			catch (Exception exception)
			{
				Log.Debug(TAG, exception.ToString());
			}
		}

		private void ButtonDisconnect_Click(object sender, System.EventArgs e)
		{
			try
			{
				buttonDisconnect.Enabled = false;
				getTemperature.Enabled = false;
				getHumidity.Enabled = false;
				buttonSendCommand.Enabled = false;
				buttonConnect.Enabled = true;
				
				listenThread.Abort();
				listenThread = null;

				device.Dispose();

				socket.OutputStream.WriteByte(187);
				socket.OutputStream.Close();

				socket.Close();

				socket = null;
			}
			catch(Exception exception) { Log.Debug(TAG, exception.ToString()); };
		}

		private void SeekBarColor_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
		{
			var color = colors[e.Progress];

			SetColor(color.R, color.G, color.B);
		}

		#endregion

		async void Listener()
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

					if(read[0]==0xFF)
					{
						socket.InputStream.Close();
						var result = Encoding.ASCII.GetString(buffer.ToArray());
						RunOnUiThread(() =>
						{
							if (result[0] == 'T')
							{
								temperatureText.Text = "\t"+ (int) result[1] + " °C";
								buffer = new List<byte>();
							}
							else if (result[0] == 'H')
							{
								humidityText.Text = "\t"+(int)result[1]+" %";
								buffer = new List<byte>();
							}

						});

					}

				}
				catch(Exception exception) { Log.Debug(TAG, exception.ToString()); }

			}
		}

	}

}