using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.Core;
using Tiraggo.Core;
using Tiraggo.Interfaces;

namespace Watch_List
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnAppStartup_UpdateThemeName(object sender, StartupEventArgs e)
        {
            DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();

            this.SetConnections();
        }

        void SetConnections()
        {
            Tiraggo.Interfaces.tgProviderFactory.Factory = new Tiraggo.Loader.tgDataProviderFactory();
            tgEntity.ConvertEmptyStringToNull = true;

            tgConnectionElement conn = new tgConnectionElement();

            conn.ConnectionString = "Data Source=database.db; Version=3;";
            conn.Name = "SQLite";
            conn.Provider = "Tiraggo.SQLiteProvider";
            conn.ProviderClass = "DataProvider";
            conn.SqlAccessType = tgSqlAccessType.DynamicSQL;
            conn.ProviderMetadataKey = "tgDefault";

            tgConfigSettings.ConnectionInfo.Connections.Add(conn);
            tgConfigSettings.ConnectionInfo.Default = "SQLite";
        }
    }
}
