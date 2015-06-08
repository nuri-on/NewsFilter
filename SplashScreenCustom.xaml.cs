using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NewsFilter.Common;
using NewsFilter.Model;
using NewsFilter.NotificationExtention;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace NewsFilter
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class SplashScreenCustom : Page
    {
        public SplashScreenCustom()
        {
            this.InitializeComponent();

            
            this.setTile();
            
        }
            

        /// <summary>
        /// 이 페이지가 프레임에 표시되려고 할 때 호출됩니다.
        /// </summary>
        /// <param name="e">페이지에 도달한 방법을 설명하는 이벤트 데이터입니다. Parameter
        /// 속성은 일반적으로 페이지를 구성하는 데 사용됩니다.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SplashScreen splash = (SplashScreen)e.Parameter; //추가
        }

        private void ProgressRing_Unloaded(object sender, RoutedEventArgs e)
        {
        }


        private void setTile()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideImageAndText01); 

            List<ContentListEntity> contentListEntity = new List<ContentListEntity>();
            
            ContentListEntity dc = new ContentListEntity();





            dc.keywordIdx = 1;
            dc.imageStr = "";
            dc.headline = "업데이트 현황\n동덕여대 : 0";
            dc.summary = "아아아아아아아";
            dc.pubDate = "2013.09.07";
            dc.publisher = "아잉";

            contentListEntity.Add(dc);



            dc = new ContentListEntity();
            dc.keywordIdx = 1;
            dc.imageStr = "";
            dc.headline = "업데이트 현황\n\nEXO : 14";
            dc.summary = "아아아아아아아";
            dc.pubDate = "2013.09.07";
            dc.publisher = "아잉";

            contentListEntity.Add(dc);


            dc = new ContentListEntity();
            dc.keywordIdx = 1;
            dc.imageStr = "";
            dc.headline = "업데이트 현황\n\n수능 : 3";
            dc.summary = "아아아아아아아";
            dc.pubDate = "2013.09.07";
            dc.publisher = "아잉";

            contentListEntity.Add(dc);

            foreach (ContentListEntity ContentListEntity in contentListEntity)
            {
                // 정사각형 라이브 타일
                var squareTile = new TileSquarePeekImageAndText04();

                squareTile.TextBodyWrap.Text = ContentListEntity.headline;
                squareTile.Image.Alt = ContentListEntity.summary;
                squareTile.Image.Src = "";

                // 큰사각형 라이브 타일
                var wideTile = new TileWideSmallImageAndText03 { SquareContent = squareTile };

                wideTile.TextBodyWrap.Text = ContentListEntity.headline;
                wideTile.Image.Alt = ContentListEntity.summary;
                wideTile.Image.Src = "";

                // 알림 설정
                var notification = wideTile.CreateNotification();
                notification.Tag = ContentListEntity.headline;

                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            }
        }
    }
}
