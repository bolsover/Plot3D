using System;
using System.Windows.Forms;
using AlibreAddOn;
using AlibreX;
using Plot3D.UI.Alibre;

// ReSharper disable once CheckNamespace
namespace AlibreAddOnAssembly
{
    /// <summary>
    /// Entry points for a managed (.adc <DLL type="Managed">) add-on. Alibre
    /// looks for this exact type name, AlibreAddOnAssembly.AlibreAddOn, and calls
    /// its static methods — the managed counterpart of the C++ exports
    /// AddOnLoad / AddOnInvoke / AddOnUnload / GetAddOnInterface.
    /// </summary>
    public static class AlibreAddOn
    {
        private static IADRoot? _root;
        private static Plot3DAddOn? _addOn;

        public static void AddOnLoad(IntPtr hwnd, IAutomationHook pAutomationHook, IntPtr unused)
        {
            try
            {
                _root = (IADRoot)pAutomationHook.Root;
                _addOn = new Plot3DAddOn(_root, hwnd);
            }
            catch (Exception e)
            {
                MessageBox.Show(Plot3DAddOn.Title + " failed to load:\n\n" + e.Message,
                    Plot3DAddOn.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static IADRoot? GetRoot()
        {
            return _root;
        }

        public static void AddOnInvoke(IntPtr hwnd, IntPtr pAutomationHook, string sessionName, bool isLicensed, int reserved1, int reserved2)
        {
            _addOn?.InvokeCommand(Plot3DAddOn.MenuIdCommand, sessionName);
        }

        public static void AddOnUnload(IntPtr hwnd, bool forceUnload, ref bool cancel, int reserved1, int reserved2)
        {
            _addOn = null;
            _root = null;
        }

        public static IAlibreAddOn? GetAddOnInterface()
        {
            return _addOn;
        }
    }
}
