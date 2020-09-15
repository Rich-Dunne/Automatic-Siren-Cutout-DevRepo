using System;
using System.Collections.Generic;
using System.Linq;
using Rage;

namespace AutomaticSirenCutout
{
    internal static class Yield
    {
        internal static List<Vehicle> yieldingVehicles = new List<Vehicle>();

        internal static void YieldMain()
        {
            Vector3 garage = new Vector3(396.42f, -970.0003f, -99.36382f);
            AppDomain.CurrentDomain.DomainUnload += TerminationHandler;

            while (true)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(garage) > 5f && Game.LocalPlayer.Character.LastVehicle && Game.LocalPlayer.Character.LastVehicle.IsSirenOn && Game.LocalPlayer.Character.LastVehicle.Speed == 0f)// && !bleftBlocker)
                {
                    foreach(Vehicle v in GetNearbyVehicles(Game.LocalPlayer.Character.LastVehicle.RearPosition, 10f).Where(v => v && v != Game.LocalPlayer.Character.LastVehicle && v.IsEngineOn && v.IsOnAllWheels && (Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) < 90f || Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) > 200f) && !yieldingVehicles.Contains(v)))
                    {
                        v.IsPersistent = true;
                        if (!v.HasDriver)
                        {
                            v.CreateRandomDriver();
                            while (!v.HasDriver)
                            {
                                GameFiber.Yield();
                            }
                        }
                        v.Driver.IsPersistent = true;

                        yieldingVehicles.Add(v);
                        Game.LogTrivialDebug($"{v.Model.Name} added to collection.");
                        v.PerformYieldTasks();
                    }
                }
                GameFiber.Yield();
            }

            Vehicle[] GetNearbyVehicles(Vector3 OriginPosition, float radius)
            {
                return (from x in World.GetAllVehicles() where !x.IsTrailer && x.DistanceTo(OriginPosition) < radius select x).ToArray();
            }
        }

        private static void PerformYieldTasks(this Vehicle v)
        {
            if (!v || !v.Driver)
            {
                Game.LogTrivialDebug($"Vehicle or driver is null");
                return;
            }

            if (v.Speed < 1f)
            {
                v.Driver.Tasks.PerformDrivingManeuver(v, VehicleManeuver.GoForwardWithCustomSteeringAngle, 1).WaitForCompletion();
            }
            v.Driver.Tasks.CruiseWithVehicle(5f, (VehicleDrivingFlags)558);
            //Game.LogTrivialDebug($"{v.Model.Name} should be cruising.");

            GameFiber.StartNew(() =>
            {
                while (v && v.Driver && Game.LocalPlayer.Character.LastVehicle && v.DistanceTo2D(Game.LocalPlayer.Character.LastVehicle.RearPosition) < 10f)
                {
                    if (v && v.Driver && v.Speed < 1f)
                    {
                        v.Driver.Tasks.PerformDrivingManeuver(v, VehicleManeuver.GoForwardWithCustomSteeringAngle, 1).WaitForCompletion();
                        v.Driver.Tasks.CruiseWithVehicle(5f, (VehicleDrivingFlags)558);
                    }
                    //Game.LogTrivialDebug($"Waiting for {v.Model.Name} to leave the area.");
                    GameFiber.Sleep(100);
                }

                yieldingVehicles.Remove(v);
                v.Driver.Tasks.Clear();
                Game.LogTrivialDebug($"{v.Model.Name} removed from collection.");

                v.Driver.Dismiss();
                v.Dismiss();
            });
        }

        internal static void TerminationHandler(object sender, EventArgs e)
        {
            foreach(Vehicle v in yieldingVehicles.Where(v => v))
            {
                if (v.Driver)
                {
                    v.Driver.Dismiss();
                }
                v.Dismiss();
            }
            yieldingVehicles.Clear();

            Game.LogTrivial("[ASC Yield]: Ending from crash or reload");
        }
    }
}
