﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Caliburn.Micro;

namespace Fibertest.WpfCommonViews
{
    public static class ReportExt
    {
        public static void ShowReport(this string fileName)
        {
            try
            {
                Process.Start(fileName);
            }
            catch (Exception e)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>(){ @"Failed to show report:", e.Message}, 1);
                new WindowManager().ShowDialogWithAssignedOwner(vm).Wait();
            }
        }
    }
}
