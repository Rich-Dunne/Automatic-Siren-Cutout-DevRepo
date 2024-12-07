using System.Reflection;
using Rage;
using LSPD_First_Response.Mod.API;
using System.IO;
using AutomaticSirenCutout.Features;

[assembly: Rage.Attributes.Plugin("Automatic Siren Cutout", Author = "Rich", Description = "Automatically turn off your siren when you exit an emergency vehicle, as well as other quality of life features.", PrefersSingleInstance = true)]

namespace AutomaticSirenCutout
{
    internal class Main : Plugin
    {
        public override void Initialize()
        {
            Settings.Prepare();
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
        }

        private void OnOnDutyStateChangedHandler(bool onDuty)
        {
            if (onDuty)
            {
                LoadFeatures();
                GetAssemblyVersion();
            }
        }

        public override void Finally()
        {
            Game.LogTrivial($"[AutomaticSirenCutout]: Plugin is cleaned up.");
        }

        private static void LoadFeatures()
        {
            if (Settings.EnableASC)
            {
                Game.LogTrivial("[AutomaticSirenCutout]: AutomaticSirenCutout is enabled.");
                GameFiber.StartNew(() => Features.AutomaticSirenCutout.Main(), "AutomaticSirenCutout Fiber");
            }
            else
            {
                Game.LogTrivial("[AutomaticSirenCutout]: AutomaticSirenCutout is disabled.");
            }

            if (Settings.EnableTrafficLightControl)
            {
                Game.LogTrivial("[AutomaticSirenCutout]: TrafficLightControl is enabled.");
                GameFiber.StartNew(() => TrafficLightControl.Main(), "Traffic Light Control Fiber");
            }
            else
            {
                Game.LogTrivial("[AutomaticSirenCutout]: TrafficLightControl is disabled.");
            }

            if (Settings.EnableFriendlyHonk)
            {
                Game.LogTrivial("[AutomaticSirenCutout]: FriendlyHonk is enabled.");
                GameFiber.StartNew(() => FriendlyHonk.Main(), "Friendly Honk Fiber");
            }
            else
            {
                Game.LogTrivial("[AutomaticSirenCutout]: FriendlyHonk is disabled.");
            }

            if (Settings.EnableYielding)
            {
                Game.LogTrivial("[AutomaticSirenCutout]: Yielding is enabled.");
                GameFiber.StartNew(() => Yield.YieldMain(), "Yield Fiber");
            }
            else
            {
                Game.LogTrivial("[AutomaticSirenCutout]: Yielding is disabled.");
            }
        }

        private static void GetAssemblyVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Game.LogTrivial($"[AutomaticSirenCutout]: Automatic Siren Cutout V{version} is ready.");
        }
    }
}
