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
// Created On:   2018/06/01 14:27
// Modified On:  2018/10/26 22:48
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Sys.IO.FS;

namespace SuperMemoAssistant.Services.IO.FS
{
  public interface ICollectionFSService
  {
    string GetPluginResourcePath(ISMAPlugin plugin);

    CollectionFile this[int fileId] { get; }

    IEnumerable<CollectionFile> ForElement(int        elementId,
                                           ISMAPlugin plugin = null);

    IEnumerable<CollectionFile> ForPlugin(ISMAPlugin plugin);

    CollectionFile Create(ISMAPlugin     requester,
                          int            elementId,
                          Action<Stream> streamWriter,
                          string         extension,
                          string         crc32 = null);

    bool DeleteById(int fileId);

    int DeleteByElementId(int        elementId,
                          ISMAPlugin plugin = null);

    int DeleteByPlugin(ISMAPlugin plugin);
  }
}
