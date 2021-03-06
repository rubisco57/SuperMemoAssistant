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
// Created On:   2018/07/27 12:55
// Modified On:  2018/12/09 03:03
// Modified By:  Alexis

#endregion




using System.IO;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.Interop.SuperMemo.Core
{
  public static class SMCollectionEx
  {
    #region Methods

    public static string GetFilePath(
      this   SMCollection collection,
      params string[]     paths)
    {
      return Path.Combine(collection.Path,
                          collection.Name,
                          Path.Combine(paths));
    }

    public static string GetElementFilePath(
      this SMCollection collection,
      string            filePath)
    {
      return collection.GetFilePath(SMConst.Paths.ElementsFolder,
                                    filePath);
    }

    public static string GetInfoFilePath(
      this SMCollection collection,
      string            fileName)
    {
      return collection.GetFilePath(SMConst.Paths.InfoFolder,
                                    fileName);
    }

    public static string GetRegistryFilePath(
      this SMCollection collection,
      string            fileName)
    {
      return collection.GetFilePath(SMConst.Paths.RegistryFolder,
                                    fileName);
    }

    public static string GetSMAFolder(
      this SMCollection collection)
    {
      return collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder);
    }

    public static string GetSMAElementsFolder(
      this SMCollection collection,
      int               elementId = 0)
    {
      return elementId == 0
        ? collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder,
                                 SMAConst.Paths.CollectionElementsFolder,
                                 elementId.ToString())
        : collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder,
                                 SMAConst.Paths.CollectionElementsFolder);
    }

    public static string GetSMAPluginsFolder(
      this SMCollection collection,
      ISMAPlugin        plugin = null)
    {
      return plugin != null
        ? collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder,
                                 SMAConst.Paths.CollectionPluginsFolder,
                                 plugin.Id.ToString("D"))
        : collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder,
                                 SMAConst.Paths.CollectionPluginsFolder);
    }

    public static string GetSMASystemFolder(
      this SMCollection collection)
    {
      return collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder,
                                    SMAConst.Paths.CollectionSystemFolder);
    }

    public static string GetSMAElementsFilePath(
      this SMCollection collection,
      IElement          element,
      string            fileName)
    {
      return collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder,
                                    SMAConst.Paths.CollectionElementsFolder,
                                    element.Id.ToString(),
                                    fileName);
    }

    public static string GetSMAElementsFilePath(
      this SMCollection collection,
      int               elementId,
      string            fileName)
    {
      return collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder,
                                    SMAConst.Paths.CollectionElementsFolder,
                                    elementId.ToString(),
                                    fileName);
    }

    public static string GetSMAPluginsFilePath(
      this SMCollection collection,
      ISMAPlugin        plugin,
      string            fileName)
    {
      return collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder,
                                    SMAConst.Paths.CollectionPluginsFolder,
                                    plugin.Id.ToString("D"),
                                    fileName);
    }

    public static string GetSMASystemFilePath(
      this SMCollection collection,
      string            fileName)
    {
      return collection.GetFilePath(SMAConst.Paths.CollectionSMAFolder,
                                    SMAConst.Paths.CollectionSystemFolder,
                                    fileName);
    }

    public static string GetKnoFilePath(this SMCollection collection)
    {
      return Path.Combine(collection.Path,
                          collection.Name + ".Kno");
    }

    public static string MakeRelative(this SMCollection collection,
                                      string            absolutePath)
    {
      string basePath = collection.GetFilePath();

      return absolutePath.StartsWith(basePath)
        ? absolutePath.Substring(basePath.Length).TrimStart('\\', '/')
        : absolutePath;
    }

    #endregion
  }
}
