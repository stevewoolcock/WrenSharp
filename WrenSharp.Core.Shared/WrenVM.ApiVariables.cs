using WrenSharp.Native;

namespace WrenSharp
{
    public partial class WrenVM
    {
        /// <summary>
        /// Stores the value of <paramref name="variableName"/> from resolved module <paramref name="module"/> into <paramref name="slot"/>.<para/>
        /// This method ensures that enough slots are allocated to hold the current value of the variable.
        /// </summary>
        /// <param name="module">The module name the variable resides in.</param>
        /// <param name="variableName">The name of the variable to load into <paramref name="slot"/>.</param>
        /// <param name="slot">The slot index to load the variable into.</param>
        public WrenVM LoadVariable(string module, string variableName, int slot)
        {
            Wren.EnsureSlots(m_Ptr, slot + 1);
            Wren.GetVariable(m_Ptr, module, variableName, slot);
            return this;
        }

        /// <summary>
        /// Attempts to store the value of <paramref name="variableName"/> from the resolved module <paramref name="module"/> into <paramref name="slot"/>.<para/>
        /// Returns true if the variable exists and was able to be loaded. This method ensures that enough slots are allocated to hold the current
        /// value of the variable.
        /// </summary>
        /// <param name="module">The module name the variable resides in.</param>
        /// <param name="variableName">The name of the variable to load into <paramref name="slot"/>.</param>
        /// <param name="slot">The slot index to load the variable into.</param>
        /// <returns>True if the variable exists and was placed into <paramref name="slot"/>, otherwise false.</returns>
        public bool TryLoadVariable(string module, string variableName, int slot)
        {
            if (Wren.HasVariable(m_Ptr, module, variableName) != 0)
            {
                LoadVariable(module, variableName, slot);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stores the value of <paramref name="variableName"/> from resolved module <paramref name="module"/> into <paramref name="slot"/>.<para/>
        /// This method does <b>not</b> ensure enough slots are available to hold the value of the variable. For a safer method of
        /// retrieving the value of a variable, use <see cref="LoadVariable(string, string, int)"/>.
        /// </summary>
        /// <param name="slot">The slot index to load the variable into.</param>
        /// <param name="module">The module name the variable resides in.</param>
        /// <param name="variableName">The name of the variable to load into <paramref name="slot"/>.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        public WrenVM GetVariable(string module, string variableName, int slot)
        {
            Wren.GetVariable(m_Ptr, module, variableName, slot);
            return this;
        }

        /// <summary>
        /// Attempts to store the value of <paramref name="variableName"/> from resolved module <paramref name="module"/> into <paramref name="slot"/>.<para/>
        /// Returns true if the variable exists. This method does <b>not</b> ensure enough slots are available to hold the value of the variable.
        /// For a safer method of retrieving the value of a variable, use <see cref="TryLoadVariable(string, string, int)"/>.
        /// </summary>
        /// <param name="slot">The slot index to load the variable into.</param>
        /// <param name="module">The module name the variable resides in.</param>
        /// <param name="variableName">The name of the variable to load into <paramref name="slot"/>.</param>
        /// <returns>True if the variable exists, otherwise false.</returns>
        public bool TryGetVariable(string module, string variableName, int slot)
        {
            if (Wren.HasVariable(m_Ptr, module, variableName) != 0)
            {
                Wren.GetVariable(m_Ptr, module, variableName, slot);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if <paramref name="variableName"/> exists within the resolved module <paramref name="module"/>.
        /// </summary>
        /// <param name="module">The module name to search in.</param>
        /// <param name="variableName">The variable name to find.</param>
        /// <returns>True if the variable is found in the module, otherwise false.</returns>
        public bool HasVariable(string module, string variableName)
        {
            return Wren.HasVariable(m_Ptr, module, variableName) != 0;
        }
    }
}
