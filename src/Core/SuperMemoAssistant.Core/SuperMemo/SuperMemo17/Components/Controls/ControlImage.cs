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
// Created On:   2018/06/21 11:20
// Modified On:  2018/06/21 12:29
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Interop.SuperMemo.Components.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Components.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Components.Controls
{
  public class ControlImage : ComponentControlBase, IControlImage
  {
    #region Constructors

    /// <inheritdoc />
    public ControlImage(int           id,
                        ControlGroup  group)
      : base(id,
             ComponentType.Image,
             group) { }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public IImage ImageMember
    {
      get => SMA.Instance.Registry.Image[ImageMemberId];
      set => ImageMemberId = value?.Id ?? throw new ArgumentNullException();
    }

    /// <inheritdoc />
    public int ImageMemberId
    {
      get => _group.GetImageRegMember(this);
      set => _group.SetImageRegMember(this,
                                     value);
    }

    #endregion
  }
}
