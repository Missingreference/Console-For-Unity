using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Rendering;

namespace Elanetic.Console.Unity.Commands
{
    public class SystemInfoCommand : UnityCommand
    {
        public override string name => "systeminfo";

        public override string helpMessage => "Print all system information to the console including device information, hardware capabilities, permissions and build settings.";

        string info = "";

        public override void UnityExecute(params string[] args)
        {
            info += "-SYSTEM INFORMATION-\n";


            Section("Platform");

            Entry("Operating System: " + SystemInfo.operatingSystem);
            Entry("Is 64-Bit Process: " + Environment.Is64BitProcess.ToString());
            Entry("System Local Time: " + System.DateTime.Now.ToString("h:mm tt | dd MMMM, yyyy"));
            Entry("Device Name: " + SystemInfo.deviceName);
            Entry("Device Type: " + SystemInfo.deviceType.ToString());
            Entry("Device Model: " + SystemInfo.deviceModel);
            Entry("Graphics API: " + SystemInfo.graphicsDeviceType.ToString());
            Entry("Graphics API Version: " + SystemInfo.graphicsDeviceVersion);


            Section("System Hardware");

            Entry("Is 64-Bit System: " + Environment.Is64BitOperatingSystem.ToString());
            Entry("Device Unique Identifier: " + SystemInfo.deviceUniqueIdentifier);
            Entry("Processor: " + SystemInfo.processorType);
            Entry("Processor Count: " + SystemInfo.processorCount.ToString());
            Entry("Processor Frequency: " + SystemInfo.processorFrequency.ToString() + " MHz");
            Entry("System Memory: " + SystemInfo.systemMemorySize.ToString() + " MB");
            Entry("Used System Memory: " + Math.Floor(GC.GetTotalMemory(true) / 1048576.0).ToString() + " MB");
            Entry("Graphics Device ID: " + SystemInfo.graphicsDeviceID);
            Entry("Graphics Vendor: " + SystemInfo.graphicsDeviceVendor);
            Entry("Graphics Vendor ID: " + SystemInfo.graphicsDeviceVendorID);
            Entry("Graphics Memory: " + SystemInfo.graphicsMemorySize.ToString() + " MB");
            Entry("Audio Device Exists: " + SystemInfo.supportsAudio);
            Entry(GetLogicalDeviceInformation());
            if(SystemInfo.batteryLevel < 0)
            {
                Entry("Battery Level: N/A");
            }
            else
            {
                Entry("Battery Level: " + (SystemInfo.batteryLevel * 100.0f).ToString() + "%");
            }
            Entry("Battery Status: " + SystemInfo.batteryStatus.ToString());
            Entry("Supports Vibration: " + SystemInfo.supportsVibration.ToString());
            Entry("Supports Accelerometer: " + SystemInfo.supportsAccelerometer);
            Entry("Supports Gyroscope: " + SystemInfo.supportsGyroscope);
            Entry("Supports Location Services: " + SystemInfo.supportsLocationService);

            Section("Display Information");

            Entry("Current Resolution: " + Screen.currentResolution.width + "x" + Screen.currentResolution.height + " @ " + Screen.currentResolution.refreshRate + " Hz");
            Entry("Window Mode: " + Screen.fullScreenMode.ToString());
            Entry("Brightness: " + (Screen.brightness*100.0f).ToString() + "%");
            Entry("DPI: " + Screen.dpi.ToString());
            Entry("Orientation: " + Screen.orientation.ToString());
            Entry(GetDisplayInformation());


            Section("Graphics Support Information");

            Entry("Multi-threaded Rendering: " + SystemInfo.graphicsMultiThreaded);
            Entry("Rendering Threading Mode: " + SystemInfo.renderingThreadingMode.ToString());
            Entry("Shader Level: " + SystemInfo.graphicsShaderLevel);
            Entry("Supports Compute Shaders: " + SystemInfo.supportsComputeShaders);
            Entry("Supports Raytracing: " + SystemInfo.supportsRayTracing);
            Entry("HDR Support Flags: (" + SystemInfo.hdrDisplaySupportFlags.ToString() + ")");
            Entry("Copy Texture Support Flags: (" + SystemInfo.copyTextureSupport.ToString() + ")");
            Entry("Supports Draw Call Instancing: " + SystemInfo.supportsInstancing);
            Entry("Supports Asynchronous Compute Queues: " + SystemInfo.supportsAsyncCompute);
            Entry("Supports 2D Array Textures: " + SystemInfo.supports2DArrayTextures);
            Entry("Supports Geometry Shaders: " + SystemInfo.supportsGeometryShaders);
            Entry("Supports Graphics Fence: " + SystemInfo.supportsGraphicsFence);
            Entry("Max Graphics Buffer Size: " + Math.Round(((double)SystemInfo.maxGraphicsBufferSize) / 1073741824.0,2).ToString() + " GB");
            Entry("Max Texture Size: " + SystemInfo.maxTextureSize);
            Entry("Max Compute Groups Sizes: X: " + SystemInfo.maxComputeWorkGroupSizeX + " Y: " + SystemInfo.maxComputeWorkGroupSizeY + " Z: " + SystemInfo.maxComputeWorkGroupSizeZ + " Total: " + SystemInfo.maxComputeWorkGroupSize);



            Section("Unity Engine");

            Entry("Version: " + Application.unityVersion);
#if UNITY_EDITOR
            Entry("Is Editor: True");
#endif
            Entry("Pro Activated: " + Application.HasProLicense());
            Entry("Genuine: " + Application.genuine);
            Entry("Genuine Check Available: " + Application.genuineCheckAvailable);
            Entry("Build GUID: " + Application.buildGUID);
            Entry("Target Frame Rate: " + Application.targetFrameRate);
            Entry("Frame Count: " + Time.frameCount);
            Entry("Rendered Frame Count: " + Time.renderedFrameCount);
            Entry("Time Since Application Start: " + Math.Floor(Time.realtimeSinceStartupAsDouble / 60).ToString() + " Minutes");
            Entry("Time Since System Start: " + Math.Floor((((float)Environment.TickCount) / 1000.0f / 60.0f)).ToString() + " Minutes");

            Console.Log(info);
        }

        private void Section(string sectionName)
        {
            info += "\n[" + sectionName + "]\n";
        }

        private void Entry(string entryInformation)
        {
            info += entryInformation + "\n";
        }

        private string GetLogicalDeviceInformation()
        {
            //Get all drive info
            DriveInfo[] drives;
            try
            {
                drives = DriveInfo.GetDrives();
            }
            catch (UnauthorizedAccessException)
            {
                return "Logical Devices: { Does not have required permissions. }";
            }
            catch (IOException)
            {
                return "Logical Devices: { Failed to retrieve information. (IOException) }";
            }

            string output = "Logical Devices:\n";

            for (int i = 0; i < drives.Length; i++)
            {
                DriveInfo driveInfo = drives[i];

                output += " - Name: '" + driveInfo.Name + "' Type: " + driveInfo.DriveType.ToString() + "\n";
                output += "   Volume Label: ";
                try
                {
                    output += driveInfo.VolumeLabel;
                }
                catch (Exception ex)
                {
                    output += "{ Failed to retrieve information. (" + ex.GetType().Name + ") }";
                }
                output += "\n";

                try
                {
                    double freeSpace = Math.Floor(driveInfo.AvailableFreeSpace / 1073741824.0);
                    double totalSpace = Math.Floor(driveInfo.TotalSize / 1073741824.0);
                    output += "   Free Space: " + freeSpace.ToString() + " GB / Total Space: " + totalSpace.ToString() + " GB";
                }
                catch(Exception ex)
                {
                    output += "   Space: { Failed to retrieve information. (" + ex.GetType().Name + ") }";
                }
                output += "\n\n";
            }

            return output;

        }

        private string GetDisplayInformation()
        {
            List<DisplayInfo> displays = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displays);

            string output = "Available Displays: \n";

            for (int i = 0; i < displays.Count; i++)
            {
                DisplayInfo displayInfo = displays[i];

                output += "  [" + (i + 1).ToString() + "]   " + displayInfo.name + " " + displayInfo.width + "x" + displayInfo.height + " @ " + Math.Round(displayInfo.refreshRate.value) + " Hz\n";
            }

            return output;
        }
    }
}
