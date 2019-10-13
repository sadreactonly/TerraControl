namespace TerraControl
{
	public enum CommandCode : byte
	{
		StartMarker = 0x3C,
		EndMarker = 0x3E,
		ReadTemperature = 0x11,
		ReadHumidity = 0x12,
		LightOn	= 0x13,
		LightOff = 0x14

	}
}