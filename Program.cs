using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SlimDX.Windows;



namespace DirectX_Base
{
	static class Program
	{
		private static MainForm sMainForm;

		private static bool spRendered { get; set; }



		public static void Render()
		{
			spRendered = false;
		}



		[STAThread]
		static void Main()
		{
			spRendered = false;

			sMainForm = new MainForm();

			MessagePump.Run(sMainForm, () =>
			{
				Update();
			});


		}



		public static void Update()
		{
			if (sMainForm == null)
				return;

			if (!spRendered)
			{
				sMainForm.Draw();

				spRendered = true;
			}
		}
	}
}
