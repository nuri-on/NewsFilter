using SQLite;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;

namespace NewsFilter.Model
{

    #region SQLite 에서 사용할 Table Class End
    public class KeyWordTable
    {
        [AutoIncrement, PrimaryKey]
        public int keywordIdx { get; set; }
        [MaxLength(50)]
        public string keywordStr { get; set; }
        public DateTime createTime { get; set; }
    }

    public class SelectionList
    {
        public string datetimTrasPK { get; set; }
        public string titleTransPK { get; set; }
        public string unescapedUrl { get; set; }
        public DateTime createTime { get; set; }
    }

    public class BookMarkList
    {
        [AutoIncrement, PrimaryKey]
        public int bookmarkIdx { get; set; }
        public int keywordIdx { get; set; }
        public string datetimTrasPK { get; set; }
        public string imageStr { get; set; }
        public string headline { get; set; }
        public string summary { get; set; }
        public string pubDate { get; set; }
        public string publisher { get; set; }
        public DateTime createTime { get; set; }
        public string unescapedUrl { get; set; }
    }
#endregion

    // -- Key 등록 
    public class KeywordListEntity
    {
        public int number { get; set; }
        public string keyword { get; set; }
    }
    // -- 뉴스리스트
    public class ContentListEntity : INotifyPropertyChanged
    {
        public int keywordIdx { get; set; }
        public string imageStr { get; set; }
        public string headline { get; set; }
        public string summary { get; set; }
        public string pubDate { get; set; }
        public string publisher { get; set; }
        public string unescapedUrl { get; set; }

        private string bookmarkImg; // 질문
        public string BookmarkImg
        {
            get { return bookmarkImg; }
            set
            {
                bookmarkImg = value;
                NotifyPropertyChanged("BookmarkImg");
            }
        }

        private bool bookmarkClick;
        public bool BookmarkClick
        {
            get { return bookmarkClick; }
            set
            {
                bookmarkClick = value;
                NotifyPropertyChanged("BookmarkClick");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    // -- Google News Api 질의 하기위한 Entity
    public class ApiDataInputEntity
    {
        public string query { get; set; }
        public int startPage { get; set; }
        public string topic { get; set; }
    }    

    // -- 컨텐츠 데이타 View Model
    public class NewsViewModel : INotifyPropertyChanged
    {
        private NewsILCollection newsILC;
        public NewsILCollection NewsILC
        {
            get { return newsILC; }
            set
            {
                newsILC = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string p=null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler!=null)
            {
                eventHandler(this, new PropertyChangedEventArgs(p));
            }
        }

        // 생성자
        public NewsViewModel(ProgressRing pr, string keywordStr, string loc)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled == true)
            {
                // 디자인 타임에 보여줄 데이타 컬렉션을 정의할수 있음
            }
            else
            {
                NewsILC = new NewsILCollection(pr, keywordStr, loc);
            }
        }
    }

}
