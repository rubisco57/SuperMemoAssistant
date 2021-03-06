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
// Created On:   2018/05/12 18:26
// Modified On:  2019/01/05 04:07
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Threading;
using Anotar.Serilog;
using Process.NET;
using Process.NET.Assembly;
using Process.NET.Memory;
using Process.NET.Utilities;
using SuperMemoAssistant.Hooks;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo.Hooks;

namespace SuperMemoAssistant.SuperMemo
{
  /// <summary>Convenience class that implements helpers</summary>
  public abstract class SuperMemoBase
    : IDisposable,
      ISuperMemo,
      ISMHookSystem
  {
    #region Properties & Fields - Non-Public

    private IPointer IgnoreUserConfirmationPtr { get; set; }


    protected ExecutionContext ExecCtxt { get; } = new ExecutionContext();

    private int WndProcHookAddr { get; set; }

    #endregion




    #region Constructors

    protected SuperMemoBase(SMCollection collection)
    {
      Collection = collection;

      OnPreInit();

      SMProcess = SMHookEngine.Instance.CreateAndHook(
        collection,
        this,
        GetIOCallbacks()
      );

      SMProcess.Native.Exited += OnSMExited;

      OnPostInit();

      SMHookEngine.Instance.SignalWakeUp();
    }

    public virtual void Dispose()
    {
      IgnoreUserConfirmationPtr = null;

      SMProcess.Native.Exited -= OnSMExited;

      try
      {
        SMHookEngine.Instance.CleanupHooks();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "Failed to cleanup SMHookEngine");
      }

      try
      {
        OnSMStoppedEvent?.Invoke(this,
                                 new SMProcessArgs(this,
                                                   SMProcess));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex,
                    "An exception occured in one of OnSMStoppedEvent handlers");
      }
    }

    #endregion




    #region Properties & Fields - Public

    public ManualResetEvent MainThreadReady { get; } = new ManualResetEvent(false);

    #endregion




    #region Properties Impl - Public

    public SMCollection Collection { get; }
    public IProcess     SMProcess  { get; }
    public bool IgnoreUserConfirmation
    {
      get => IgnoreUserConfirmationPtr.Read<bool>();
      set => IgnoreUserConfirmationPtr.Write<bool>(0,
                                                   value);
    }
    public ISuperMemoRegistry Registry => SuperMemoRegistry.Instance;
    public ISuperMemoUI       UI       => SuperMemoUI.Instance;

    #endregion




    #region Methods Impl

    //
    // SM Hook

    public virtual void OnException(Exception ex)
    {
      // TODO: Notify ?
      LogTo.Error(ex,
                  "Exception caught in InjectLib.");
    }

    /// <inheritdoc />
    public void SetWndProcHookAddr(int addr)
    {
      WndProcHookAddr = addr;
    }

    /// <param name="wParam"></param>
    public bool OnUserMessage(int wParam)
    {
      switch ((InjectLibMessages)wParam)
      {
        case InjectLibMessages.ExecuteOnMainThread:
          //MainThreadReady.Set();

          return true;
      }

      return false;
    }

    public void GetExecutionParameters(out int       method,
                                       out dynamic[] parameters)
    {
      method     = (int)ExecCtxt.ExecutionMethod;
      parameters = ExecCtxt.ExecutionParameters;
    }

    public void SetExecutionResult(int result)
    {
      ExecCtxt.ExecutionResult = result;
      MainThreadReady.Set();
    }

    public Dictionary<string, int> GetPatternsHintAddresses()
    {
      throw new NotImplementedException();
    }

    public void SetPatternsHintAddresses(Dictionary<string, int> hintAddrs)
    {
      throw new NotImplementedException();
    }

    #endregion




    #region Methods

    // TODO: Not thread safe & ugly overall...
    public int ExecuteOnMainThread(NativeMethod     method,
                                   params dynamic[] parameters)
    {
      MainThreadReady.Reset();

      ExecCtxt.ExecutionResult     = 0;
      ExecCtxt.ExecutionMethod     = method;
      ExecCtxt.ExecutionParameters = parameters;

      var smMain = SMProcess.Memory.Read<int>(SMNatives.TSMMain.InstancePtr);
      var handle = SMProcess.Memory.Read<IntPtr>(new IntPtr(smMain + SMNatives.TControl.HandleOffset));

      var restoreWndProcAddr = SMNatives.TApplication.TApplicationOnMessagePtr.Read<int>(SMProcess.Memory);
      SMNatives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
                                                                 WndProcHookAddr);

      WindowHelper.PostMessage(handle,
                               2345,
                               new IntPtr((int)InjectLibMessages.ExecuteOnMainThread),
                               new IntPtr(0));

      SMA.Instance.SMMgmt.MainThreadReady.WaitOne(AssemblyFactory.ExecutionTimeout);

      SMNatives.TApplication.TApplicationOnMessagePtr.Write<int>(SMProcess.Memory,
                                                                 restoreWndProcAddr);

      ExecCtxt.ExecutionParameters = null;

      return ExecCtxt.ExecutionResult;
    }

    //
    // SM-App Lifecycle

    protected virtual void OnSMExited(object    called,
                                      EventArgs args)
    {
      Dispose();
    }

    protected virtual void OnPreInit()
    {
      SMA.Instance.OnSMStartingImpl(this);
    }

    protected virtual void OnPostInit()
    {
      SMA.Instance.OnSMStartedImpl();

      IgnoreUserConfirmationPtr = SMProcess[SMNatives.Globals.IgnoreUserConfirmationPtr];
    }

    #endregion




    #region Methods Abs

    protected abstract IEnumerable<ISMHookIO> GetIOCallbacks();


    //
    // UI Automation Core
    /*
    protected FocusChangedEventHandlerBase FocusChangedHandler { get; set; }

    protected virtual void SetupUI()
    {
      SMApp = Application.Attach(SMProcess);

    }

    protected virtual void CleanupUIAutomation()
    {
      UnregisterUIAutomationEvents();
    }

    private virtual void RegisterUIAutomationEvents()
    {
      FocusChangedHandler = UIAuto.RegisterFocusChangedEvent(OnFocusChangedGlobal);

      // WindowOpenedEvent
      RegisterAutomationEvent(
        UIAuto.EventLibrary.Window.WindowOpenedEvent,
        TreeScope.Children,
        IsSMProcess,
        OnWindowOpened,
        WrapAutomationEvent(OnWindowOpenedEvent));

      // WindowClosedEvent
      RegisterAutomationEvent(
        UIAuto.EventLibrary.Window.WindowClosedEvent,
        TreeScope.Children,
        IsSMProcess,
        OnWindowClosed,
        WrapAutomationEvent(OnWindowClosedEvent));
    }

    private override void UnregisterUIAutomationEvents()
    {
      UIAuto.UnregisterFocusChangedEvent(FocusChangedHandler);
      FocusChangedHandler = null;

      base.UnregisterUIAutomationEvents();
    }

    protected virtual void OnFocusChangedGlobal(AutomationElement elem)
    {
      if (elem.Properties.ProcessId == SMProcess.Id)
        OnFocusChanged(elem);
    }



    //
    // UI Automation Events

    /// <summary>
    /// Notification for a SM-related focus changed event.
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="eventId"></param>
    protected abstract void OnFocusChanged(AutomationElement elem);

    /// <summary>
    /// Notification for a SM-related window open event.
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="eventId"></param>
    protected abstract void OnWindowOpened(AutomationElement elem, EventId eventId);
    /// <summary>
    /// Notification for a SM-related window close event.
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="eventId"></param>
    protected abstract void OnWindowClosed(AutomationElement elem, EventId eventId);
    */


    //
    // ISuperMemo Methods

    public abstract SMAppVersion AppVersion { get; }

    #endregion




    #region Events

    public event EventHandler<SMProcessArgs> OnSMStoppedEvent;

    #endregion




    protected class ExecutionContext
    {
      #region Properties & Fields - Public

      public NativeMethod ExecutionMethod     { get; set; }
      public dynamic[]    ExecutionParameters { get; set; }
      public int          ExecutionResult     { get; set; }

      #endregion
    }
  }
}
