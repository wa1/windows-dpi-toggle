using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WindowsDpiToggle
{
  class Program
  {
    public enum DMDO
    {
      DEFAULT = 0,
      D90 = 1,
      D180 = 2,
      D270 = 3
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct DEVMODE
    {
      public const int DM_PELSWIDTH = 0x80000;
      public const int DM_PELSHEIGHT = 0x100000;
      private const int CCHDEVICENAME = 32;
      private const int CCHFORMNAME = 32;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
      public string dmDeviceName;
      public short dmSpecVersion;
      public short dmDriverVersion;
      public short dmSize;
      public short dmDriverExtra;
      public int dmFields;

      public int dmPositionX;
      public int dmPositionY;
      public DMDO dmDisplayOrientation;
      public int dmDisplayFixedOutput;

      public short dmColor;
      public short dmDuplex;
      public short dmYResolution;
      public short dmTTOption;
      public short dmCollate;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
      public string dmFormName;
      public short dmLogPixels;
      public int dmBitsPerPel;
      public int dmPelsWidth;
      public int dmPelsHeight;
      public int dmDisplayFlags;
      public int dmDisplayFrequency;
      public int dmICMMethod;
      public int dmICMIntent;
      public int dmMediaType;
      public int dmDitherType;
      public int dmReserved1;
      public int dmReserved2;
      public int dmPanningWidth;
      public int dmPanningHeight;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int ChangeDisplaySettings([In] ref DEVMODE lpDevMode, int dwFlags);

    static void Main(string[] args)
    {
      ToggleDPI(); // 100%
    }

    static void ToggleDPI()
    {
      RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop\PerMonitorSettings", true);

      var monitors = key.GetSubKeyNames();

      if (monitors.Length == 1)
      {
        key = key.OpenSubKey(monitors[0], true);
      }
      else if (monitors.Length > 1)
      {
        for (var i = 0; i < monitors.Length; i++)
        {
          Console.WriteLine($"{i}: {monitors[i]}");
        }

        Console.Write("Choose monitor to toggle DPI on (eg. 0, 1..): ");
        var input = Console.ReadLine();
        int index = Int32.Parse(input);

        Console.WriteLine("Monitor chosen: " + monitors[index]);
        var monitorName = monitors[index];

        key = key.OpenSubKey(monitorName, true);
      }
      else return;

      var dpiValue = key.GetValue("DpiValue");
      var dpi = (int)dpiValue;
      var newDpi = dpi == 0 ? 4 : 0;
      key.SetValue("DpiValue", newDpi, RegistryValueKind.DWord);

      // Switch resolution to trigger scaling change
      SetResolution(1920, 1080);
      SetResolution(2560, 1440);
    }

    private static void SetResolution(int w, int h)
    {
      long RetVal = 0;

      DEVMODE dm = new DEVMODE();

      dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

      dm.dmPelsWidth = w;
      dm.dmPelsHeight = h;

      dm.dmFields = DEVMODE.DM_PELSWIDTH | DEVMODE.DM_PELSHEIGHT;

      RetVal = ChangeDisplaySettings(ref dm, 0);
    }
  }
}
