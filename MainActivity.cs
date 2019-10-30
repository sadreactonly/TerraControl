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

		CommunicationService communicationService;
		public Color Color { get; set; } = new Color(0, 0, 0);

		bool isFanOn = false;
		bool isLightOn = false;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			SetContentView(Resource.Layout.activity_main);
			communicationService = new CommunicationService(this);

			GetUIComponents();
			SetUIComponents(false);
			SetUIComponentsHandlers();
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