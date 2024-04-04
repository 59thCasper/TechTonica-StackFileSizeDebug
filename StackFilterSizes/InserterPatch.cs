using HarmonyLib;



namespace StackFilterSizes
{
    public static class InserterPatch
    {
        [HarmonyPatch(typeof(InserterInstance), "get_maxStackSize")]
        public static class MaxStackSizePatch
        {
            [HarmonyPostfix]
            public static void Postfix(ref int __result, InserterInstance __instance)
            {
                if (SharedState.PatchEnabled)
                {
                    if (__instance.isStack)
                    {
                        __result = SharedState.InserterValue;
                    }
                }
            }
        }
    }
}


