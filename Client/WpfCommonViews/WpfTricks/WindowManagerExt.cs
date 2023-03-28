using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Fibertest.WpfCommonViews
{
    public static class WindowManagerExt
    {
        public static async Task<bool?> ShowDialogWithAssignedOwner(this IWindowManager windowManager, object model)
        {
            var setting = new Dictionary<string, object>()
            {
                [@"Owner"] = Application.Current.MainWindow!
            };
            return await windowManager.ShowDialogAsync(model, null, setting);
        }

        public static async Task ShowWindowWithAssignedOwner(this IWindowManager windowManager, object model)
        {
            var setting = new Dictionary<string, object>()
            {
                [@"Owner"] = Application.Current.MainWindow!
            };
            await windowManager.ShowWindowAsync(model, null, setting);
        }
    }
}