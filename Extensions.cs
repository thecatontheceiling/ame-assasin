// Decompiled with JetBrains decompiler
// Type: ame_assassin.Extensions
// Assembly: ame-assassin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 290C26D0-0B34-4756-9171-250499022CFA
// Assembly location: C:\Users\home-pc\Downloads\ame-assassin\ame-assassin.exe

using System;
using System.Text.RegularExpressions;

#nullable disable
namespace ame_assassin
{
  public static class Extensions
  {
    public static bool EqualsIC(this string text, string value)
    {
      return text.Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContainsIC(this string text, string value)
    {
      return text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static string ReplaceIC(this string text, string value, string replacement)
    {
      return Regex.Replace(text.ToString(), Regex.Escape(value), Environment.ExpandEnvironmentVariables(replacement), RegexOptions.IgnoreCase);
    }
  }
}
