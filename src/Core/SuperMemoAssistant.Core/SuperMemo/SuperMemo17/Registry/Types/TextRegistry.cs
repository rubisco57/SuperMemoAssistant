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
// Created On:   2018/05/19 11:29
// Modified On:  2018/05/30 23:26
// Modified By:  Alexis

#endregion




using System;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types
{
  [InitOnLoad]
  public class TextRegistry : RegistryBase<Text, IText>, ITextRegistry
  {
    #region Constants & Statics

    public static TextRegistry Instance { get; } = new TextRegistry();

    #endregion




    #region Properties & Fields - Non-Public

    protected override string MemFileName => SMConst.Files.TextMemFileName;
    protected override string RtxFileName => SMConst.Files.TextRtxFileName;
    protected override string RtfFileName => SMConst.Files.TextRtfFileName;
    protected override IntPtr RegistryPtr => new IntPtr(SMNatives.TRegistry.TextRegistryInstance.Read<int>(Svc.SM.Memory));

    #endregion




    #region Constructors

    protected TextRegistry() { }

    #endregion




    #region Methods Impl

    protected override Text Create(int id, RegMemElem mem, RegRtElem rtxOrRtf)
    {
      return new Text(id, mem, rtxOrRtf);
    }

    public Task<IText> AddAsync(string text)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
