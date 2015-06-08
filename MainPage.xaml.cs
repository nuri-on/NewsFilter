using NewsFilter.Common.Util;
using NewsFilter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Net.Json;
using System.Text.RegularExpressions;

using WinRTXamlToolkit.Controls;
using WinRTXamlToolkit.Controls.Extensions;
using NewsFilter.Common;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace NewsFilter
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>


    

    public sealed partial class MainPage : Page
    {

        HttpClient client;
        private int keywordMaxIndex;

        public NewsViewModel newsViewModel
        {
            get { return this.DataContext as NewsViewModel; }
            set
            {
                this.DataContext = value;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<KeywordListEntity> KeyWord { get; set; }
        //public ObservableCollection<ContentListEntity> Contents { get; set; }


        

        /// <summary>
        /// 이 페이지가 프레임에 표시되려고 할 때 호출됩니다.
        /// </summary>
        /// <param name="e">페이지에 도달한 방법을 설명하는 이벤트 데이터입니다. Parameter
        /// 속성은 일반적으로 페이지를 구성하는 데 사용됩니다.</param>
        /// 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GetKeywordTableList(); //데이터베이스에 저장된 키워드를 가져오는 메서드
            listViewKeyWords.ItemsSource = KeyWord; // 가져온 키워드들을 리스트뷰에 삽입
        }


        private void GetKeywordTableList() // 데이터베이스에 저장된 키워드테이블을 가져오는 메서드
        {
            try
            {
                using (var db = new SQLite.SQLiteConnection(NewsConfig.DB_PATH)) // 데이터베이스에 있는 내용을 db변수에 저장
                {
                    var d = from x in db.Table<KeyWordTable>().OrderByDescending(x => x.keywordIdx)
                            select x; // db변수에 저장된 키워드테이블들을 변수 d에 저장, 만든 역순으로 저장하기 위한 OrderByDescending

                    KeyWord = new ObservableCollection<KeywordListEntity>(); // 앞서 선언했던 Keyword에 ObservavleCollection 인스턴스 생성

                    foreach (var item in d)
                    {
                        KeyWord.Add(new KeywordListEntity { number = item.keywordIdx, keyword = item.keywordStr.ToString() }); //변수 d에 저장된 테이블을 리스트뷰에 추가
                    }

                    if (KeyWord.Count == 0)
                    {
                        keywordMaxIndex = 0; // 키워드가 없을경우, keywordMaxIndex는 0으로 초기화
                    }
                    else
                    {
                        keywordMaxIndex = KeyWord.Max(m => m.number); // 있을 경우, 키워드의 가장 큰 값으로 초기화
                    }

                    db.Dispose(); //db.Close();
                }
            }
            catch (Exception ex)
            {
                var msg = new MessageDialog(ex.StackTrace);    // 다이어그램 메세지 설정                
                msg.ShowAsync();
            }
        }


        private async void KeywordAdd_Click(object sender, RoutedEventArgs e) //키워드 추가 버튼 메서드
        {
            try
            {
                ProgressRingKeyword.Visibility = Visibility.Collapsed;  //프로그래스링을 잠깐 보여줌 //안보여줌(수정)              
                await getItemsAdd(); //키워드추가 메서드
            }
            finally
            {
                ProgressRingKeyword.Visibility = Visibility.Collapsed; //프로그래스링 사라짐
            }

        }

        public async Task getItemsAdd()
        {
            await Task.Delay(500); 
            InsertKeyWord(); // 데이터베이스에 키워드 저장 메서드
            keywordMaxIndex++;
            KeyWord.Insert(0, new KeywordListEntity { number = keywordMaxIndex, keyword = txtBoxRegKeyword.Text }); // 리스트뷰에 추가키워드 디스플레이
            newsViewModel = new NewsViewModel(ProgressRingKeyword2, txtBoxRegKeyword.Text, "KEYWORD");
        }

        private void InsertKeyWord() //데이터베이스에 키워드 저장메서드
        {
            try
            {
                using (var db = new SQLite.SQLiteConnection(NewsConfig.DB_PATH))
                {
                    db.Insert(new KeyWordTable()
                    {
                        keywordStr = txtBoxRegKeyword.Text.ToString(),
                        createTime = DateTime.Now
                    });

                    db.Commit();
                    db.Dispose();
                }
            }
            catch (Exception ex)
            {
                var msg = new MessageDialog(ex.Message);
                msg.ShowAsync();
            }
        }

        private void listViewKeyWords_ItemClick(object sender, ItemClickEventArgs e)
        {

            ProgressRingKeyword2.Visibility = Visibility.Visible;

            var itemObj = e.ClickedItem as KeywordListEntity;
            var keyword = itemObj.keyword.ToString();
            var keywordIdx = itemObj.number;


            var scrollViewer = gridviewContent.GetFirstDescendantOfType<ScrollViewer>();
            scrollViewer.ScrollToHorizontalOffset(0);

            newsViewModel = new NewsViewModel(ProgressRingKeyword2, keyword, "KEYWORD");
        }


        #region listViewKeyWords_SelectionChanged 주석 (다른플젝시 참고용으로 남겨둠)
        //private void listViewKeyWords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{

        //    ProgressRingKeyword2.Visibility = Visibility.Visible;
            
            

        //    #region 북마크 해놓은 아이템 표시를 위함
        //    var dbpath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "NewsFilter.db3");
        //    List<SelectionList> bookMarkList;

        //    using (var db = new SQLite.SQLiteConnection(dbpath))
        //    {
        //        db.DeleteAll<SelectionList>();

        //        bookMarkList = new List<SelectionList>();

        //        var getBookMark = from b in db.Table<BookMarkList>().OrderByDescending(b => b.bookmarkIdx)
        //                          select b;

        //        foreach (var item in getBookMark)
        //        {
        //            bookMarkList.Add(new SelectionList() { datetimTrasPK = item.datetimTrasPK, titleTransPK = item.headline });
        //        }

        //        db.Dispose();//db.Close();
        //    }
        //    #endregion

        //    var keyword = ((KeywordListEntity)listViewKeyWords.SelectedItem).keyword;
        //    var keywordIdx = ((KeywordListEntity)listViewKeyWords.SelectedItem).number;




        //    var scrollViewer = gridviewContent.GetFirstDescendantOfType<ScrollViewer>();
        //    scrollViewer.ScrollToHorizontalOffset(0);



        //    ApiDataInputEntity apiInputData = new ApiDataInputEntity()
        //    {
        //        query = keyword,
        //        startPage = 0,
        //        topic = "h"
        //    };

        //    var task = getApiDataText(apiInputData);
        //    var awaiter = task.GetAwaiter();    // api 데이타를 받아올때까지 기다려준다

        //    awaiter.OnCompleted(() =>
        //    {
        //        string jsonData = awaiter.GetResult();

        //        Contents = new ObservableCollection<ContentListEntity>();

        //        JsonTextParser parser = new JsonTextParser();
        //        JsonObject obj = parser.Parse(jsonData);
        //        JsonObjectCollection col = (JsonObjectCollection)obj;

        //        JsonObjectCollection responseData = (JsonObjectCollection)col["responseData"];
        //        JsonArrayCollection results = responseData["results"] as JsonArrayCollection;

        //        foreach (JsonObjectCollection item in results)
        //        {
        //            ContentListEntity jsonDT = new ContentListEntity();

        //            jsonDT.headline = AboutString.specialStringConverter((string)item["titleNoFormatting"].GetValue());

        //            JsonObjectCollection image = (JsonObjectCollection)item["image"];
        //            jsonDT.imageStr = image != null ? (string)image["url"].GetValue() : "/image/noimage.png";
        //            jsonDT.summary = AboutString.specialStringConverter(Regex.Replace((string)item["content"].GetValue(), @"\\u[\d\w]{4}", String.Empty));
        //            jsonDT.pubDate = String.Format("{0:yyyy-MM-dd hh:mm:ss}", Convert.ToDateTime((string)item["publishedDate"].GetValue()));
        //            jsonDT.publisher = (string)item["publisher"].GetValue();
        //            jsonDT.BookmarkImg = "/Image/bookmark.png";
        //            jsonDT.BookmarkClick = false;
        //            jsonDT.keywordIdx = keywordIdx;
        //            jsonDT.unescapedUrl = (string)item["unescapedUrl"].GetValue();

        //            #region 각각의 아이템을 sqlite 에 저장 (익스플로러로 갈때는 필요없음)
        //            try
        //            {
        //                using (var db = new SQLite.SQLiteConnection(dbpath))
        //                {
        //                    db.Insert(new SelectionList()
        //                    {
        //                        datetimTrasPK = String.Format("{0:yyyyMMddhhmmss}", jsonDT.pubDate),
        //                        titleTransPK = jsonDT.headline,
        //                        unescapedUrl = (string)item["unescapedUrl"].GetValue(),
        //                        createTime = DateTime.Now
        //                    });

        //                    db.Commit();
        //                    db.Dispose();
        //                }
        //            }
        //            catch (Exception ex) { }
        //            #endregion

        //            ProgressRingKeyword2.Visibility = Visibility.Collapsed;

        //            Contents.Add(jsonDT);
        //        }

        //        // -- 두개의 키값으로 조인방법은?
        //        #region api 에서 가져온 데이타하고 북마크해 놓은 데이타하고 연결고리를 찾아 이미지교체
        //        var joinData = (from c in Contents
        //                        join
        //                            b in bookMarkList
        //                            //on  String.Format("{0:yyyyMMddhhmmss}", c.pubDate) equals b.datetimTrasPK into td
        //                            on c.headline equals b.titleTransPK into td
        //                        from lojoin in td.DefaultIfEmpty()
        //                        //where c.headline == lojoin.titleTransPK
        //                        orderby c.pubDate descending
        //                        select new ContentListEntity
        //                        {
        //                            headline = c.headline,
        //                            imageStr = c.imageStr,
        //                            summary = c.summary,
        //                            pubDate = c.pubDate,
        //                            publisher = c.publisher,
        //                            BookmarkImg = lojoin == null ? "/Image/bookmark.png" : "/Image/bookmark_change.png",
        //                            BookmarkClick = lojoin == null ? false : true,
        //                            unescapedUrl = c.unescapedUrl
        //                        }).ToList();
        //        #endregion

        //        //gridviewContent.ItemsSource = Contents.OrderByDescending(o => o.pubDate);
        //        gridviewContent.ItemsSource = joinData;
        //    });


        //}
        #endregion

        public async Task<string> getApiDataText(ApiDataInputEntity data)
        {
            await Task.Delay(3000);
            client = new HttpClient();

            //UTF8Encoding utf8 = new UTF8Encoding();
            //byte[] ipEncodedBytes = utf8.GetBytes(deviceIp);

            Task<string> apiText = client.GetStringAsync(String.Format(NewsConfig.NEWS_RESOURCE_URL, data.query, data.startPage, AboutNetwork.GetDeviceUserIP()));
            return await apiText;
        }

        private async void btnBookMarkList_Click(object sender, RoutedEventArgs e)
        {
            ProgressRingKeyword2.Visibility = Visibility.Visible;
            await Task.Delay(500);

            var scrollViewer = gridviewContent.GetFirstDescendantOfType<ScrollViewer>();
            scrollViewer.ScrollToHorizontalOffset(0);

            newsViewModel = new NewsViewModel(ProgressRingKeyword2, "", "BOOKMARK");
        }

        private void BookMarkButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var dc = btn.DataContext as ContentListEntity;
            var bookMark = dc.BookmarkClick;

            dc.BookmarkClick = (bookMark) ? false : true;

            if (dc.BookmarkClick)
            {
                dc.BookmarkImg = "/Image/bookmark_change.png";
                try
                {
                    using (var db = new SQLite.SQLiteConnection(NewsConfig.DB_PATH))
                    {
                        db.Insert(new BookMarkList()
                        {
                            keywordIdx = dc.keywordIdx,
                            datetimTrasPK = String.Format("{0:yyyyMMddhhmmss}", dc.pubDate),
                            imageStr = dc.imageStr,
                            headline = dc.headline,
                            summary = dc.summary,
                            pubDate = dc.pubDate,
                            publisher = dc.publisher,
                            createTime = DateTime.Now,
                            unescapedUrl = dc.unescapedUrl
                        });

                        db.Commit();
                        db.Dispose();
                    }
                }
                catch (Exception ex) { }
            }
            else
            {
                dc.BookmarkImg = "/Image/bookmark.png";
                try
                {
                    using (var db = new SQLite.SQLiteConnection(NewsConfig.DB_PATH))
                    {
                        var deleteBookMark = (db.Table<BookMarkList>().Where(b => b.headline == dc.headline).Single());
                        db.Delete(deleteBookMark);
                    }
                }
                catch (Exception ex) { }
            }
        }



        
    }
}
