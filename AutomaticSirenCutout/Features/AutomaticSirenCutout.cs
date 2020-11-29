using InputManager;
using Rage;

namespace AutomaticSirenCutout.Features
{
    internal static class AutomaticSirenCutout
    {
        internal static void ASC()
        {
            while (true)
            {
                if (Game.LocalPlayer.Character.IsAlive && Game.LocalPlayer.Character.IsInAnyVehicle(false) && Game.LocalPlayer.Character.CurrentVehicle && Game.LocalPlayer.Character.CurrentVehicle.IsSirenOn && (Game.IsKeyDown(Settings.KeyboardExitVehicle) || Game.IsControllerButtonDown(Settings.ControllerExitVehicle)))
                {
                    Game.LogTrivialDebug("[Automatic Siren Cutout]: Player is exiting vehicle with stage 3 on, resetting stage 3.");
                    for (int i = 0; i < 4; i++)
                    {
                        Keyboard.KeyDown(Settings.KeyboardELSLights);
                        GameFiber.Wait(1);
                        Keyboard.KeyUp(Settings.KeyboardELSLights);
                        GameFiber.Wait(1);
                    }
                }
                GameFiber.Yield();
            }
        }
    }
}
