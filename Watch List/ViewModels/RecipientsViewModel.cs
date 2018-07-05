using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Watch_List.Classes;
using System.Collections.ObjectModel;

namespace Watch_List.ViewModels
{
    [POCOViewModel]
    public class RecipientsViewModel
    {
        #region Properties
        protected IDialogService DialogService { get { return this.GetService<IDialogService>(); } }
        protected IMessageBoxService MessageBoxService { get { return this.GetRequiredService<IMessageBoxService>(); } }

        public virtual ObservableCollection<Recipients> Recipients { get; set; }
        public virtual RecipientsCollection RecipientsCollection { get; set; }
        public List<UICommand> DialogCommands { get; private set; }
        public UICommand OKCommand { get; private set; }
        public UICommand CancelCommand { get; private set; }
        public virtual bool AllowCloseDialog { get; set; }
        #endregion

        public static RecipientsViewModel Create()
        {
            return ViewModelSource.Create(() => new RecipientsViewModel());
        }

        protected RecipientsViewModel()
        {
            this.AllowCloseDialog = true;
            this.DialogCommands = new List<UICommand>();

            this.OKCommand = new UICommand()
            {
                Caption = "Ok",
                Command = new DelegateCommand<CancelEventArgs>(Ok, CanOk),
                Id = MessageBoxResult.OK,
                IsDefault = true
            };
            this.DialogCommands.Add(OKCommand);

            this.CancelCommand = new UICommand()
            {
                Caption = "Cancel",
                Command = new DelegateCommand<CancelEventArgs>(Cancel),
                IsCancel = true,
                Id = MessageBoxResult.Cancel,
            };
            this.DialogCommands.Add(CancelCommand);

            this.RecipientsCollection = new RecipientsCollection();
            this.RecipientsCollection.LoadAll();
            this.Recipients = new ObservableCollection<Watch_List.Recipients>();
            foreach (Recipients recipient in this.RecipientsCollection) 
            {
                this.Recipients.Add(recipient);
            }
        }

        void Ok(CancelEventArgs parameter)
        {
            if (!this.AllowCloseDialog)
                parameter.Cancel = true;
        }

        bool CanOk(CancelEventArgs parameter)
        {
            return true;
        }

        void Cancel(CancelEventArgs parameter)
        {
            if (!this.AllowCloseDialog)
                parameter.Cancel = true;
        }

        public void AddRecipient()
        {
            try
            {
                var vm = RecipientViewModel.Create();

                var result = this.DialogService.ShowDialog(
                   dialogCommands: vm.DialogCommands,
                   title: "Recipients",
                   documentType: "RecipientView",
                   parameter: null,
                   parentViewModel: this,
                   viewModel: vm);

                if (result != null && result.IsDefault)
                {
                    var recipient = new Recipients();
                    recipient.EmailAddress = vm.EmailAddress;
                    recipient.Save();
                    this.Recipients.Add(recipient);
                }
            }
            catch (Exception ex)
            {
                this.MessageBoxService.ShowMessage(ex.Message, "Error");
            }
        }

        public bool CanAddRecipient()
        {
            return true;
        }

        public void EditRecipient(Recipients recipient)
        {
            try
            {
                var vm = RecipientViewModel.Create();
                vm.EmailAddress = recipient.EmailAddress;

                var result = this.DialogService.ShowDialog(
                   dialogCommands: vm.DialogCommands,
                   title: "Recipients",
                   documentType: "RecipientView",
                   parameter: null,
                   parentViewModel: this,
                   viewModel: vm);

                if (result != null && result.IsDefault)
                {
                    recipient.EmailAddress = vm.EmailAddress;
                    recipient.Save();
                }
            }
            catch (Exception ex)
            {
                this.MessageBoxService.ShowMessage(ex.Message, "Error");
            }
        }

        public bool CanEditRecipient(Recipients recipient)
        {
            return recipient != null;
        }

        public void DeleteRecipient(Recipients recipient)
        {
            if (this.MessageBoxService.ShowMessage("Are you sure you wish to delete the selected recipient?", "Confirm", MessageButton.YesNo, DevExpress.Mvvm.MessageIcon.Question) == DevExpress.Mvvm.MessageResult.Yes)
            {
                recipient.MarkAsDeleted();
                this.RecipientsCollection.Save();
                this.Recipients.Remove(recipient);
                this.RaisePropertyChanged(x => x.Recipients);
            }
        }

        public bool CanDeleteRecipient(Recipients recipient)
        {
            return recipient != null;
        }
    }
}