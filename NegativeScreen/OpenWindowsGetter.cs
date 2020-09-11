using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using static NegativeScreen.NativeMethods;

namespace NegativeScreen
{
	// sources:
	//	https://www.tcx.be/blog/2006/list-open-windows/
	//  and https://stackoverflow.com/questions/41293761/how-to-find-window-handle-from-exe-files-name

	using HWND = IntPtr;

	/// <summary>Contains functionality to get all the open windows.</summary>
	public static class OpenWindowGetter
	{
		/// <summary>Returns a dictionary that contains the handle and title and the full path and file name of the window.</summary>
		/// <returns>A dictionary that contains the handle and a tuple which holds the title and the full path and file name of all the open windows. (It seems like the dictionary is automatically sorted by the z order of the windows.)</returns>
		public static List<Window> GetOpenWindows()
		{
			HWND shellWindow = GetShellWindow();
			List<Window> windows = new List<Window>();

			EnumWindows(delegate (HWND hWnd, int lParam)
			{
				if (hWnd == shellWindow) return true;
				if (!IsWindowVisible(hWnd)) return true;

				int length = GetWindowTextLength(hWnd);
				if (length == 0) return true;

				uint processId;
				uint threadID = GetWindowThreadProcessId(hWnd, out processId);

				HWND processHandle = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, (int) processId);

				StringBuilder processFileNameBuilder = new StringBuilder(256);
				GetProcessImageFileName(processHandle, processFileNameBuilder, processFileNameBuilder.Capacity);

				windows.Add(new Window() {
					Handle = hWnd,
					Path = processFileNameBuilder.ToString()
				});
				return true;
			}, 0);

			return windows;
		}

		public class Window
		{
			public HWND Handle;
			public string Path;
			public string Title {
				get
				{
					int length = GetWindowTextLength(Handle);
					if (length == 0) return "";
					StringBuilder windowTextBuilder = new StringBuilder(length);
					GetWindowText(Handle, windowTextBuilder, length + 1);
					return windowTextBuilder.ToString();
				}
			}
			public string FileName
			{
				get => System.IO.Path.GetFileNameWithoutExtension(this.Path);
			}
			public string Summary
			{
				get => FileName + " (" + Title + ")";
			}
			public override string ToString()
			{
				return this.Summary;
			}
		}
	}
}
