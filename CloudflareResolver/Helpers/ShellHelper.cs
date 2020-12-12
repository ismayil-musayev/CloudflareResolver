using System.Diagnostics;
using System.Threading.Tasks;

namespace CloudflareResolver.Helpers
{
    public static class ShellHelper
    {
        public static async Task Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var psi = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{escapedArgs}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            await p.WaitForExitAsync().ConfigureAwait(false);
        }
    }
}
