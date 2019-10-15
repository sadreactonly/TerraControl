using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using System.Collections.Generic;
using Android.Graphics;
using TerraControl.Services;
using ChiralCode.ColorPickerLib;
using static ChiralCode.ColorPickerLib.ColorPickerDialog;
using Android.Views;

namespace TerraControl
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
		bool isSendClicked = false;

		ImageButton buttonLightCommand;
		ImageButton temperatureButton;
		ImageButton humidityButton;
		ImageButton colorDialogButton;
		ImageButton fanButton;
		ImageButton waterButton;

		Switch switchBT;

		TextView temperatureText;
		TextView humidityText;
		TextView buttonLightText;

		CommunicationService communicationService;
		ProgressBar progressBar;

		ColorPickerDialog colorDialog;

		Color color;
		bool isFanOn = false;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			SetContentView(Resource.Layout.activity_main);
			communicationService = new CommunicationService(this);

			GetUIComponents();
			SetUIComponents();
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
			buttonLightCommand = FindViewById<ImageButton>(Resource.Id.imageButton3);
			temperatureButton = FindViewById<ImageButton>(Resource.Id.imageButton2);
			humidityButton = FindViewById<ImageButton>(Resource.Id.imageButton1);
			colorDialogButton = FindViewById<ImageButton>(Resource.Id.imageButton4);
			temperatureText = FindViewById<TextView>(Resource.Id.textView1);
			humidityText = FindViewById<TextView>(Resource.Id.textView2);
			buttonLightText = FindViewById<TextView>(Resource.Id.textView3);
			switchBT = FindViewById<Switch>(Resource.Id.switch1);
			progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar1);
			fanButton = FindViewById<ImageButton>(Resource.Id.imageButton5);

		}

		private void SetUIComponents()
		{
			temperatureButton.Enabled = false;
			humidityButton.Enabled = false;
			buttonLightCommand.Enabled = false;
		}

		private void SetUIComponentsHandlers()
		{
	

			buttonLightCommand.Click += ButtonSendCommand_Click;
			temperatureButton.Click += GetTemperature_Click;
			humidityButton.Click += GetHumidity_Click;
			colorDialogButton.Click += ColorDialogButton_Click;
			Action<int> act = OnColorSelected;
			colorDialog = new ColorPickerDialog(this, 0, new ColorSelectedListener(act));
			switchBT.CheckedChange += SwitchBT_CheckedChange;
			switchBT.Click += SwitchBT_Click;
			fanButton.Click += FanButton_Click;

		}




		#endregion

		#region Handlers
		private void SwitchBT_Click(object sender, EventArgs e)
		{
			var sw = sender as Switch;

			if (sw.Checked)
			{
				ButtonConnect_Click(sender, e);

			}
			else
			{
				ButtonDisconnect_Click(sender, e);

			}
			progressBar.Visibility = ViewStates.Invisible;


		}

		private void SwitchBT_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			progressBar.Visibility = ViewStates.Visible;
		}

		private void GetHumidity_Click(object sender, EventArgs e)
		{
			CheckHumidity();
		}

		private void CheckHumidity()
		{
			communicationService.Write(new byte[3] { (byte)CommandCode.StartMarker, (byte)CommandCode.ReadHumidity, (byte)CommandCode.EndMarker });
		}

		private void GetTemperature_Click(object sender, EventArgs e)
		{
			CheckTemperature();
		}

		private void CheckTemperature()
		{
			communicationService.Write(new byte[3] { (byte)CommandCode.StartMarker, (byte)CommandCode.ReadTemperature, (byte)CommandCode.EndMarker });
		}

		private void ButtonConnect_Click(object sender, EventArgs e)
		{


			if (communicationService.Connect())
			{
			
				temperatureButton.Enabled = true;
				humidityButton.Enabled = true;
				buttonLightCommand.Enabled = true;
				CheckTemperature();
				CheckHumidity();
			}
		}

		private void ButtonSendCommand_Click(object sender, System.EventArgs e)
		{
				if(isSendClicked == false)
				{
					
					List<byte> message = new List<byte>() { (byte)CommandCode.StartMarker };

					message.Add((byte)CommandCode.LightOn);
					message.Add(this.color.R);
					message.Add(this.color.G);
					message.Add(this.color.B);
					message.Add((byte)CommandCode.EndMarker);

					communicationService.Write(message.ToArray());

					isSendClicked = true;
					buttonLightText.Text = "OFF.";
				}
				else
				{
					communicationService.Write(new byte[3] { (byte)CommandCode.StartMarker, (byte)CommandCode.LightOff, (byte)CommandCode.EndMarker });

					isSendClicked = false;
					buttonLightText.Text = "ON";
				}
			buttonLightCommand.SetBackgroundColor(BackgroundConverter.GetFromBool(isSendClicked));
		}

		private void ButtonDisconnect_Click(object sender, System.EventArgs e)
		{
			temperatureButton.Enabled = false;
			humidityButton.Enabled = false;
			buttonLightCommand.Enabled = false;
			communicationService.Disconnect();
		}

		private void ColorDialogButton_Click(object sender, EventArgs e)
		{
			colorDialog.Show();
		}
		private void FanButton_Click(object sender, EventArgs e)
		{
			if(isFanOn)
			{
				communicationService.Write(new byte[3] { (byte)CommandCode.StartMarker, (byte)CommandCode.FanOff, (byte)CommandCode.EndMarker });
				isFanOn = false;
			}
			else
			{
				communicationService.Write(new byte[3] { (byte)CommandCode.StartMarker, (byte)CommandCode.FanOn, (byte)CommandCode.EndMarker });
				isFanOn = true;
			}
			fanButton.SetBackgroundColor(BackgroundConverter.GetFromBool(isFanOn));
		}
		#endregion

		public void OnColorSelected(int color)
		{
			this.color = new Color(color);
			colorDialogButton.SetBackgroundColor(this.color);

			//buttonLightText.SetTextColor(color);

			List<byte> message = new List<byte>() { (byte)CommandCode.StartMarker };
			message.Add(this.color.R);
			message.Add(this.color.G);
			message.Add(this.color.B);
			message.Add((byte)CommandCode.EndMarker);

			communicationService.Write(message.ToArray());

		}
	}

}