using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;
using Xamarin.Forms;

namespace SampleFormsApp
{
    public partial class SampleFormsAppPage : ContentPage
    {
        public SampleFormsAppPage()
        {
            InitializeComponent();
        }

		void Handle_Clicked(object sender, System.EventArgs e)
		{
			Task.Run(async () =>
			{
				await Task.Delay(100);
				var address = "AC:3F:A4:5B:1E:7E";
				Print(address);
			});
		}

		IConnection connection;

		private void Print(string address)
		{
			string zpl = "^XA^LL200^FO30,20^A0N,30,30^FDHello World^FS^XZ";

			try
			{
				if ((connection == null) || (!connection.IsConnected))
				{
					connection = ConnectionBuilder.Current.Build("BT:" + address);
					connection.Open();
				}
				if ((SetPrintLanguage(connection)) && (CheckPrinterStatus(connection)))
				{
					connection.Write(new UTF8Encoding().GetBytes(zpl));
				}
			}
			catch (System.Exception e)
			{
				//if the device is unable to connect, an exception is thrown
				Debug.WriteLine(e.ToString());
			}
		}

		private bool SetPrintLanguage(IConnection conn)
		{
			string setLanguage = "! U1 setvar \"device.languages\" \"zpl\"\r\n\r\n! U1 getvar \"device.languages\"\r\n\r\n";
			byte[] response = connection.SendAndWaitForResponse(new UTF8Encoding().GetBytes(setLanguage), 500, 500);
			string s = new UTF8Encoding().GetString(response, 0, response.Length);
			if (!s.Contains("zpl"))
			{
				Debug.WriteLine("Not a ZPL printer.");
				return false;
			}
			return true;
		}

		private bool CheckPrinterStatus(IConnection conn)
		{
			IZebraPrinter printer = ZebraPrinterFactory.Current.GetInstance(PrinterLanguage.ZPL, conn);
			IPrinterStatus status = printer.CurrentStatus;
			if (!status.IsReadyToPrint)
			{
				Debug.WriteLine("Printer in Error: " + status.ToString());
			}
			return true;
		}
    }
}
