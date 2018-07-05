using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Watch_List.ViewModels
{
    [POCOViewModel(ImplementIDataErrorInfo = true)]
    public class PreferencesViewModel
    {
        #region Properties
        public List<UICommand> DialogCommands { get; private set; }
        public UICommand OKCommand { get; private set; }
        public UICommand CancelCommand { get; private set; }
        public virtual bool AllowCloseDialog { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual string Password { get; set; }
        #endregion

        public static PreferencesViewModel Create()
        {
            return ViewModelSource.Create(() => new PreferencesViewModel());
        }

        public static void BuildMetadata(MetadataBuilder<PreferencesViewModel> builder)
        {
            builder.Property(x => x.EmailAddress).Required(() => "Email Address Required");
            builder.Property(x => x.Password).Required(() => "Password Required");
        }

        protected PreferencesViewModel()
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
        }

        void Ok(CancelEventArgs parameter)
        {
            if (!this.AllowCloseDialog)
                parameter.Cancel = true;
        }

        bool CanOk(CancelEventArgs parameter)
        {
            return String.IsNullOrWhiteSpace(IDataErrorInfoHelper.GetErrorText(this, "EmailAddress"))
                && String.IsNullOrWhiteSpace(IDataErrorInfoHelper.GetErrorText(this, "Password"));
        }

        void Cancel(CancelEventArgs parameter)
        {
            if (!this.AllowCloseDialog)
                parameter.Cancel = true;
        }
    }
}