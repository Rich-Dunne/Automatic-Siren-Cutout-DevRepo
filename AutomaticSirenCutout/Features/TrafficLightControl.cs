using System.Collections.Generic;
using System.Linq;
using Rage;

namespace AutomaticSirenCutout.Features
{
    internal class TrafficLightControl
    {
        internal static void Main()
        {
            List<uint> trafficSignalHashes = new List<uint> { 3639322914, 862871082, 865627822, 1043035044 };

            while (true)
            {
                GameFiber.Sleep(500);
                if(Game.LocalPlayer.Character.CurrentVehicle && Game.LocalPlayer.Character.CurrentVehicle.IsSirenOn)
                {
                    foreach (Entity entity in World.GetEntities(Game.LocalPlayer.Character.Position, 50f, GetEntitiesFlags.ConsiderAllObjects).Where(o => o && trafficSignalHashes.Contains(o.Model.Hash)))
                    {
                        if (entity.Heading <= Game.LocalPlayer.Character.LastVehicle.Heading + 45f && entity.Heading >= Game.LocalPlayer.Character.LastVehicle.Heading - 45f)
                        {
                            // Turn light green
                            Rage.Native.NativeFunction.Natives.SET_ENTITY_TRAFFICLIGHT_OVERRIDE(entity, 0);
                        }
                        else
                        {
                            // Turn light red
                            Rage.Native.NativeFunction.Natives.SET_ENTITY_TRAFFICLIGHT_OVERRIDE(entity, 1);
                        }
                    }
                }
            }
        }
    }
}
