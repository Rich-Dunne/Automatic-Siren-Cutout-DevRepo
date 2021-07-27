using Rage;

namespace AutomaticSirenCutout.Features
{
    internal static class LeaveEngineRunning
    {
        internal static void Main()
        {
            while (true)
            {
                GameFiber.Yield();

                if (Game.LocalPlayer.Character.LastVehicle?.Driver && Game.LocalPlayer.Character.LastVehicle?.Driver == Game.LocalPlayer.Character && (Game.IsKeyDown(Settings.KeyboardExitVehicle) || Game.IsControllerButtonDown(Settings.ControllerExitVehicle)))
                {
                    GameFiber.Sleep(100);
                    if (Game.LocalPlayer.Character.LastVehicle && (Game.IsKeyDownRightNow(Settings.KeyboardExitVehicle) || Game.IsControllerButtonDownRightNow(Settings.ControllerExitVehicle)))
                    {
                        Game.LocalPlayer.Character.LastVehicle.IsEngineOn = true;
                    }
                }
            }
        }
    }
}
