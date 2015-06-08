using NewsFilter.Common;
using NewsFilter.Common.Util;
using NewsFilter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;


namespace NewsFilter.Model
{
    public class NewsILCollection : ObservableCollection<ContentListEntity>, ISupportIncrementalLoading
    {
        private bool _busy;
        private int incrementIdx;
        private ProgressRing progressRing;
        private string keyword;
        private int rowDataCnt;
        private int estimatedResultCount;
        private bool firstGetDataBool = false;
        private bool _hasMoreItems = true;
        private string location;

        #region ISupportIncrementalLoading
        public bool HasMoreItems
        {
            get { return _hasMoreItems; }
        }

        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy)
            {
                return null;
            }
            _busy = true;

            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                progressRing.Visibility = Visibility.Visible;
                await Task.Delay(500);

                if (location == "KEYWORD")
                {
                    #region 키워드 쪽에서 요청했을때
                    ApiDataInputEntity queryData = new ApiDataInputEntity()
                    {
                        query = keyword,
                        startPage = incrementIdx * 8,
                        topic = ""
                    };

                    ObservableCollection<ContentListEntity> items = new ObservableCollection<ContentListEntity>();

                    using (var http = new HttpClient())
                    {
                        var getQuery = string.Format(NewsConfig.NEWS_RESOURCE_URL, queryData.query, queryData.startPage, AboutNetwork.GetDeviceUserIP());

                        var resp = await http.GetStringAsync(getQuery);
                        if (resp != null)
                        {
                            #region 북마크 표시하기 위함
                            List<SelectionList> bookMarkList;

                            using (var db = new SQLite.SQLiteConnection(NewsConfig.DB_PATH))
                            {
                                db.DeleteAll<SelectionList>();

                                bookMarkList = new List<SelectionList>();

                                var getBookMark = from b in db.Table<BookMarkList>().OrderByDescending(b => b.bookmarkIdx)
                                                  select b;

                                foreach (var item in getBookMark)
                                {
                                    bookMarkList.Add(new SelectionList() { datetimTrasPK = item.datetimTrasPK, titleTransPK = item.headline });
                                }

                                db.Dispose();//db.Close();
                            }
                            #endregion

                            #region JsonParse
                            JsonTextParser parser = new JsonTextParser();
                            JsonObject obj = parser.Parse(resp);
                            JsonObjectCollection col = (JsonObjectCollection)obj;

                            try
                            {
                                JsonObjectCollection responseData = (JsonObjectCollection)col["responseData"];
                                JsonArrayCollection results = responseData["results"] as JsonArrayCollection;

                                List<ContentListEntity> jsonDtList = new List<ContentListEntity>();

                                foreach (JsonObjectCollection item in results)
                                {
                                    ContentListEntity jsonDT = new ContentListEntity();

                                    jsonDT.headline = AboutString.specialStringConverter((string)item["titleNoFormatting"].GetValue());

                                    JsonObjectCollection image = (JsonObjectCollection)item["image"];
                                    jsonDT.imageStr = image != null ? (string)image["url"].GetValue() : "http://public.slidesharecdn.com/images/logo.png";
                                    jsonDT.summary = AboutString.specialStringConverter(Regex.Replace((string)item["content"].GetValue(), @"\\u[\d\w]{4}", String.Empty));
                                    jsonDT.pubDate = String.Format("{0:yyyy-MM-dd HH:mm:ss}", Convert.ToDateTime((string)item["publishedDate"].GetValue()));
                                    jsonDT.publisher = (string)item["publisher"].GetValue();
                                    jsonDT.BookmarkImg = "/Image/bookmark.png";
                                    jsonDT.BookmarkClick = false;
                                    jsonDT.keywordIdx = 0;
                                    jsonDT.unescapedUrl = (string)item["unescapedUrl"].GetValue();

                                    rowDataCnt++;

                                    jsonDtList.Add(jsonDT);
                                    //this.Add(jsonDT);
                                }

                                // -- 두개의 키값으로 조인방법은?
                                var joinData = (from j in jsonDtList
                                                join
                                                    b in bookMarkList
                                                    //on  String.Format("{0:yyyyMMddhhmmss}", c.pubDate) equals b.datetimTrasPK into td
                                                    on j.headline equals b.titleTransPK into td
                                                from lojoin in td.DefaultIfEmpty()
                                                //where c.headline == lojoin.titleTransPK
                                                orderby j.pubDate descending
                                                select new ContentListEntity
                                                {
                                                    headline = j.headline,
                                                    imageStr = j.imageStr,
                                                    summary = j.summary,
                                                    pubDate = j.pubDate,
                                                    publisher = j.publisher,
                                                    BookmarkImg = lojoin == null ? "/Image/bookmark.png" : "/Image/bookmark_change.png",
                                                    BookmarkClick = lojoin == null ? false : true,
                                                    unescapedUrl = j.unescapedUrl
                                                }).ToList();

                                // 북마크 표시를 위해 다시 반복처리... 데이타가 8개라서 크게 부담없을듯...
                                foreach (var itemJoin in joinData)
                                {
                                    ContentListEntity jsonDT2 = new ContentListEntity();
                                    jsonDT2.headline = itemJoin.headline;
                                    jsonDT2.imageStr = itemJoin.imageStr;
                                    jsonDT2.summary = itemJoin.summary;
                                    jsonDT2.pubDate = itemJoin.pubDate;
                                    jsonDT2.publisher = itemJoin.publisher;
                                    jsonDT2.BookmarkImg = itemJoin.BookmarkImg;
                                    jsonDT2.BookmarkClick = itemJoin.BookmarkClick;
                                    jsonDT2.keywordIdx = itemJoin.keywordIdx;
                                    jsonDT2.unescapedUrl = itemJoin.unescapedUrl;

                                    this.Add(jsonDT2);
                                }

                                JsonObjectCollection cursor = (JsonObjectCollection)responseData["cursor"];

                                if (!firstGetDataBool)
                                {
                                    estimatedResultCount = Convert.ToInt32(cursor["estimatedResultCount"].GetValue());
                                    firstGetDataBool = true;
                                }

                                incrementIdx++;

                                if (estimatedResultCount <= rowDataCnt && firstGetDataBool)
                                {
                                    _hasMoreItems = false;
                                }
                            }
                            catch (Exception) { }

                            #endregion
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 북마크 리스트에서 요청
                    using (var db = new SQLite.SQLiteConnection(NewsConfig.DB_PATH))
                    {
                        var bookMarkList = from b in db.Table<BookMarkList>().OrderByDescending(b => b.createTime)
                                           select b;

                        foreach (var item in bookMarkList)
                        {
                            ContentListEntity jsonDT2 = new ContentListEntity();
                            jsonDT2.headline = item.headline;
                            jsonDT2.imageStr = item.imageStr;
                            jsonDT2.summary = item.summary;
                            jsonDT2.pubDate = item.pubDate;
                            jsonDT2.publisher = item.publisher;
                            jsonDT2.BookmarkImg = "/Image/bookmark_change.png";
                            jsonDT2.BookmarkClick = true;
                            jsonDT2.unescapedUrl = item.unescapedUrl;

                            this.Add(jsonDT2);
                        }

                        _hasMoreItems = false;
                    }
                    #endregion
                }
                return new LoadMoreItemsResult { Count = (uint)Items.Count() };
            }
            finally
            {
                progressRing.Visibility = Visibility.Collapsed;
                _busy = false;
            }
        }
        #endregion

        public NewsILCollection(ProgressRing pr, string keywordStr, string loc)
        {
            progressRing = pr;
            keyword = keywordStr;
            location = loc;
        }
    }
}
