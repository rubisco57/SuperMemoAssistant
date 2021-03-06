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
// Created On:   2018/06/01 14:29
// Modified On:  2018/11/23 12:39
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Anotar.Serilog;
using JetBrains.Annotations;
using LiteDB;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Services.IO.FS;
using SuperMemoAssistant.Sys.IO.FS;
// ReSharper disable PossibleMultipleEnumeration

namespace SuperMemoAssistant.PluginsHost.Services.FS
{
  [Export(typeof(ICollectionFSService))]
  public class CollectionFSService : ICollectionFSService
  {
    #region Properties & Fields - Non-Public

    protected ISuperMemoAssistant SMA { get; set; }

    protected LiteCollection<CollectionFSFile> DbFiles { get; set; }

    #endregion




    #region Constructors

    [ImportingConstructor]
    public CollectionFSService(ISuperMemoAssistant sma)
    {
      SMA = sma;

      LoadDb();
    }

    #endregion




    #region Properties Impl - Public

    public CollectionFile this[int fileId] => FromDbFile(DbFiles.FindById(fileId));

    #endregion




    #region Methods Impl

    public IEnumerable<CollectionFile> ForElement(int        elementId,
                                                  ISMAPlugin plugin = null)
    {
      Query query = Query.EQ("ElementId",
                             elementId);

      if (plugin != null)
        query = Query.And(query,
                          Query.EQ("PluginId",
                                   plugin.Id));

      return DbFiles.Find(query).Select(FromDbFile);
    }

    /// <inheritdoc />
    public IEnumerable<CollectionFile> ForPlugin([NotNull] ISMAPlugin plugin)
    {
      return DbFiles.Find(f => f.PluginId == plugin.Id).Select(FromDbFile);
    }

    public CollectionFile Create(
      [NotNull] ISMAPlugin     requester,
      int                      elementId,
      [NotNull] Action<Stream> streamWriter,
      string                   extension,
      string                   crc32 = null)
    {
      if (elementId <= 0)
        return null;

      CollectionFSFile dbFile = null;

      try
      {
        extension = extension?.TrimStart('.');

        dbFile = new CollectionFSFile
        {
          ElementId = elementId,
          Extension = extension ?? string.Empty,
          PluginId  = requester.Id
        };

        dbFile.Id = DbFiles.Insert(dbFile).AsInt32;

        CollectionFile colFile = FromDbFile(dbFile);

        DirectoryEx.EnsureExists(Path.GetDirectoryName(colFile.Path));

        using (var stream = File.Open(colFile.Path,
                                      System.IO.FileMode.Create,
                                      FileAccess.ReadWrite))
          streamWriter(stream);

        if (crc32 != null)
        {
          var fsCrc32 = FileEx.GetCrc32(colFile.Path);

          if (fsCrc32 != crc32)
            throw new IOException($"CRC32 did not match for file {colFile.Path}. Expected {crc32}, got {fsCrc32}");
        }

        return colFile;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "CollectionFS: Create threw an exception.");

        try
        {
          if (dbFile != null)
            DbFiles.Delete(dbFile.Id);
        }
        catch (Exception dbEx)
        {
          LogTo.Error(dbEx,
                      "CollectionFS: Create threw an exception. Exception's DB cleanup code threw an exception");
        }

        throw;
      }
    }

    /// <inheritdoc />
    public bool DeleteById(int id)
    {
      try
      {
        CollectionFSFile dbFile = DbFiles.FindById(id);

        if (dbFile == null)
          return false;

        File.Delete(GetFilePath(dbFile));

        DbFiles.Delete(dbFile.Id);

        return true;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "CollectionFS: DeleteById threw an exception.");
        return false;
      }
    }

    /// <inheritdoc />
    public int DeleteByElementId(int        elementId,
                                 ISMAPlugin plugin = null)
    {
      Query query = Query.EQ("ElementId",
                             elementId);

      if (plugin != null)
        query = Query.And(query,
                          Query.EQ("PluginId",
                                   plugin.Id));

      return Delete(DbFiles.Find(query));
    }

    /// <inheritdoc />
    public int DeleteByPlugin([NotNull] ISMAPlugin plugin)
    {
      return Delete(DbFiles.Find(f => f.PluginId == plugin.Id));
    }

    #endregion




    #region Methods
    
    public void Dispose()
    {
      DbFiles = null;
    }

    /// <inheritdoc />
    public string GetPluginResourcePath(ISMAPlugin plugin)
    {
      var path = SMA.Collection.GetSMAPluginsFolder(plugin);

      DirectoryEx.EnsureExists(path);

      return path;
    }

    public int PruneOrphans()
    {
      int count    = 0;
      var allFiles = DbFiles.FindAll();

      foreach (var group in allFiles.GroupBy(f => f.ElementId))
      {
        var element = SMA.Registry.Element[group.Key];

        if (element != null)
          continue;

        count += Delete(group);
      }

      return count;
    }

    private int Delete(IEnumerable<CollectionFSFile> dbFiles)
    {
      var toDelete = new List<CollectionFSFile>(dbFiles.Count());

      foreach (var file in dbFiles)
        try
        {
          File.Delete(GetFilePath(file));

          toDelete.Add(file);
        }
        catch (Exception ex)
        {
          LogTo.Error(ex,
                      "Failed to delete {file}",
                      file);
        }

      return DbFiles.Delete(Query.In("Id",
                                     toDelete.Select(f => new BsonValue(f.Id))));
    }

    private CollectionFile FromDbFile(CollectionFSFile dbFile)
    {
      string filePath = GetFilePath(dbFile);

      return new CollectionFile
      {
        Id        = dbFile.Id,
        ElementId = dbFile.ElementId,
        Path      = filePath
      };
    }

    private void LoadDb()
    {
      DbFiles = SystemDb.Instance.GetCollection<CollectionFSFile>();
      DbFiles.EnsureIndex(f => f.ElementId);
    }

    private string GetFilePath(CollectionFSFile dbFile)
    {
      return SMA.Collection.GetSMAElementsFilePath(dbFile.ElementId,
                                                   $"{dbFile.Id}{GetFileExtension(dbFile.Extension)}");
    }

    private string GetFileExtension(string extension)
    {
      return String.IsNullOrWhiteSpace(extension)
        ? string.Empty
        : $".{extension}";
    }

    #endregion
  }
}
