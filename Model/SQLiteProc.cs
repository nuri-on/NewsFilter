using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

namespace NewsFilter.Model
{
    class SQLiteProc
    {
        public void existsSqliteDb()
        {
            #region DB 존재여부 체크
            //bool returnBool = false;
            //var uri = new Uri("ms-appx:///NewsFilter2.db3"); //in application folder 
            //var msg = new MessageDialog("");
            //try
            //{
            //    var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            //    returnBool = true;
            //}
            //catch (Exception ex)
            //{
            //    returnBool = false;
            //}            

            //msg = new MessageDialog(returnBool.ToString());
            //await msg.ShowAsync();
            #endregion
            var dbpath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "NewsFilter.db3");

            try
            {
                using (var db = new SQLite.SQLiteConnection(dbpath))
                {
                    db.CreateTable<KeyWordTable>();
                    db.CreateTable<SelectionList>();
                    db.CreateTable<BookMarkList>();
                    db.Commit();
                    db.Dispose();
                }
            }
            catch (Exception ex) { }
        }

        private void DropKeyWordTAble()
        {
            try
            {
                var dbpath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "NewsFilter.db3");
                using (var db = new SQLite.SQLiteConnection(dbpath))
                {
                    db.DropTable<KeyWordTable>();
                    db.Dispose();
                }
            }
            catch (Exception ex)
            {
                var msg = new MessageDialog(ex.Message);
                msg.ShowAsync();
            }
        }
    }
}
