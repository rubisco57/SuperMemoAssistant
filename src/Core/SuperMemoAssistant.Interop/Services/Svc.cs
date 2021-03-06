﻿#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2018/06/01 15:25
// Modified On:  2019/01/04 21:21
// Modified By:  Alexis

#endregion




using FlaUI.UIA3;
using Process.NET;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Services.IO.Devices;
using SuperMemoAssistant.Services.IO.FS;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable UnusedTypeParameter

namespace SuperMemoAssistant.Services
{
  public static class Svc
  {
    #region Constants & Statics

    public static ISuperMemoAssistant SMA { get; set; }
    public static IProcess            SM => SMA.SMProcess;

    public static UIA3Automation UIAutomation { get; } = new UIA3Automation();

    public static IKeyboardHotKeyService KeyboardHotKeyLegacy { get; set; }
    public static IKeyboardHookService   KeyboardHotKey       { get; set; }

    #endregion
  }

  public static class Svc<T>
    where T : ISMAPlugin
  {
    #region Constants & Statics

    public static PluginCollectionFSService CollectionFS  { get; set; }
    public static ConfigurationService      Configuration { get; set; }

    public static T Plugin { get; set; }

    #endregion
  }
}
