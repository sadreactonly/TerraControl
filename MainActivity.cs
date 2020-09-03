using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using Android.Graphics;
using TerraControl.Services;
using ChiralCode.ColorPickerLib;
using static ChiralCode.ColorPickerLib.ColorPickerDialog;

namespace TerraControl
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
	public class MainActivity : AppCompatActivity
	{
		public ImageButton LightButton { get; set; }
		public ImageButton TemperatureButton { get; set; }
		public ImageButton HumidityButton { get; set; }
		public ImageButton ColorDialogButton { get; set; }
		public ImageButton FanButton { get; set; }
		public ImageButton WaterButton { get; set; }
		public Switch WwitchBT { get; set; }
		public ColorPickerDialog colorDialog { get; set; }
		public TextView temperatureText;
		public TextView humidityText;


		private CommunicationService communicationService;
		private ResponseEngine responseEngine;
		public Color Color { get; set; } = new Color(0, 0, 0);

		bool isFanOn = false;
		bool isLightOn = false;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			SetContentView(Resource.Layout.activity_main);
			
			communicationService = new CommunicationService();
			responseEngine = new ResponseEngine();
			
			communicationService.BluetoothDisabledEvent += BluetoothDisabled;
			communicationService.ResultEvent += CommunicationService_ResultEvent;

			responseEngine.HandleConfigurationParsedEvent += ResponseEngine_HandleConfigurationParsedEvent;
			responseEngine.HandleTempertureEvent += ResponseEngine_HandleTempertureEvent;
			responseEngine.HandleHumidityEvent += ResponseEngine_HandleHumidityEvent;

			GetUIComponents();
			SetUIComponents(false);
			SetUIComponentsHandlers();
		}

		private void ResponseEngine_HandleHumidityEvent(int humidity)
		{
			humidityText.Text = "\t" + humidity + " %";
			HumidityButton.SetBackgroundColor(BackgroundConverter.GetFromHumidity(humidity));
		}

		private void ResponseEngine_HandleTempertureEvent(int temp)
		{
			temperatureText.Text = "\t" + temp + " °C";
			TemperatureButton.SetBackgroundColor(BackgroundConverter.GetFromTemperature(temp));
		}

		private void ResponseEngine_HandleConfigurationParsedEvent(int temperatureText, int humidityText, bool isFanOn, bool isLightOn, int R, int G, int B)
		{
			ResponseEngine_HandleTempertureEvent(temperatureText);
			ResponseEngine_HandleHumidityEvent(humidityText);
			FanButton.SetBackgroundColor(BackgroundConverter.GetFromBool(isFanOn));
			LightButton.SetBackgroundColor(BackgroundConverter.GetFromBool(isLightOn));
			ColorDialogButton.SetBackgroundColor(new Color(R,G,B));
		}

		private void CommunicationService_ResultEvent(System.Collections.Generic.List<byte> resultBuffer)
		{
			responseEngine.ParseResultBuffer(resultBuffer.ToArray());
		}

		private void BluetoothDisabled(object sender, EventArgs e)
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

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		#region UI
		private void GetUIComponents()
		{
			LightButton = FindViewById<ImageButton>(Resource.Id.imageButton3);
			TemperatureButton = FindViewById<ImageButton>(Resource.Id.imageButton2);
			HumidityButton = FindViewById<ImageButton>(Resource.Id.imageButton1);
			ColorDialogButton = FindViewById<ImageButton>(Resource.Id.imageButton4);
			WwitchBT = FindViewById<Switch>(Resource.Id.switch1);
			FanButton = FindViewById<ImageButton>(Resource.Id.imageButton5);
			WaterButton = FindViewById<ImageButton>(Resource.Id.imageButton6);
			temperatureText = FindViewById<TextView>(Resource.Id.textView1);
		    humidityText = FindViewById<TextView>(Resource.Id.textView2);
	}
		private void SetUIComponents(bool isEnabled)
		{
			TemperatureButton.Enabled = isEnabled;
			HumidityButton.Enabled = isEnabled;
			LightButton.Enabled = isEnabled;
			ColorDialogButton.Enabled = isEnabled;
			FanButton.Enabled = isEnabled;
		    WaterButton.Enabled = isEnabled;
		}
		private void SetUIComponentsHandlers()
		{
			LightButton.Click += ButtonLightCommand_Click;
			TemperatureButton.Click += GetTemperature_Click;
			HumidityButton.Click += GetHumidity_Click;
			ColorDialogButton.Click += ColorDialogButton_Click;
			WwitchBT.Click += SwitchBT_Click;
			FanButton.Click += FanButton_Click;
		}
		#endregion

		#region Handlers
		private void SwitchBT_Click(object sender, EventArgs e)
		{
			var sw = sender as Switch;

			if (sw.Checked)
			{
				if (communicationService.Connect())
				{
					SetUIComponents(true);
					RequestGuiInfo();

					Action<int> act = OnColorSelected;
					colorDialog = new ColorPickerDialog(this, Color, new ColorSelectedListener(act));
				}
				else
				{
					WwitchBT.Checked = false;
					SetUIComponents(false);
				}
			}
			else
			{
				SetUIComponents(false);
				communicationService.Disconnect();
			}
		}
		private void GetHumidity_Click(object sender, EventArgs e)
		{
			Message message = new Message((byte)CommandCode.ReadHumidity);
			message.Create();

			communicationService.Write(message.RawBytes);
		}
		private void GetTemperature_Click(object sender, EventArgs e)
		{
			Message message = new Message((byte)CommandCode.ReadTemperature);
			message.Create();

			communicationService.Write(message.RawBytes);
		}
		private void ButtonLightCommand_Click(object sender, System.EventArgs e)
		{
			Message message;
			if(isLightOn == false)
			{
				message = new Message((byte)CommandCode.LightOn, Color.R, Color.G, Color.B);
				message.Create();

				isLightOn = true;
			}
			else
			{
				message = new Message((byte)CommandCode.LightOff);
				message.Create();

				isLightOn = false;
			}
			communicationService.Write(message.RawBytes);

			LightButton.SetBackgroundColor(BackgroundConverter.GetFromBool(isLightOn));
		}
		private void ColorDialogButton_Click(object sender, EventArgs e)
		{
			colorDialog.Show();
		}
		private void FanButton_Click(object sender, EventArgs e)
		{
			Message message;

			if (isFanOn)
			{
				message = new Message((byte)CommandCode.FanOff);
				message.Create();

				isFanOn = false;
			}
			else
			{
				message = new Message((byte)CommandCode.FanOn);
				message.Create();

				isFanOn = true;
			}

			communicationService.Write(message.RawBytes);
			FanButton.SetBackgroundColor(BackgroundConverter.GetFromBool(isFanOn));
		}
		#endregion

		private void RequestGuiInfo()
		{
			Message message = new Message((byte)CommandCode.ReadConfig);
			message.Create();

			communicationService.Write(message.RawBytes);
		}
		private void OnColorSelected(int color)
		{
			Message message;
			Color = new Color(color);

			message = new Message(Color.R, Color.G, Color.B);
			message.Create();

			communicationService.Write(message.RawBytes);

			ColorDialogButton.SetBackgroundColor(this.Color);
			if (!isLightOn)
				isLightOn = true;

			LightButton.SetBackgroundColor(BackgroundConverter.GetFromBool(isLightOn));
		}
	}
}