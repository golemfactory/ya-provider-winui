using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GolemUI
{
    public class ComputeDevice
    {
        public string Name { get; set; }
        public long Memory { get; set; }
        public string Vendor { get; set; }

        public ComputeDevice()
        {
            Name = "";
            Memory = 0;
            Vendor = "";
        }
    };

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonOpenCLDevice
    {
        [JsonProperty("_CL_DEVICE_NAME")]
        public string? ClDeviceName { get; set; }

        [JsonProperty("BUS_ID")]
        public string? BusID { get; set; }

        [JsonProperty("DeviceID")]
        public string? DeviceID { get; set; }

        [JsonProperty("_CL_DEVICE_BOARD_NAME_AMD")]
        public string? ClDeviceNameAmd { get; set; }

        [JsonProperty("_CL_DEVICE_GLOBAL_MEM_SIZE")]
        public long? ClGlobalMemSize { get; set; }

        [JsonProperty("_CL_DEVICE_TYPE")]
        public string? ClDeviceType { get; set; }

        [JsonProperty("_CL_DEVICE_VENDOR")]
        public string? ClDeviceVendor { get; set; }

        [JsonProperty("_CL_DEVICE_VERSION")]
        public string? ClDeviceVersion { get; set; }

        [JsonProperty("_CL_DRIVER_VERSION")]
        public string? ClDriverVersion { get; set; }

    };

    public class JsonCudaDevice
    {
        [JsonProperty("DeviceName")]
        public string? DeviceName { get; set; }

        [JsonProperty("DeviceGlobalMemory")]
        public long? DeviceGlobalMemory { get; set; }

        [JsonProperty("DeviceID")]
        public int? DeviceID { get; set; }

        [JsonProperty("HasMonitorConnected")]
        public int? HasMonitorConnected { get; set; }

        [JsonProperty("SMX")]
        public int? SMX { get; set; }

        [JsonProperty("SM_major")]
        public int? SM_major { get; set; }

        [JsonProperty("SM_minor")]
        public int? SM_minor { get; set; }

        [JsonProperty("UUID")]
        public string? UUID { get; set; }

        [JsonProperty("VendorID")]
        public string? VendorID { get; set; }

        [JsonProperty("VendorName")]
        public string? VendorName { get; set; }

        [JsonProperty("PCIBusID")]
        public string? BusID { get; set; }

        [JsonProperty("pciDeviceId")]
        public string? pciDeviceId { get; set; }

        [JsonProperty("pciSubSystemId")]
        public string? pciSubSystemId { get; set; }
    };

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonPlatform
    {
        [JsonProperty("Devices")]
        public List<JsonOpenCLDevice>? Devices { get; set; }

        [JsonProperty("PlatformName")]
        public string? PlatformName { get; set; }

        [JsonProperty("PlatformNum")]
        public int? PlatformNum { get; set; }

        [JsonProperty("PlatformVendor")]
        public string? PlatformVendor { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonGpuInfo
    {
        [JsonProperty("platforms")]
        public List<JsonPlatform>? Platforms { get; set; }

        [JsonProperty("CudaDevices")]
        public List<JsonCudaDevice>? CudaDevices { get; set; }
    };

    class GpuInfoCommand
    {
        string _deviceDetectionPath = "device_detection.exe";

        public List<ComputeDevice> GetGpuInfo(bool hideNvidiaOpenCLDevices)
        {
            List<JsonGpuInfo>? infos = Exec<List<JsonGpuInfo>>("all");

            List<ComputeDevice> computeDevices = new List<ComputeDevice>();

            if (infos == null)
            {
                return computeDevices;
            }
            foreach (var info in infos)
            {
                if (info.CudaDevices != null)
                {
                    foreach (var cudaDevice in info.CudaDevices)
                    {
                        var newDevice = new ComputeDevice();
                        newDevice.Name = cudaDevice.DeviceName ?? "";
                        newDevice.Memory = cudaDevice.DeviceGlobalMemory ?? 0;
                        newDevice.Vendor = "nVidia";
                        computeDevices.Add(newDevice);
                    }
                }
                if (info.Platforms != null)
                {
                    foreach (var platform in info.Platforms)
                    {
                        if (platform.Devices != null)
                        {
                            bool isNvidia = false;
                            if (platform.PlatformName != null && platform.PlatformName.ToLower().Contains("nvidia"))
                            {
                                isNvidia = true;
                            }
                            if (platform.PlatformVendor != null && platform.PlatformVendor.ToLower().Contains("nvidia"))
                            {
                                isNvidia = true;
                            }

                            bool isIntel = false;
                            if (platform.PlatformName != null && platform.PlatformName.ToLower().Contains("intel"))
                            {
                                isIntel = true;
                            }
                            if (platform.PlatformVendor != null && platform.PlatformVendor.ToLower().Contains("intel"))
                            {
                                isIntel = true;
                            }

                            bool isAmd = false;
                            if (platform.PlatformName != null && (platform.PlatformName.ToLower().Contains("amd") || platform.PlatformName.ToLower().Contains("radeon")))
                            {
                                isAmd = true;
                            }
                            if (platform.PlatformVendor != null && (platform.PlatformVendor.ToLower().Contains("amd") || platform.PlatformVendor.ToLower().Contains("radeon")))
                            {
                                isAmd = true;
                            }

                            //mark device as unknown if both guesses hit
                            if ((isIntel && isNvidia) || (isAmd && isNvidia) || (isAmd && isIntel))
                            {
                                isIntel = false;
                                isNvidia = false;
                                isAmd = false;
                            }

                            if (hideNvidiaOpenCLDevices && isNvidia)
                            {
                                continue;
                            }

                            foreach (var openClDevice in platform.Devices)
                            {
                                var newDevice = new ComputeDevice();
                                newDevice.Name = openClDevice.ClDeviceName ?? "";
                                newDevice.Memory = openClDevice.ClGlobalMemSize ?? 0;
                                if (isNvidia)
                                {
                                    newDevice.Vendor = "nVidia";
                                }
                                else if (isAmd)
                                {
                                    newDevice.Vendor = "AMD";
                                }
                                else if (isIntel)
                                {
                                    newDevice.Vendor = "Intel";
                                }
                                else
                                {
                                    newDevice.Vendor = "Unknown";
                                }
                                computeDevices.Add(newDevice);
                            }
                        }
                    }
                }
            }
            return computeDevices;
        }


        private T? Exec<T>(string arguments) where T : class
        {
            var text = this.ExecToText(arguments);
            return JsonConvert.DeserializeObject<T>(text);
        }

        private string ExecToText(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = this._deviceDetectionPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

    }
}
