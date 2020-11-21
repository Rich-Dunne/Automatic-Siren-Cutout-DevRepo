using System.Reflection;
using Rage;
using LSPD_First_Response.Mod.API;
using System.IO;
using System.Net;

[assembly: Rage.Attributes.Plugin("Automatic Siren Cutout", Author = "Rich", Description = "Automatically turn off your siren when you exit an emergency vehicle, as well as other quality of life features.", PrefersSingleInstance = true)]

namespace AutomaticSirenCutout
{
    internal class Main : Plugin
    {
        public override void Initialize()
        {
            Settings.LoadSettings();
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
        }

        private void OnOnDutyStateChangedHandler(bool onDuty)
        {
            if (onDuty)
            {
                GetAssemblyVersion();

                if (Settings.EnableASC)
                {
                    if (InputManagerChecker())
                    {
                        Game.LogTrivial("[AutomaticSirenCutout]: AutomaticSirenCutout is enabled.");
                        GameFiber ASCFiber = new GameFiber(() => AutomaticSirenCutout.ASC());
                        ASCFiber.Start();
                    }
                    else
                    {
                        Game.LogTrivial("[AutomaticSirenCutout]: AutomaticSirenCutout is disabled due to an error.");
                    }
                }
                else
                {
                    Game.LogTrivial("[AutomaticSirenCutout]: AutomaticSirenCutout is disabled.");
                }

                if (Settings.EnableTrafficLightControl)
                {
                    Game.LogTrivial("[AutomaticSirenCutout]: TrafficLightControl is enabled.");
                    GameFiber TrafficLightFiber = new GameFiber(() => TrafficLightControl.TLC());
                    TrafficLightFiber.Start();
                }
                else
                {
                    Game.LogTrivial("[AutomaticSirenCutout]: TrafficLightControl is disabled.");
                }

                if (Settings.EnableFriendlyHonk)
                {
                    Game.LogTrivial("[AutomaticSirenCutout]: FriendlyHonk is enabled.");
                    GameFiber FriendlyHonkFiber = new GameFiber(() => FriendlyHonk.Honk());
                    FriendlyHonkFiber.Start();
                }
                else
                {
                    Game.LogTrivial("[AutomaticSirenCutout]: FriendlyHonk is disabled.");
                }

                if (Settings.EnableYielding)
                {
                    Game.LogTrivial("[AutomaticSirenCutout]: Yielding is enabled.");
                    GameFiber YieldFiber = new GameFiber(() => Yield.YieldMain());
                    YieldFiber.Start();
                }
                else
                {
                    Game.LogTrivial("[AutomaticSirenCutout]: Yielding is disabled.");
                }
            }
        }

        public override void Finally()
        {
            Game.LogTrivial($"[AutomaticSirenCutout]: Plugin is cleaned up.");
        }

        private static void GetAssemblyVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Game.LogTrivial($"[AutomaticSirenCutout]: Automatic Siren Cutout V{version} is ready.");
        }

        private static bool InputManagerChecker()
        {
            var directory = Directory.GetCurrentDirectory();
            var exists = File.Exists(directory + @"\InputManager.dll");
            if (!exists)
            {
                Game.LogTrivial($"[AutomaticSirenCutout]: InputManager was not found in the user's GTA V directory.");
                Game.DisplayNotification($"~o~Automatic Siren Cutout ~r~[Error]\n~w~InputManager.dll was not found in your GTA V directory.  Please install InputManager.dll and try again.");
                return false;
            }
            return true;
        }
    }
}
