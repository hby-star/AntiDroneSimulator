using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


/// <summary>
/// Unity Warpper for KAT Native SDK
/// </summary>
public class KATNativeSDK
{
    const int packAlign = 1;

	public const string sdk_warpper_lib = "KATSDKWarpper";
    public const string nexus_client_lib = "NexusClient";

	public static NativeFunctionLoader sdkLoader ;

	/// <summary>
	/// Description of KAT Devices
	/// </summary>
	/// 
	[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
	public struct DeviceDescription
	{
		//Device Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string device;

		//Device Serial Number
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string serialNumber;

		//Device PID
		public int pid;

		//Device VID
		public int vid;

		//Device Type
		//0. Err 1. Tread Mill 2. Tracker 
		public int deviceType;
	};

	/// <summary>
	/// Device Status Data
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DeviceData
	{
		//Is Calibration Button Pressed?
		[MarshalAs(UnmanagedType.I1)]
		public bool btnPressed;
		[MarshalAs(UnmanagedType.I1)]
		//Is Battery Charging?
		public bool isBatteryCharging;
		//Battery Used
		public float batteryLevel;
		[MarshalAs(UnmanagedType.I1)]
		public byte firmwareVersion;
	};

	/// <summary>
	/// TreadMill Device Data
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
	public struct TreadMillData
	{
		//Device Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string deviceName;
		[MarshalAs(UnmanagedType.I1)]
		//Is Device Connected
		public bool connected;
		//Last Update Time
		public double lastUpdateTimePoint;

		//Body Rotation(Quaternion), for treadmill it will cause GL
		public Quaternion bodyRotationRaw;

		//Target Move Speed With Direction
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public Vector3 moveSpeed;

		//Sensor Device Datas
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public DeviceData[] deviceDatas;

		//Extra Data of TreadMill
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		public byte[] extraData;
	};


	delegate int device_count_def();
	public static int DeviceCount()
	{
		return sdkLoader.Invoke<device_count_def>("DeviceCount")();
	}

	delegate DeviceDescription get_device_desc_def(uint index);
	public static DeviceDescription GetDevicesDesc(uint index)
	{
		return sdkLoader.Invoke<get_device_desc_def>("GetDevicesDesc")(index);
	}

	delegate double get_last_calibrated_time_escaped_def();
	public static double GetLastCalibratedTimeEscaped()
	{
		return sdkLoader.Invoke<get_last_calibrated_time_escaped_def>("GetLastCalibratedTimeEscaped")();
	}

	delegate TreadMillData get_walk_status_def(string sn);
	public static TreadMillData GetWalkStatus(string sn = "")
	{
		return sdkLoader.Invoke<get_walk_status_def>("GetWalkStatus")(sn);
	}

	delegate void unload_sdk_library_def();
	public static void UnloadSDKLibrary()
	{
		sdkLoader.Invoke<unload_sdk_library_def>("UnloadSDKLibrary")();
        sdkLoader.Release();
    }

    //KAT Extensions, Only for WalkCoord2 and later device
    public class KATExtension
	{
		//KAT Extensions, amplitude: 0(close) - 1.0(max)
		delegate void vibrate_const_def(float amplitude);

		public static void VibrateConst(float amplitude)
		{
			sdkLoader.Invoke<vibrate_const_def>("VibrateConst")(amplitude);
		}

		delegate void LEDConst_def(float amplitude);
		public static  void LEDConst(float amplitude)
		{
			sdkLoader.Invoke<LEDConst_def>("LEDConst")(amplitude);
		}

		//Vibrate in duration
		delegate void vibrate_in_seconds_def(float amplitude, float duration);
		public static void VibrateInSeconds(float amplitude, float duration)
		{
			sdkLoader.Invoke<vibrate_in_seconds_def>("VibrateInSeconds")(amplitude, duration);
		}

		//Vibrate once, simulate a "Click" like function
		delegate void vibrate_once_def(float amplitude);
		public static void VibrateOnce(float amplitude)
		{
            sdkLoader.Invoke<vibrate_once_def>("VibrateOnce")(amplitude);
        }
		

		//Vibrate with a frequency in duration
		delegate void vibrate_for_def(float duration, float frequency, float amplitude);
		public static void VibrateFor(float duration, float frequency, float amplitude)
		{
            sdkLoader.Invoke<vibrate_for_def>("VibrateFor")(duration, frequency, amplitude);
        }

		//Lighting LED in Seconds
		delegate void LED_in_seconds_def(float amplitude, float duration);
		public static void LEDInSeconds(float amplitude, float duration)
		{
            sdkLoader.Invoke<LED_in_seconds_def>("LEDInSeconds")(amplitude, duration);
        }

		//Lighting once
		delegate void LED_once_def(float amplitude);
		public static void LEDOnce(float amplitude)
		{
            sdkLoader.Invoke<LED_once_def>("LEDOnce")(amplitude);
        }


		//Vibrate with a frequency in duration
		delegate void LED_for_def(float duration, float frequency, float amplitude);
		public static void LEDFor(float duration, float frequency, float amplitude)
		{
            sdkLoader.Invoke<LED_for_def>("LEDFor")(duration, frequency, amplitude);
        }
		

	}
}
