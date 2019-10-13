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

		Button buttonConnect;
		Button buttonDisconnect;
		Button buttonSendCommand;
		Button temperatureButton;
		Button humidityButton;
		Button colorDialogButton;

		TextView temperatureText;
		TextView humidityText;
		CommunicationService communicationService;

		ColorPickerDialog colorDialog;
		View colorView;

		Color color;

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
		private void SetUIComponents()
		{
			buttonDisconnect.Enabled = false;
			temperatureButton.Enabled = false;
			humidityButton.Enabled = false;
			buttonSendCommand.Enabled = false;
		}

		private void SetUIComponentsHandlers()
		{
			buttonConnect.Click += ButtonConnect_Click;
			buttonDisconnect.Click += ButtonDisconnect_Click;
			buttonSendCommand.Click += ButtonSendCommand_Click;
			temperatureButton.Click += GetTemperature_Click;
			humidityButton.Click += GetHumidity_Click;
			colorDialogButton.Click += ColorDialogButton_Click;
			Action<int> act = OnColorSelected;
			colorDialog = new ColorPickerDialog(this, 0, new ColorSelectedListener(act));

		}

		private void GetUIComponents()
		{
			buttonConnect = FindViewById<Button>(Resource.Id.button1);
			buttonDisconnect = FindViewById<Button>(Resource.Id.button2);
			buttonSendCommand = FindViewById<Button>(Resource.Id.button3);
			temperatureButton = FindViewById<Button>(Resource.Id.button4);
			humidityButton = FindViewById<Button>(Resource.Id.button5);
			colorDialogButton = FindViewById<Button>(Resource.Id.button6);
			temperatureText = FindViewById<TextView>(Resource.Id.textView1);
			humidityText = FindViewById<TextView>(Resource.Id.textView2);
			colorView = FindViewById<View>(Resource.Id.view1);
		}
		#endregion

		#region Handlers
		private void GetHumidity_Click(object sender, EventArgs e)
		{
			communicationService.Write(new byte[3] { (byte)CommandCode.StartMarker, (byte)CommandCode.ReadHumidity,(byte)CommandCode.EndMarker }); 
		}

		private void GetTemperature_Click(object sender, EventArgs e)
		{
			communicationService.Write(new byte[3] { (byte)CommandCode.StartMarker, (byte)CommandCode.ReadTemperature, (byte)CommandCode.EndMarker });
		}

		private void ButtonConnect_Click(object sender, EventArgs e)
		{
			if (communicationService.Connect())
			{
				buttonDisconnect.Enabled = true;
				buttonConnect.Enabled = false;
				temperatureButton.Enabled = true;
				humidityButton.Enabled = true;
				buttonSendCommand.Enabled = true;
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
					buttonSendCommand.Text = "Light off.";
				}
				else
				{
					communicationService.Write(new byte[3] { (byte)CommandCode.StartMarker, (byte)CommandCode.LightOff, (byte)CommandCode.EndMarker });

					isSendClicked = false;
					buttonSendCommand.Text = "Light on.";
				}
		}

		private void ButtonDisconnect_Click(object sender, System.EventArgs e)
		{
			buttonDisconnect.Enabled = false;
			temperatureButton.Enabled = false;
			humidityButton.Enabled = false;
			buttonSendCommand.Enabled = false;
			buttonConnect.Enabled = true;

			communicationService.Disconnect();
		}

		private void ColorDialogButton_Click(object sender, EventArgs e)
		{
			colorDialog.Show();
		}

		#endregion

		public void OnColorSelected(int color)
		{
			this.color = new Color(color);
			colorView.SetBackgroundColor(this.color);

			List<byte> message = new List<byte>() { (byte)CommandCode.StartMarker };
			message.Add(this.color.R);
			message.Add(this.color.G);
			message.Add(this.color.B);
			message.Add((byte)CommandCode.EndMarker);

			communicationService.Write(message.ToArray());

		}
	}

}