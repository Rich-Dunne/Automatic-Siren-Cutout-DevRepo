using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;

namespace AutomaticSirenCutout
{
    internal static class Yield
    {
        internal static List<Vehicle> yieldingVehicles = new List<Vehicle>();

        internal static void YieldMain()
        {
            Vector3 garage = new Vector3(396.42f, -970.0003f, -99.36382f);
            Vector3 rearPos = new Vector3();
            float collectionRadius = 7;
            AppDomain.CurrentDomain.DomainUnload += TerminationHandler;
            Vehicle pursuitVehicle = null;

            while (true)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(garage) > 5f && Game.LocalPlayer.Character.LastVehicle && Game.LocalPlayer.Character.LastVehicle.IsSirenOn && Game.LocalPlayer.Character.LastVehicle.Speed == 0f)
                {
                    rearPos = Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -4f, 0));
                    //Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, rearPos, 0, 0, 0, 0, 0, 0, collectionRadius, collectionRadius, 1f, 255, 255, 255, 100, false, false, 0, false, 0, 0, false);
                    foreach(Vehicle vehicle in Game.LocalPlayer.Character.GetNearbyVehicles(16).Where(v => v && v.FrontPosition.DistanceTo(rearPos) <= collectionRadius && v != Game.LocalPlayer.Character.LastVehicle && v.IsEngineOn && v.IsOnAllWheels && !v.IsSirenOn && !v.IsTrailer && !v.IsTrain && (Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) < 90f || Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) > 200f) && !yieldingVehicles.Contains(v)))
                    {
                        if(Functions.GetActivePursuit() != null && IsVehicleInCurrentPursuit(vehicle))
                        {
                            continue;
                        }
                        if (vehicle.HasSiren && IsVehicleBackupUnit(vehicle))
                        {
                            continue;
                        }
                        if(vehicle)
                        {
                            SetVehicleAndDriverPersistence(vehicle);
                            yieldingVehicles.Add(vehicle);
                            Game.LogTrivialDebug($"[ASC Yield]: {vehicle.Model.Name} added to collection.");

                            GameFiber YieldTasksFiber = new GameFiber(() => PerformYieldTasks(vehicle));
                            YieldTasksFiber.Start();
                        }
                    }
                }
                GameFiber.Yield();
            }

            bool IsVehicleInCurrentPursuit(Vehicle vehicle)
            {
                if (vehicle.HasDriver && vehicle.Driver && Functions.GetPursuitPeds(Functions.GetActivePursuit()).Contains(vehicle.Driver))
                {
                    pursuitVehicle = vehicle;
                    Game.LogTrivialDebug($"[ASC Yield]: Vehicle is in the current pursuit, ignore.");
                    return true;
                }
                return false;
            }

            bool IsVehicleBackupUnit(Vehicle vehicle)
            {
                Rage.Native.NativeFunction.Natives.x260BE8F09E326A20(vehicle, collectionRadius / 2, 5, true);
                //v.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
                GameFiber.Sleep(7000);
                if (vehicle && !vehicle.HasDriver && vehicle.FrontPosition.DistanceTo(rearPos) <= collectionRadius)
                {
                    Game.LogTrivialDebug($"[ASC Yield]: {vehicle.Model.Name} is an emergency vehicle and hasn't moved in 7 seconds.  It might be a backup unit, disregard.");
                    return true;
                }
                //Rage.Native.NativeFunction.Natives.x260BE8F09E326A20(v, 0f, 1, true);
                //vehicle.Driver.Tasks.Clear();
                return false;
            }

            void SetVehicleAndDriverPersistence(Vehicle vehicle)
            {
                vehicle.IsPersistent = true;
                if (!vehicle.HasDriver)
                {
                    vehicle.CreateRandomDriver();
                    while (vehicle && !vehicle.HasDriver)
                    {
                        GameFiber.Yield();
                    }
                    if (!vehicle)
                    {
                        Game.LogTrivialDebug($"[ASC Yield]: Vehicle is null");
                        return;
                    }
                }
                vehicle.Driver.IsPersistent = true;
            }

            void PerformYieldTasks(Vehicle vehicle)
            {
                if (!vehicle)
                {
                    Game.LogTrivialDebug($"[ASC Yield]: Vehicle is null");
                    return;
                }
                if (!vehicle.Driver)
                {
                    Game.LogTrivialDebug($"[ASC Yield]: Driver is null");
                    return;
                }

                SetCustomSteeringAngle();

                while (vehicle && vehicle.Driver && Game.LocalPlayer.Character.LastVehicle && vehicle.FrontPosition.DistanceTo2D(Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -4f, 0))) < 7f)
                {
                    if (vehicle && vehicle.Driver && vehicle.Speed < 1f)
                    {
                        vehicle.Driver.Tasks.PerformDrivingManeuver(vehicle, VehicleManeuver.GoForwardWithCustomSteeringAngle, 1).WaitForCompletion();
                        if (vehicle && vehicle.Driver)
                        {
                            vehicle.Driver.Tasks.CruiseWithVehicle(5f, (VehicleDrivingFlags)558);
                        }
                    }
                    //Game.LogTrivialDebug($"[Yield]: Waiting for {v.Model.Name} to leave the area.");
                    GameFiber.Sleep(100);
                }
                if (vehicle)
                {
                    Dismiss();
                }

                void SetCustomSteeringAngle()
                {
                    if (vehicle.Speed < 1f)
                    {
                        vehicle.SteeringAngle = 45;
                        vehicle.Driver.Tasks.PerformDrivingManeuver(vehicle, VehicleManeuver.GoForwardWithCustomSteeringAngle, 2).WaitForCompletion();

                        if (vehicle && vehicle.Driver)
                        {
                            vehicle.Driver.Tasks.CruiseWithVehicle(5f, (VehicleDrivingFlags)558);
                        }
                    }
                }

                void Dismiss()
                {
                    yieldingVehicles.Remove(vehicle);

                    if (vehicle)
                    {
                        if (vehicle.Driver)
                        {
                            vehicle.Driver.Tasks.Clear();
                            vehicle.Driver.Dismiss();
                        }
                        Game.LogTrivialDebug($"[ASC Yield]: {vehicle.Model.Name} removed from collection.");
                        vehicle.Dismiss();
                    }
                }
            }
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

            Game.LogTrivial("[ASC Yield]: Plugin terminated.");
        }
    }
}
