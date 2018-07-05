using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using TVMazeAPI;

namespace Watch_List.ViewModels
{
    [POCOViewModel]
    public class LookupViewModel
    {
        #region Properties
        public List<UICommand> DialogCommands { get; private set; }
        public UICommand OKCommand { get; private set; }
        public UICommand CancelCommand { get; private set; }
        public virtual bool AllowCloseDialog { get; set; }
        public virtual string LookupValue { get; set; }
        public virtual string ShowId { get; set; }
        public virtual string ShowImage { get; set; }
        public virtual string ShowName { get; set; }
        public virtual string NetworkName { get; set; }
        public virtual string Synopsis { get; set; }
        public virtual string NextAirDate { get; set; }
        #endregion

        public static LookupViewModel Create()
        {
            return ViewModelSource.Create(() => new LookupViewModel());
        }

        protected LookupViewModel()
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
                Command = new  DelegateCommand<CancelEventArgs>(Cancel),
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
            return true;
        }

        void Cancel(CancelEventArgs parameter)
        {
            if (!this.AllowCloseDialog)
                parameter.Cancel = true;
        }

        public async void Search()
        {
            var series = await TVMaze.TVMaze.FindSingleSeries(LookupValue, FetchEpisodes: true);

            if (series.status != "404")
            {
                string typeofSeries = "";
                if (series.webChannel == null) typeofSeries = $"Network: {series.network.name}({series.network.country.code}) \n";
                else if (series.network == null) typeofSeries = $"WebChannel: {series.webChannel.name}\n";

                string nextEpisode = "N/A";

                foreach (var episode in series.Episodes)
                {
                    if (episode.airdate >= DateTime.Today)
                    {
                        var convertedDate = DateTime.SpecifyKind(
                            DateTime.Parse(episode.airstamp.ToString()),
                            DateTimeKind.Utc);

                        nextEpisode = episode.ToString() + " - " + convertedDate.ToString() + " (" + episode.name + ")";
                        NextAirDate = convertedDate.ToString("MM/dd/yy") + " " + convertedDate.ToShortTimeString();
                        break;
                    }
                }

                this.ShowId = series.id.ToString();
                this.ShowName = series.name;
                this.NetworkName = $"{typeofSeries}{series.Episodes.Count} Episodes \nFirst Aired: {Convert.ToDateTime(series.premiered).ToShortDateString()} \nRuntime: {series.runtime} minutes.\nNext Episode: {nextEpisode}";
                this.Synopsis = series.summary;
                this.ShowImage = series.image.medium.AbsoluteUri.ToString();
            }
            else
            {
                //splashScreenManager1.CloseWaitForm();
                //btnAdd.Enabled = false;
                //pnlMain.Visible = false;
                //MessageBox.Show("Series not found.", txtName.Text);
            }
        }

        public bool CanSearch()
        {
            return !String.IsNullOrEmpty(LookupValue);
        }
    }
}