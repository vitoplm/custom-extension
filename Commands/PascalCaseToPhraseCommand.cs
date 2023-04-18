using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;

namespace PhraseToMethod
{

	internal sealed class PascalCaseToPhrase
	{
		public const int CommandId = 0x0101;

		public static readonly Guid CommandSet = new("bea4976d-811a-4dd0-9745-0bed7a658b5d");

		private readonly Package package;

		private PascalCaseToPhrase(Package package)
		{
			this.package = package ?? throw new ArgumentNullException("package");

			OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService != null)
			{
				var menuCommandID = new CommandID(CommandSet, CommandId);
				var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
				commandService.AddCommand(menuItem);
			}
		}

		public static PascalCaseToPhrase Instance
		{
			get;
			private set;
		}

		private IServiceProvider ServiceProvider
		{
			get
			{
				return this.package;
			}
		}

		public static void Initialize(Package package)
		{
			Instance = new PascalCaseToPhrase(package);
		}

		private async void MenuItemCallback(object sender, EventArgs e)
		{
			var docView = await VS.Documents.GetActiveDocumentViewAsync();

			var selection = docView?.TextView.Selection.SelectedSpans.FirstOrDefault();

			if (selection.HasValue)
			{
				var value = selection.Value.Snapshot.GetText(selection.Value.Span);
				if(value.Contains(" "))
					VsShellUtilities.ShowMessageBox(
						 this.ServiceProvider,
						 "Invalid input",
						 "Error",
						 OLEMSGICON.OLEMSGICON_WARNING,
						 OLEMSGBUTTON.OLEMSGBUTTON_OK,
						 OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				else
					docView?.TextBuffer.Replace(selection.Value, PascalCaseToPhraseConverter(value));

			}
		}

		private string PascalCaseToPhraseConverter(string input)
		{
			String output = "";
			for (int i = 0; i < input.Length; i++)
			{
				if (i > 0 && char.IsUpper(input[i]))
				{
					output += " ";
				}
				output += input[i];
			}
			return output;
		}
	}
}
