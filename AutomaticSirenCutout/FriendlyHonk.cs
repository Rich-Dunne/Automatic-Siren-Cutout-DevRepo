using Rage;
using System.Linq;

namespace AutomaticSirenCutout
{
    public class FriendlyHonk
    {
        public static void Honk()
        {
            while (true)
            {
                if (Game.LocalPlayer.Character.CurrentVehicle && Game.LocalPlayer.Character.CurrentVehicle.HasSiren)
                {
                    if (Game.IsKeyDown(Settings.KeyboardYelp) || Game.IsControllerButtonDown(Settings.ControllerYelp))
                    {
                        foreach (Vehicle v in World.GetAllVehicles().Where(v => v && v != Game.LocalPlayer.Character.CurrentVehicle && v.HasSiren && v.HasDriver && v.DistanceTo2D(Game.LocalPlayer.Character.Position) <= 30f && !v.IsSirenOn))
                        {
                            Game.LogTrivial("Player is yelping siren at another cop.");
                            GameFiber.Sleep(500);
                            v.BlipSiren(false);
                            break;
                        }
                    }
                    else if (Game.IsKeyDown(Settings.KeyboardHorn) || Game.IsControllerButtonDown(Settings.ControllerHorn))
                    {
                        foreach (Vehicle v in World.GetAllVehicles().Where(v => v && v != Game.LocalPlayer.Character.CurrentVehicle && v.HasSiren && v.HasDriver && v.DistanceTo2D(Game.LocalPlayer.Character.Position) <= 30f && !v.IsSirenOn))
                        {
                            Game.LogTrivial("Player is honking at another cop.");
                            GameFiber.Sleep(500);
                            Rage.Native.NativeFunction.Natives.START_VEHICLE_HORN(v, 10, "NORMAL", false);
                            break;
                        }
                    }
                }
                GameFiber.Yield();
            }
        }
    }
}
