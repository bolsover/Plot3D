using System;
using System.Windows.Forms;
using AlibreAddOn;
using AlibreX;

namespace Plot3D.UI.Alibre
{
    /// <summary>
    /// Implements IAlibreAddOn to integrate Plot3D into Alibre Design.
    /// </summary>
    public class Plot3DAddOn : IAlibreAddOn
    {
        public const string Title = "Plot3D Studio";
        public const int MenuIdRoot = 40300;

        public Array SubMenuItems(int menuId) => null;

        private readonly IADRoot _root;
        private readonly IntPtr _parentWindow;
        private readonly int _build;
        private MainForm? _form;

        public Plot3DAddOn(IADRoot root, IntPtr parentWindow)
        {
            _root = root;
            _parentWindow = parentWindow;
            _build = AlibreVersion.GetBuild(root);
        }

        public IADRoot Root => _root;
        public IntPtr ParentWindow => _parentWindow;
        private bool IsSupportedVersion => _build >= AlibreVersion.MinimumBuild;

        public int RootMenuItem => MenuIdRoot;

        // The root menu item is a single leaf command: no sub menus, not a popup.
        public bool HasSubMenus(int menuId) => false;


        public string MenuItemText(int menuId) => menuId == MenuIdRoot ? Title : string.Empty;

        public bool PopupMenu(int menuId) => false;

        public ADDONMenuStates MenuItemState(int menuId, string sessionIdentifier)
        {
            return ADDONMenuStates.ADDON_MENU_ENABLED;
        }

        public string MenuItemToolTip(int menuId)
        {
            return "Open Plot3D to create 3D parametric curves and generate 3D sketches in the active Part.";
        }

        public string MenuIcon(int menuId) => "Plot3D.ico";

        public bool HasPersistentDataToSave(string sessionIdentifier) => false;

        public IAlibreAddOnCommand? InvokeCommand(int menuId, string sessionIdentifier)
        {
            ShowWindow(sessionIdentifier);

            return null;
        }

        public void ShowWindow(string? sessionIdentifier = null)
        {
            try
            {
                if (!IsSupportedVersion)
                {
                    MessageBox.Show(MenuItemToolTip(MenuIdRoot), Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_form != null && !_form.IsDisposed)
                {
                    if (_form.WindowState == FormWindowState.Minimized)
                        _form.WindowState = FormWindowState.Normal;
                    _form.Activate();
                    return;
                }

                _form = new MainForm(_root, _parentWindow);
                _form.FormClosed += (s, e) => _form = null;
                if (_parentWindow != IntPtr.Zero)
                {
                    _form.Show(new WindowHandle(_parentWindow));
                }
                else
                {
                    _form.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadData(IStream pCustomData, string sessionIdentifier)
        {
        }

        public void SaveData(IStream pCustomData, string sessionIdentifier)
        {
        }

        public void setIsAddOnLicensed(bool isLicensed)
        {
        }

        public bool UseDedicatedRibbonTab() => false;
    }

    /// <summary>
    /// Simple IWin32Window wrapper for native window handles.
    /// </summary>
    internal sealed class WindowHandle : IWin32Window
    {
        public IntPtr Handle { get; }
        public WindowHandle(IntPtr handle) => Handle = handle;
    }
}