﻿using System;
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
            Vector3 rearPos = new Vector3();
            float collectionRadius = 7;
            AppDomain.CurrentDomain.DomainUnload += TerminationHandler;

            while (true)
            {
                if (Game.LocalPlayer.Character.Position.DistanceTo(garage) > 5f && Game.LocalPlayer.Character.LastVehicle && Game.LocalPlayer.Character.LastVehicle.IsSirenOn && Game.LocalPlayer.Character.LastVehicle.Speed == 0f)
                {
                    rearPos = Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -4f, 0));
                    //Rage.Native.NativeFunction.Natives.DRAW_MARKER(1, rearPos, 0, 0, 0, 0, 0, 0, collectionRadius, collectionRadius, 1f, 255, 255, 255, 100, false, false, 0, false, 0, 0, false);
                    foreach(Vehicle v in Game.LocalPlayer.Character.GetNearbyVehicles(16))
                    //foreach (Vehicle v in GetNearbyVehicles(Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -4f, 0)), 5f))
                    {
                        if (v && v.FrontPosition.DistanceTo(rearPos) <= collectionRadius && v != Game.LocalPlayer.Character.LastVehicle && v.IsEngineOn && v.IsOnAllWheels && !v.IsSirenOn && !v.IsTrailer && !v.IsTrain && (Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) < 90f || Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) > 200f) && !yieldingVehicles.Contains(v))
                        {
                            SetVehicleAndDriverPersistence(v);
                            yieldingVehicles.Add(v);
                            Game.LogTrivialDebug($"{v.Model.Name} added to collection.");

                            GameFiber YieldTasksFiber = new GameFiber(() => v.PerformYieldTasks());
                            YieldTasksFiber.Start();
                        }
                    }
                }
                GameFiber.Yield();
            }

            Vehicle[] GetNearbyVehicles(Vector3 OriginPosition, float radius)
            {
                return (from v in World.GetAllVehicles() where v && v.FrontPosition.DistanceTo(OriginPosition) <= radius && v != Game.LocalPlayer.Character.LastVehicle && v.IsEngineOn && v.IsOnAllWheels && !v.IsSirenOn && !v.IsTrailer && !v.IsTrain && (Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) < 90f || Math.Abs(Game.LocalPlayer.Character.LastVehicle.Heading - v.Heading) > 200f) select v).ToArray();
            }
        }

        private static void SetVehicleAndDriverPersistence(Vehicle v)
        {
            v.IsPersistent = true;
            if (!v.HasDriver)
            {
                v.CreateRandomDriver();
                while (v && !v.HasDriver)
                {
                    GameFiber.Yield();
                }
                if (!v)
                {
                    Game.LogTrivialDebug($"Vehicle is null");
                    return;
                }
            }
            v.Driver.IsPersistent = true;
        }

        private static void PerformYieldTasks(this Vehicle v)
        {
            if (!v)
            {
                Game.LogTrivialDebug($"Vehicle is null");
                return;
            }
            if (!v.Driver)
            {
                Game.LogTrivialDebug($"Driver is null");
                return;
            }

            if (v.Speed < 1f)
            {
                v.Driver.Tasks.PerformDrivingManeuver(v, VehicleManeuver.GoForwardWithCustomSteeringAngle, 1).WaitForCompletion();
                if (v && v.Driver)
                {
                    v.Driver.Tasks.CruiseWithVehicle(5f, (VehicleDrivingFlags)558);
                }
            }

            while (v && v.Driver && Game.LocalPlayer.Character.LastVehicle && v.FrontPosition.DistanceTo2D(Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -4f, 0))) < 7f)
            {
                if (v && v.Driver && v.Speed < 1f)
                {
                    v.Driver.Tasks.PerformDrivingManeuver(v, VehicleManeuver.GoForwardWithCustomSteeringAngle, 1).WaitForCompletion();
                    if(v && v.Driver)
                    {
                        v.Driver.Tasks.CruiseWithVehicle(5f, (VehicleDrivingFlags)558);
                    }
                }
                Game.LogTrivialDebug($"Waiting for {v.Model.Name} to leave the area.");
                GameFiber.Sleep(100);
            }

            Dismiss(v);
        }

        private static void Dismiss(this Vehicle v)
        {
            yieldingVehicles.Remove(v);
            v.Driver.Tasks.Clear();
            Game.LogTrivialDebug($"{v.Model.Name} removed from collection.");

            v.Driver.Dismiss();
            v.Dismiss();
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
