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

namespace TerraControl
{
	public class Message
	{
		private byte[] messageParams;
		public byte[] RawBytes { get; set; } = default;

		public Message(params byte[] message)
		{
			this.messageParams = message;
		}

		public void Create()
		{
			int messageLenght = messageParams.Length + 2;
			RawBytes = new byte[messageLenght]; //start and end marker
			RawBytes[0] = (byte)CommandCode.StartMarker;

			for(int i = 0;i<messageParams.Length;i++)
			{
				RawBytes[i + 1] = messageParams[i];
			}
			RawBytes[messageLenght-1] = (byte)CommandCode.EndMarker;

		}

	}
}