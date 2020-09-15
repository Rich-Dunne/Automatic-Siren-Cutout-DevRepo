using System.Reflection;
using Rage;

[assembly: Rage.Attributes.Plugin("Automatic Siren Cutout", Author = "Rich", Description = "Automatically turn off your siren when you exit an emergency vehicle, as well as other quality of life features.")]

namespace AutomaticSirenCutout
{
    internal class EntryPoint
    {
        internal static void Main()
        {
            Settings.LoadSettings();
            GetAssemblyVersion();

            if (Settings.EnableASC)
            {
                Game.LogTrivial("AutomaticSirenCutout is enabled.");
                GameFiber ASCFiber = new GameFiber(() => AutomaticSirenCutout.ASC());
                ASCFiber.Start();
            }
            else
            {
                Game.LogTrivial("AutomaticSirenCutout is disabled.");
            }

            if (Settings.EnableTrafficLightControl)
            {
                Game.LogTrivial("TrafficLightControl is enabled.");
                GameFiber TrafficLightFiber = new GameFiber(() => TrafficLightControl.TLC());
                TrafficLightFiber.Start();
            }
            else
            {
                Game.LogTrivial("TrafficLightControl is disabled.");
            }

            if (Settings.EnableFriendlyHonk)
            {
                Game.LogTrivial("FriendlyHonk is enabled.");
                GameFiber FriendlyHonkFiber = new GameFiber(() => FriendlyHonk.Honk());
                FriendlyHonkFiber.Start();
            }
            else
            {
                Game.LogTrivial("FriendlyHonk is disabled.");
            }

            if(Settings.EnableYielding)
            {
                Game.LogTrivial("Yielding is enabled.");
                GameFiber YieldFiber = new GameFiber(() => Yield.YieldMain());
                YieldFiber.Start();
            }
            else
            {
                Game.LogTrivial("Yielding is disabled.");
            }
        }

        private static void GetAssemblyVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            Game.LogTrivial($"Automatic Siren Cutout V{version} is ready.");
        }
    }
}

/*
 *  if (Game.LocalPlayer.IsFreeAimingAtAnyEntity)
    {
        Game.LogTrivial("Entity hash: " + Game.LocalPlayer.GetFreeAimingTarget().Model.Hash.ToString());
        Game.LogTrivial("Entity heading: " + Game.LocalPlayer.GetFreeAimingTarget().Heading.ToString());
        Game.LogTrivial("Player heading: " + Game.LocalPlayer.Character.Heading.ToString());
    }
*/
