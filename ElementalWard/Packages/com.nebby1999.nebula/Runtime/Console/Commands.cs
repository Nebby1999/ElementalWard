using UnityEngine;

namespace Nebula.Console
{
    internal static class Commands
    {
        [ConsoleCommand("timescale", "Sets the timescale. Arg0=\"float, new time scale\"")]
        private static void CCTimeScale(ConsoleCommandArgs args)
        {
            args.CheckArgCount(1);
            float scale = args.GetArgFloat(0);
            var old = Time.timeScale;
            Time.timeScale = scale;
            Debug.Log($"Timescale changed from {old} to {Time.timeScale}");
        }
    }
}