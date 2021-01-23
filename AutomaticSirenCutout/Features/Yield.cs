using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using AutomaticSirenCutout.Utils;

namespace AutomaticSirenCutout.Features
{
    internal static class Yield
    {
        private static List<Vehicle> YieldingVehicles = new List<Vehicle>();

        internal static void YieldMain()
        {
            Vector3 garage = new Vector3(396.42f, -970.0003f, -99.36382f);
            float collectionRadius = 7;
            AppDomain.CurrentDomain.DomainUnload += TerminationHandler;
            List<Vehicle> ignoredVehicles = new List<Vehicle>();

            GameFiber.StartNew(() => CleanupCollectedVehicles(), "Collected Vehicles Cleanup Fiber");

            while (true)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(garage) > 5f && Game.LocalPlayer.Character.LastVehicle && Game.LocalPlayer.Character.LastVehicle.IsSirenOn && Game.LocalPlayer.Character.LastVehicle.Speed == 0f)
                {
                    var rearPos = Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -4f, 0));
                    //Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, rearPos, 0, 0, 0, 0, 0, 0, collectionRadius, collectionRadius, 1f, 255, 255, 255, 100, false, false, 0, false, 0, 0, false);
                    foreach(Vehicle vehicle in Game.LocalPlayer.Character.GetNearbyVehicles(16).Where(v => v && v.FrontPosition.DistanceTo(rearPos) <= collectionRadius && v != Game.LocalPlayer.Character.LastVehicle && v.IsEngineOn && v.IsOnAllWheels && !v.IsSirenOn && !v.IsTrailer && !v.IsTrain && (Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) < 90f || Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) > 200f) && !YieldingVehicles.Contains(v) && !ignoredVehicles.Contains(v)))
                    {
                        if (VehicleShouldBeIgnored(vehicle) && !ignoredVehicles.Contains(vehicle))
                        {
                            ignoredVehicles.Add(vehicle);
                        }
                        else
                        {
                            SetVehicleAndDriverPersistence(vehicle);
                            YieldingVehicles.Add(vehicle);
                            Game.LogTrivialDebug($"[ASC Yield]: {vehicle.Model.Name} added to yield collection.");

                            GameFiber.StartNew(() => PerformYieldTasks(vehicle), "Yield Task Fiber");
                        }
                    }
                }
                GameFiber.Yield();
            } 
        }

        private static bool VehicleShouldBeIgnored(Vehicle vehicle)
        {
            if (Functions.GetCurrentPullover() != null && Functions.GetPulloverSuspect(Functions.GetCurrentPullover()).CurrentVehicle == vehicle)
            {
                return true;
            }
            if (Functions.GetActivePursuit() != null && IsVehicleInCurrentPursuit())
            {
                return true;
            }
            if (vehicle.HasSiren && vehicle.Driver && !vehicle.Driver.IsAmbient())
            {
                return true;
            }

            return false;

            bool IsVehicleInCurrentPursuit()
            {
                if (vehicle.HasDriver && vehicle.Driver && Functions.GetPursuitPeds(Functions.GetActivePursuit()).Contains(vehicle.Driver))
                {
                    Game.LogTrivialDebug($"[ASC Yield]: Vehicle is in the current pursuit, ignore.");
                    return true;
                }
                return false;
            }
        }

        private static void SetVehicleAndDriverPersistence(Vehicle vehicle)
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

        private static void PerformYieldTasks(Vehicle vehicle)
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

            while (vehicle && vehicle.Driver && Game.LocalPlayer.Character.LastVehicle && vehicle.FrontPosition.DistanceTo2D(Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -4f, 0))) < 7f)
            {
                if (vehicle && vehicle.Driver && vehicle.Speed < 2f)
                {
                    vehicle.SteeringAngle = 45;
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

            void Dismiss()
            {
                if (vehicle)
                {
                    if (vehicle.Driver)
                    {
                        vehicle.Driver.Tasks.Clear();
                        vehicle.Driver.Dismiss();
                    }
                    Game.LogTrivialDebug($"[ASC Yield]: {vehicle.Model.Name} removed from yield collection.");
                    vehicle.Dismiss();
                }
            }
        }

        private static void CleanupCollectedVehicles()
        {
            while (true)
            {
                YieldingVehicles.RemoveAll(x => !x);
                GameFiber.Sleep(5000);
            }
        }

        private static void TerminationHandler(object sender, EventArgs e)
        {
            foreach(Vehicle v in YieldingVehicles.Where(v => v))
            {
                if (v.Driver)
                {
                    v.Driver.Dismiss();
                }
                v.Dismiss();
            }
            YieldingVehicles.Clear();

            Game.LogTrivial("[AutomaticSirenCutout]: Plugin terminated.");
        }
    }
}
