// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Windows.Forms;

namespace WandHandler
{
	public partial class WandReceiver : Form
	{
		static List<USBDeviceInfo> GetUSBDevices()
		{
			List<USBDeviceInfo> Devices = new List<USBDeviceInfo>();

			ManagementObjectCollection Collection;
			using( ManagementObjectSearcher Searcher = new ManagementObjectSearcher( "Select * From Win32_USBHub" ) )
			{
				Collection = Searcher.Get();
			}

			foreach( ManagementObject Device in Collection )
			{
				Devices.Add( new USBDeviceInfo( ( string )Device.GetPropertyValue( "DeviceID" ), ( string )Device.GetPropertyValue( "PNPDeviceID" ), ( string )Device.GetPropertyValue( "Description" ) ) );
			}

			Collection.Dispose();
			return Devices;
		}

		public WandReceiver()
		{
			InitializeComponent();

			List<USBDeviceInfo> USBDevices = GetUSBDevices();

			foreach( USBDeviceInfo USBDevice in USBDevices )
			{
				Console.WriteLine( "Device ID: {0}, PNP Device ID: {1}, Description: {2}", USBDevice.DeviceID, USBDevice.PnpDeviceID, USBDevice.Description );
			}
		}
	}

	public class USBDeviceInfo
	{
		public USBDeviceInfo( string deviceID, string pnpDeviceID, string description )
		{
			this.DeviceID = deviceID;
			this.PnpDeviceID = pnpDeviceID;
			this.Description = description;
		}

		public string DeviceID { get; private set; }
		public string PnpDeviceID { get; private set; }
		public string Description { get; private set; }
	}
}
