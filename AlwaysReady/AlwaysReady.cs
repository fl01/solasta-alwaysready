using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;

namespace AlwaysReady
{
    public static class AlwaysReady
    {
        private static Harmony _hi;
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static bool Enabled { get; set; } = true;

        public static void Log(string message) => Logger?.Log(message);

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            _hi = new Harmony(modEntry.Info.Id);
            _hi.PatchAll(Assembly.GetExecutingAssembly());
            Log($"Patched {string.Join(", ", _hi.GetPatchedMethods().Select(mb => $"{mb.DeclaringType}.{mb.Name}"))}");
            modEntry.OnToggle = Toggle;
            return true;
        }

        public static bool Toggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }
    }

    [HarmonyPatch]
    public static class LoadingModalPatches
    {
        private static readonly MethodInfo _refreshStartButton = typeof(LoadingModal).GetMethod("RefreshStartButton", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly object[] _params = new object[] { false };

        [HarmonyPrefix, HarmonyPatch(typeof(LoadingModal), "OnUpdateHook")]
        public static bool Prefix_LoadingModal_OnUpdateHook(ref LoadingModal __instance)
        {
            if (!AlwaysReady.Enabled)
            {
                return true;
            }

            if (__instance.ReadyToProceed)
            {
                _refreshStartButton.Invoke(__instance, _params);
                __instance.OnProceedToLocationCb();
                return false;
            }

            return true;
        }
    }
}
