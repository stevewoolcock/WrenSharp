#if WRENSHARP_EXT
namespace WrenSharp.Unsafe
{
    public enum WrenObjectType
    {
        // No Wren equivalent
        Unknown = -1,

        // OBJ_CLASS
        Class,

        // OBJ_CLOSURE
        Closure,

        // OBJ_FIBER
        Fiber,

        // OBJ_FN
        Function,

        // OBJ_FOREIGN
        Foreign,

        // OBJ_INSTANCE
        Instance,

        // OBJ_LIST
        List,

        // OBJ_MAP
        Map,

        // OBJ_MODULE
        Module,

        // OBJ_RANGE
        Range,

        // OBJ_STRING
        String,

        // OBJ_UPVALUE
        Upvalue
    }
}
#endif