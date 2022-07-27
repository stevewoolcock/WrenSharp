using System.Collections.Generic;

namespace WrenSharp.Native
{
    internal class ForeignLookup<TForeign>
    {
        private class Module
        {
            public Dictionary<string, TForeign> Classes = new Dictionary<string, TForeign>();
        }

        private readonly Dictionary<string, Module> Modules = new Dictionary<string, Module>();

        public void AddClass(string moduleName, string className, TForeign foreign)
        {
            if (!Modules.TryGetValue(moduleName, out Module module))
            {
                module = new Module();
                Modules[moduleName] = module;
            }

            module.Classes[className] = foreign;
        }

        public TForeign GetClass(string moduleName, string className)
        {
            if (!Modules.TryGetValue(moduleName, out Module module))
                return default;

            module.Classes.TryGetValue(className, out TForeign foreign);
            return foreign;
        }
    }
}
