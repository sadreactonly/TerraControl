using Android.Graphics;

namespace TerraControl
{
	public static class BackgroundConverter
	{
		public static Color GetFromTemperature(int temp)
		{
			if (temp < 10 || temp>=27)
			{
				return Color.Red;
			}
			else if ((temp >10 && temp<15)||(temp>22&&temp<27))
			{
				return Color.LightGreen;
			}
			else if(temp>=15 && temp<=22)
			{
				return Color.Green;
			}
			else
			{
				return Color.OrangeRed;
			}
			
		}

		public static Color GetFromHumidity(int temp)
		{
			if (temp < 50 || temp > 90)
			{
				return Color.Red;
			}
			else if ((temp > 70 && temp < 80))
			{
				return Color.LightGreen;
			}
			else if (temp >=80 && temp <= 90)
			{
				return Color.Green;
			}
			else
			{
				return Color.OrangeRed;
			}

		}

		public static Color GetFromBool(bool command)
		{
			if (command)
				return Color.Green;
			else
				return Color.Red;
		}
	}
}
