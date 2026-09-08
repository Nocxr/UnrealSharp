#if WITH_EDITOR
using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.Core.Attributes;
using UnrealSharp.Core.Marshallers;
using UnrealSharp.Interop;
using static UnrealSharp.Interop.Bind_FProperty;

namespace UnrealSharp.OptimusCore;

[UStruct, GeneratedType("OptimusConstantIdentifier", "UnrealSharp.OptimusCore.OptimusConstantIdentifier")]
public partial record struct FOptimusConstantIdentifier : MarshalledStruct<FOptimusConstantIdentifier>
{
    private static readonly IntPtr NativeClassPtr = Bind_UCoreUObject.CallGetType(
        typeof(FOptimusConstantIdentifier).GetAssemblyName(),
        "UnrealSharp.OptimusCore",
        "OptimusConstantIdentifier");

    private static readonly int NodePathOffset;
    private static readonly int GroupNameOffset;
    private static readonly int ConstantNameOffset;

    public FName NodePath;
    public FName GroupName;
    public FName ConstantName;

    public FOptimusConstantIdentifier(FName nodePath, FName groupName, FName constantName)
    {
        NodePath = nodePath;
        GroupName = groupName;
        ConstantName = constantName;
    }

    public static readonly int NativeDataSize;

    static FOptimusConstantIdentifier()
    {
        NodePathOffset = CallGetPropertyOffset(CallGetNativePropertyFromName(NativeClassPtr, "NodePath"));
        GroupNameOffset = CallGetPropertyOffset(CallGetNativePropertyFromName(NativeClassPtr, "GroupName"));
        ConstantNameOffset = CallGetPropertyOffset(CallGetNativePropertyFromName(NativeClassPtr, "ConstantName"));
        NativeDataSize = Bind_UScriptStruct.CallGetNativeStructSize(NativeClassPtr);
    }

    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public FOptimusConstantIdentifier(IntPtr nativeStruct)
    {
        NodePath = BlittableMarshaller<FName>.FromNative(nativeStruct + NodePathOffset, 0);
        GroupName = BlittableMarshaller<FName>.FromNative(nativeStruct + GroupNameOffset, 0);
        ConstantName = BlittableMarshaller<FName>.FromNative(nativeStruct + ConstantNameOffset, 0);
    }

    public static IntPtr GetNativeClassPtr() => NativeClassPtr;
    public static int GetNativeDataSize() => NativeDataSize;
    public static FOptimusConstantIdentifier FromNative(IntPtr buffer) => new(buffer);

    public void ToNative(IntPtr buffer)
    {
        BlittableMarshaller<FName>.ToNative(buffer + NodePathOffset, 0, NodePath);
        BlittableMarshaller<FName>.ToNative(buffer + GroupNameOffset, 0, GroupName);
        BlittableMarshaller<FName>.ToNative(buffer + ConstantNameOffset, 0, ConstantName);
    }
}

public static class FOptimusConstantIdentifierMarshaller
{
    public static FOptimusConstantIdentifier FromNative(IntPtr nativeBuffer, int arrayIndex) =>
        new(nativeBuffer + (arrayIndex * FOptimusConstantIdentifier.NativeDataSize));

    public static void ToNative(IntPtr nativeBuffer, int arrayIndex, FOptimusConstantIdentifier value) =>
        value.ToNative(nativeBuffer + (arrayIndex * FOptimusConstantIdentifier.NativeDataSize));
}
#endif
