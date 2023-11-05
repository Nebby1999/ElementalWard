using Nebula;

namespace ElementalWard
{
    public static class StaticElementReferences
    {
        public static ElementDef WaterDef { get; private set; }
        public static ElementDef FireDef { get; private set; }
        public static DotBuffDef OnFireDot { get; private set; }
        public static ElementDef ElectricDef { get; private set; }


        [SystemInitializer]
        private static void SystemInit()
        {
            ElementCatalog.resourceAvailability.CallWhenAvailable(() =>
            {
                WaterDef = ElementCatalog.GetElementDef(ElementCatalog.FindElementIndex("WaterElement"));
                FireDef = ElementCatalog.GetElementDef(ElementCatalog.FindElementIndex("FireElement"));
                ElectricDef = ElementCatalog.GetElementDef(ElementCatalog.FindElementIndex("ElectricElement"));
            });

            BuffCatalog.resourceAvailability.CallWhenAvailable(() =>
            {
                OnFireDot = BuffCatalog.GetDotDef(BuffCatalog.FindDotIndex("OnFireDot"));
            });
        }
    }
}