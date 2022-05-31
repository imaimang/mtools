// See https://aka.ms/new-console-template for more information
using MTools.Http;
using System.Security.Cryptography;
using System.Text;

Console.WriteLine("Hello, World!");


HttpProxy httpProxy = new HttpProxy("http://localhost:5000");

Device device = new Device();
device.Name = "SWK";
var result = await httpProxy.PostMessage<Device>("/device/adddevice", device);
Console.WriteLine("是否成功:{0} 返回值:{1}", result.IsSuccess, result.Content);

var result1 = await httpProxy.GetMessage<string>("/device/getdevices", null);
Console.WriteLine("是否成功:{0} 返回值:{1}", result1.IsSuccess, result1.Content);






public class Device
{
    public int DeviceID { get; set; }

    public string Name { get; set; } = "";

    public string Config { get; set; } = "";
}