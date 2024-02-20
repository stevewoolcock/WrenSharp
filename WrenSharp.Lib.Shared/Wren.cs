using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WrenSharp.Native
{
    public partial class Wren
    {
        [DllImport(NativeLibrary, EntryPoint = "wrenGetVersionNumber", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetVersionNumber();

        [DllImport(NativeLibrary, EntryPoint = "wrenInitConfiguration", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitConfiguration(ref WrenConfiguration configuration);

        [DllImport(NativeLibrary, EntryPoint = "wrenNewVM", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NewVM(ref WrenConfiguration configuration);

        [DllImport(NativeLibrary, EntryPoint = "wrenFreeVM", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeVM(IntPtr vm);

        [DllImport(NativeLibrary, EntryPoint = "wrenCollectGarbage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CollectGarbage(IntPtr vm);

        [DllImport(NativeLibrary, EntryPoint = "wrenInterpret", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern WrenInterpretResult Interpret(IntPtr vm, [In] string module, [In] string source);

        [DllImport(NativeLibrary, EntryPoint = "wrenInterpret", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe static extern WrenInterpretResult Interpret(IntPtr vm, [In] string module, [In] ReadOnlySpan<char> source);

        [DllImport(NativeLibrary, EntryPoint = "wrenInterpret", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern WrenInterpretResult Interpret(IntPtr vm, [In] string module, [In] StringBuilder source);

        [DllImport(NativeLibrary, EntryPoint = "wrenInterpret", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern WrenInterpretResult Interpret(IntPtr vm, [In] string module, [In] byte[] source);

        [DllImport(NativeLibrary, EntryPoint = "wrenInterpret", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern WrenInterpretResult Interpret(IntPtr vm, [In] string module, [In] IntPtr source);

        [DllImport(NativeLibrary, EntryPoint = "wrenMakeCallHandle", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr MakeCallHandle(IntPtr vm, [In] string signature);

        [DllImport(NativeLibrary, EntryPoint = "wrenCall", CallingConvention = CallingConvention.Cdecl)]
        public static extern WrenInterpretResult Call(IntPtr vm, IntPtr method);

        [DllImport(NativeLibrary, EntryPoint = "wrenReleaseHandle", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReleaseHandle(IntPtr vm, IntPtr handle);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetSlotCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSlotCount(IntPtr vm);

        [DllImport(NativeLibrary, EntryPoint = "wrenEnsureSlots", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnsureSlots(IntPtr vm, int numSlots);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetSlotType", CallingConvention = CallingConvention.Cdecl)]
        public static extern WrenType GetSlotType(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetSlotBool", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte GetSlotBool(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetSlotBytes", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetSlotBytes(IntPtr vm, int slot, IntPtr length);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetSlotDouble", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetSlotDouble(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetSlotForeign", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetSlotForeign(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetSlotString", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern string GetSlotString(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetSlotHandle", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetSlotHandle(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetSlotBool", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSlotBool(IntPtr vm, int slot, [MarshalAs(UnmanagedType.I1)] bool value);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetSlotBytes", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSlotBytes(IntPtr vm, int slot, IntPtr bytes, ulong length);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetSlotDouble", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSlotDouble(IntPtr vm, int slot, double value);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetSlotNewForeign", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetSlotNewForeign(IntPtr vm, int slot, int classSlot, ulong size);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetSlotNewList", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSlotNewList(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetSlotNewMap", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSlotNewMap(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetSlotNull", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSlotNull(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetSlotString", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetSlotString(IntPtr vm, int slot, [In] string text);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetSlotHandle", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSlotHandle(IntPtr vm, int slot, IntPtr handle);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetListCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetListCount(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetListElement", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetListElement(IntPtr vm, int listSlot, int index, int elementSlot);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetListElement", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetListElement(IntPtr vm, int listSlot, int index, int elementSlot);

        [DllImport(NativeLibrary, EntryPoint = "wrenInsertInList", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InsertInList(IntPtr vm, int listSlot, int index, int elementSlot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetMapCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetMapCount(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetMapContainsKey", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte GetMapContainsKey(IntPtr vm, int mapSlot, int keySlot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetMapValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetMapValue(IntPtr vm, int mapSlot, int keySlot, int valueSlot);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetMapValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMapValue(IntPtr vm, int mapSlot, int keySlot, int valueSlot);

        [DllImport(NativeLibrary, EntryPoint = "wrenRemoveMapValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RemoveMapValue(IntPtr vm, int mapSlot, int keySlot, int removedValueSlot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetVariable", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void GetVariable(IntPtr vm, [In] string module, [In] string name, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenHasVariable", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern byte HasVariable(IntPtr vm, [In] string module, [In] string name);

        [DllImport(NativeLibrary, EntryPoint = "wrenHasModule", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern byte HasModule(IntPtr vm, [In] string module);

        [DllImport(NativeLibrary, EntryPoint = "wrenAbortFiber", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AbortFiber(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "wrenGetUserData", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetUserData(IntPtr vm);

        [DllImport(NativeLibrary, EntryPoint = "wrenSetUserData", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetUserData(IntPtr vm, IntPtr userData);

#if WRENSHARP_EXT
        [DllImport(NativeLibrary, EntryPoint = "ext_wrenGetListIndexOf", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetListIndexOf(IntPtr vm, int listSlot, int valueSlot);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenListClear", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListClear(IntPtr vm, int listSlot);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenListRemove", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListRemove(IntPtr vm, int listSlot, int index, int removedValueSlot);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenMapClear", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MapClear(IntPtr vm, int mapSlot);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenCreateFiber", CallingConvention = CallingConvention.Cdecl)]
        public static extern WrenFiberResume CreateFiber(IntPtr vm);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenResumeFiber", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResumeFiber(IntPtr vm, WrenFiberResume resume);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenSetGCEnabled", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetGCEnabled(IntPtr vm, [MarshalAs(UnmanagedType.I1)] bool value);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenGetGCEnabled", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetGCEnabled(IntPtr vm);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenBytesAllocated", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong BytesAllocated(IntPtr vm);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenGetSlotPtr", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe Unsafe.WrenValue* GetSlotPtr(IntPtr vm, int slot);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenSetSlotValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSlot(IntPtr vm, int slot, Unsafe.WrenValue value);

        [DllImport(NativeLibrary, EntryPoint = "ext_wrenGetSlotForeignSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong GetSlotForeignSize(IntPtr vm, int slot);
#endif
    }
}
