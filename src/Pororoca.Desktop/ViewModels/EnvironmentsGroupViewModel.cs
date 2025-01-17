using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using ReactiveUI;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.ImportEnvironment.PororocaEnvironmentImporter;
using static Pororoca.Domain.Features.ImportEnvironment.PostmanEnvironmentImporter;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using Avalonia.Controls;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class EnvironmentsGroupViewModel : CollectionOrganizationItemParentViewModel<EnvironmentViewModel>
    {
        #region COLLECTION ORGANIZATION

        public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;
        public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
        public ReactiveCommand<Unit, Unit> AddNewEnvironmentCmd { get; }
        public ReactiveCommand<Unit, Unit> PasteToEnvironmentsCmd { get; }

        #endregion

        #region ENVIRONMENTS GROUP

        public override ObservableCollection<EnvironmentViewModel> Items { get; }
        public ReactiveCommand<Unit, Unit> ImportEnvironmentsCmd { get; }

        #endregion

        #region OTHERS

        private readonly bool isOperatingSystemMacOsx;

        #endregion

        public EnvironmentsGroupViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                          IEnumerable<PororocaEnvironment> envs,
                                          Func<bool>? isOperatingSystemMacOsx = null) : base(parentVm, string.Empty)
        {
            #region OTHERS
            this.isOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
            #endregion
            
            #region COLLECTION ORGANIZATION
            AddNewEnvironmentCmd = ReactiveCommand.Create(AddNewEnvironment);
            PasteToEnvironmentsCmd = ReactiveCommand.Create(PasteToThis);
            ImportEnvironmentsCmd = ReactiveCommand.CreateFromTask(ImportEnvironmentsAsync);
            #endregion

            #region ENVIRONMENTS GROUP
            Items = new(envs.Select(e => new EnvironmentViewModel(this, e, SetEnvironmentAsCurrent)));
            RefreshSubItemsAvailableMovements();
            #endregion
        }

        #region COLLECTION ORGANIZATION

        public override void RefreshSubItemsAvailableMovements()
        {
            for (int x = 0; x < Items.Count; x++)
            {
                EnvironmentViewModel envVm = Items[x];
                bool canMoveUp = x > 0;
                bool canMoveDown = x < Items.Count - 1;
                envVm.CanMoveUp = canMoveUp;
                envVm.CanMoveDown = canMoveDown;
            }
        }

        public void AddNewEnvironment()
        {
            PororocaEnvironment newEnv = new(Localizer.Instance["Environment/NewEnvironment"]);
            AddEnvironment(newEnv);
        }

        private void AddEnvironment(PororocaEnvironment envToAdd)
        {
            EnvironmentViewModel envToAddVm = new(this, envToAdd, SetEnvironmentAsCurrent);
            Items.Add(envToAddVm);
            RefreshSubItemsAvailableMovements();
        }

        protected override void CopyThis() =>
            throw new NotImplementedException();

        public override void PasteToThis()
        {
            ICloneable? itemToPaste = ClipboardAreaDataCtx.FetchCopy();
            if (itemToPaste is PororocaEnvironment envToPaste)
                AddEnvironment(envToPaste);
        }

        #endregion

        #region ENVIRONMENTS GROUP

        private void SetEnvironmentAsCurrent(EnvironmentViewModel envVm)
        {
            foreach (EnvironmentViewModel evm in Items)
            {
                evm.IsCurrentEnvironment = evm == envVm;
            }
        }

        public IEnumerable<PororocaEnvironment> ToEnvironments() =>
            Items.Select(e => e.ToEnvironment());

        public async Task ImportEnvironmentsAsync()
        {
            List<FileDialogFilter> fileSelectionfilters = new();
            // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
            if (!isOperatingSystemMacOsx)
            {
                fileSelectionfilters.Add(
                    new()
                    {
                        Name = Localizer.Instance["Collection/ImportEnvironmentDialogTypes"],
                        Extensions = new List<string> { PororocaEnvironmentExtension, PostmanEnvironmentExtension }
                    }
                );
            }

            OpenFileDialog dialog = new()
            {
                Title = Localizer.Instance["Collection/ImportEnvironmentDialogTitle"],
                AllowMultiple = true,
                Filters = fileSelectionfilters
            };
            string[]? result = await dialog.ShowAsync(MainWindow.Instance!);
            if (result != null)
            {
                foreach (string envFilePath in result)
                {
                    // First, tries to import as a Pororoca environment
                    if (envFilePath.EndsWith(PororocaEnvironmentExtension))
                    {
                        string fileContent = await File.ReadAllTextAsync(envFilePath, Encoding.UTF8);
                        if (TryImportPororocaEnvironment(fileContent, out PororocaEnvironment? importedPororocaEnvironment))
                        {
                            importedPororocaEnvironment!.IsCurrent = false; // Imported environment should always be disabled
                            AddEnvironment(importedPororocaEnvironment!);
                        }
                    }
                    // If not a valid Pororoca collection, then tries to import as a Postman collection
                    else if (envFilePath.EndsWith(PostmanEnvironmentExtension))
                    {
                        string fileContent = await File.ReadAllTextAsync(envFilePath, Encoding.UTF8);
                        if (TryImportPostmanEnvironment(fileContent, out PororocaEnvironment? importedPostmanEnvironment))
                        {
                            AddEnvironment(importedPostmanEnvironment!);
                        }
                    }
                }
            }
        }

        #endregion
    }
}