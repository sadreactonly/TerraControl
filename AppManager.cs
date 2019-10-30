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
	public class GUIManager
	{
		private static GUIManager instance = null;
		private static readonly object padlock = new object();

		GUIManager()
		{
		}

		public static GUIManager Instance
		{
			get
			{
				if (instance == null)
				{
					lock (padlock)
					{
						if (instance == null)
						{
							instance = new GUIManager();
						}
					}
				}
				return instance;
			}
		}

	

	}
}
