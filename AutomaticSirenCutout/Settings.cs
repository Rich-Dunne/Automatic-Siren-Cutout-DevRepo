using System.Windows.Forms;
using Rage;

namespace AutomaticSirenCutout
{
    internal static class Settings
    {
        internal static Keys KeyboardExitVehicle = Keys.F;
        internal static Keys KeyboardELSLights = Keys.J;
        internal static Keys KeyboardHorn = Keys.E;
        internal static Keys KeyboardYelp = Keys.R;
        internal static ControllerButtons ControllerExitVehicle = ControllerButtons.Y;
        internal static ControllerButtons ControllerELSLights = ControllerButtons.DPadLeft;
        internal static ControllerButtons ControllerHorn = ControllerButtons.LeftThumb;
        internal static ControllerButtons ControllerYelp = ControllerButtons.B;
        internal static bool EnableASC = true;
        internal static bool EnableFriendlyHonk = false;
        internal static bool EnableTrafficLightControl = false;
        internal static bool EnableYielding = false;

        internal static void LoadSettings()
        {
            Game.LogTrivial("Loading AutomaticSirenCutout.ini settings");
            InitializationFile ini = new InitializationFile("Plugins/AutomaticSirenCutout.ini");
            ini.Create();
            KeyboardExitVehicle = ini.ReadEnum("Keybindings", "KeyboardExitVehicle", Keys.F);
            ControllerExitVehicle = ini.ReadEnum("Keybindings", "ControllerExitVehicle", ControllerButtons.Y);
            KeyboardELSLights = ini.ReadEnum("Keybindings", "KeyboardELSLights", Keys.J);
            ControllerELSLights = ini.ReadEnum("Keybindings", "ControllerELSLights", ControllerButtons.DPadLeft);
            KeyboardHorn = ini.ReadEnum("Keybindings", "KeyboardHorn", Keys.E);
            ControllerHorn = ini.ReadEnum("Keybindings", "ControllerHorn", ControllerButtons.LeftThumb);
            KeyboardYelp = ini.ReadEnum("Keybindings", "KeyboardYelp", Keys.R);
            ControllerYelp = ini.ReadEnum("Keybindings", "ControllerYelp", ControllerButtons.B);
            EnableASC = ini.ReadBoolean("Features", "EnableASC", true);
            EnableFriendlyHonk = ini.ReadBoolean("Features", "EnableFriendlyHonk", false);
            EnableTrafficLightControl = ini.ReadBoolean("Features", "EnableTrafficLightControl", false);
            EnableYielding = ini.ReadBoolean("Features", "EnableYielding", false);
        }
    }
}
