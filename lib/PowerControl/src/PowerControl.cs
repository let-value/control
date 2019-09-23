using System;

namespace PowerControl
{
    public static class PowerControl
    {
        public static void Sleep()
        {
            if (OperatingSystem.IsWindows())
                Command.Exec("rundll32.exe powrprof.dll,SetSuspendState 0,1,0");
            else if (OperatingSystem.IsLinux())
                Command.Exec("systemctl hybrid-sleep");
            else if(OperatingSystem.IsMacOS())
                Command.Exec("pmset sleepnow");
            else
                throw new PlatformNotSupportedException();
        }

        public static void Shutdown()
        {
            if (OperatingSystem.IsWindows())
                Command.Exec("shutdown -s -t 0");
            else if (OperatingSystem.IsLinux())
                Command.Exec("systemctl poweroff");
            else
                throw new PlatformNotSupportedException();
        }

        public static void Reboot()
        {
            if (OperatingSystem.IsLinux())
                Command.Exec("systemctl reboot");
            else
                throw new PlatformNotSupportedException();
        }
    }
}
