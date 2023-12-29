// Decompiled with JetBrains decompiler
// Type: ame_assassin.RegistryValueType
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

#nullable disable
namespace ame_assassin
{
  public enum RegistryValueType
  {
    REG_NONE = -1, // 0xFFFFFFFF
    REG_UNKNOWN = 0,
    REG_SZ = 1,
    REG_EXPAND_SZ = 2,
    REG_BINARY = 3,
    REG_DWORD = 4,
    REG_MULTI_SZ = 7,
    REG_QWORD = 11, // 0x0000000B
  }
}
