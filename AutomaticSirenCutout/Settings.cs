using System;
using System.IO;
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
        internal static bool EnableLeaveEngineRunning = false;

        internal static void Prepare()
        {
            string fileName = "AutomaticSirenCutout.ini";
            var gameDirectory = Directory.GetCurrentDirectory();
            var iniDirectory = gameDirectory + @"\plugins\LSPDFR\";
            var iniPath = iniDirectory + fileName;
            var exists = File.Exists(iniPath);
            if (!exists)
            {
                Game.LogTrivial("[AutomaticSirenCutout]: INI file missing, creating file...");
                CreateINIFile(iniPath);
            }

            LoadSettings();
        }

        private static void CreateINIFile(string iniPath)
        {
            string content = @"// Automatic Siren Cutout Keybindings
// For a list of valid keyboard keys: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=windowsdesktop-9.0
// Valid controller buttons are:
//    DPadUp, DPadDown, DPadLeft, DPadRight, Start, Back, LeftThumb, RightThumb, LeftShoulder, RightShoulder, A, B, X, and Y.

[Keybindings]

// Must be bound to the same key/button you use to exit your vehicle
// Keyboard default is F key, controller default is Y button
KeyboardExitVehicle=F
ControllerExitVehicle=Y

// Must be bound to the same key/button you use to scroll through the ELS lighting stages
// Keyboard default is J key, controller default is DPadLeft button
KeyboardELSLights=J
ControllerELSLights=DPadLeft

// Must be bound to the same key/button you use to honk your horn
// Keyboard default is E key, controller default is LeftThumb button
KeyboardHorn=E
ControllerHorn=LeftThumb

// Must be bound to the same key/button you use to manually yelp your siren
// Keyboard default is R key, controller default is B button
KeyboardYelp=R
ControllerYelp=B

[Features]

//Enables/disables the automatic siren cutoff feature.
// Default is true
EnableASC=true

//Enables/disables the feature where nearby emergency vehicles will blip their siren at you if you give them a friendly honk or blip your manual siren.
// Default is false
EnableFriendlyHonk=false

//Enables/disables the feature where traffic lights will turn green for you when you are driving with stage 3 lighting on.  Be advised that AI traffic is NOT affected by the color of the lights.  This feature may be resource intensive for low-end systems resulting in FPS loss.
// Default is false
EnableTrafficLightControl=false

//Enables/disables the feature where AI will drive around your vehicle if you are stopped with stage 3 lighting on. **NOTE**  If you would like this feature to work with stage 1 or stage 2 lighting as well, you must go into each of your vehicle's ELS .xml files and change the value of <DfltSirenLtsActivateAtLstg>3</DfltSirenLtsActivateAtLstg> to 1 or 2.  Be aware that doing this will initiate cars to pull over if you're attempting a traffic stop (as it does with stage 3 lighting by default).
// Default is false
EnableYielding=false";

            File.WriteAllText(iniPath, content);
            Game.LogTrivial("[AutomaticSirenCutout]: INI created successfully.");
        }

        private static void LoadSettings()
        {
            Game.LogTrivial("[AutomaticSirenCutout]: Loading AutomaticSirenCutout.ini settings");
            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/AutomaticSirenCutout.ini");
            ini.Create();
            // Keybinds
            KeyboardExitVehicle = ini.ReadEnum("Keybindings", "KeyboardExitVehicle", Keys.F);
            ControllerExitVehicle = ini.ReadEnum("Keybindings", "ControllerExitVehicle", ControllerButtons.Y);
            KeyboardELSLights = ini.ReadEnum("Keybindings", "KeyboardELSLights", Keys.J);
            KeyboardHorn = ini.ReadEnum("Keybindings", "KeyboardHorn", Keys.E);
            ControllerHorn = ini.ReadEnum("Keybindings", "ControllerHorn", ControllerButtons.LeftThumb);
            KeyboardYelp = ini.ReadEnum("Keybindings", "KeyboardYelp", Keys.R);
            ControllerYelp = ini.ReadEnum("Keybindings", "ControllerYelp", ControllerButtons.B);

            // Features
            EnableASC = ini.ReadBoolean("Features", "EnableASC", true);
            EnableFriendlyHonk = ini.ReadBoolean("Features", "EnableFriendlyHonk", false);
            EnableTrafficLightControl = ini.ReadBoolean("Features", "EnableTrafficLightControl", false);
            EnableYielding = ini.ReadBoolean("Features", "EnableYielding", false);
            EnableLeaveEngineRunning = ini.ReadBoolean("Features", "EnableLeaveEngineRunning", false);

            if (KeyboardELSLights == Keys.None)
            {
                Game.LogTrivial($"[Automatic Siren Cutout]: Default keyboard key cannot be \"None\", defaulting to \"J\"");
                KeyboardELSLights = Keys.J;
                ini.Write("Keybindings", "KeyboardELSLights", KeyboardELSLights);
            }
        }
    }
}
