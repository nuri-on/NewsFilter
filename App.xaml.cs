using NewsFilter.Common.Util;
using NewsFilter.Model;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 새 응용 프로그램 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234227에 나와 있습니다.

namespace NewsFilter
{
    /// <summary>
    /// 기본 응용 프로그램 클래스를 보완하는 응용 프로그램별 동작을 제공합니다.
    /// </summary>
    sealed partial class App : Application
    {

        private Frame _rootFrame; //추가- 최상위 프레임을 클래스 전역 필드로 선언.


        /// <summary>
        /// Singleton 응용 프로그램 개체를 초기화합니다. 이것은 실행되는 작성 코드의 첫 번째
        /// 줄이며 따라서 main() 또는 WinMain()과 논리적으로 동일합니다.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 최종 사용자가 응용 프로그램을 정상적으로 시작할 때 호출됩니다. 다른 진입점은
        /// 특정 파일을 열거나, 검색 결과를 표시하는 등 응용 프로그램을 시작할 때
        /// 사용됩니다.
        /// </summary>
        /// <param name="args">시작 요청 및 프로세스에 대한 정보입니다.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {

            //추가
            if (_rootFrame == null)
            {
                _rootFrame = new Frame();
            }

            _rootFrame.Navigate(typeof(SplashScreenCustom), args.SplashScreen); // 앱이 시작되면 Splash Screen 확장 xaml 페이지로 이동하며 SplashScreen 객체도 함께 넘긴다.
            Window.Current.Content = _rootFrame; // Splash Screen 확장페이지를 현재페이지로 설정하고,
            Window.Current.Activate(); // 활성화 시킨다.

            await PerformDataFetch(); // Splash Screen 확장 페이지가 활성화되는 동안 비동기 호출로 우리가 처리할 일을 한다. PerformDateFetch 메서드 호출(아래)


            #region 삭제코드
            //Frame rootFrame = Window.Current.Content as Frame;
            //// 창에 콘텐츠가 이미 있는 경우 앱 초기화를 반복하지 말고,
            //// 창이 활성화되어 있는지 확인하십시오.
            //if (rootFrame == null)
            //{
            //    // 탐색 컨텍스트로 사용할 프레임을 만들고 첫 페이지로 이동합니다.
            //    rootFrame = new Frame();

            //    if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            //    {
            //        //TODO: 이전에 일시 중지된 응용 프로그램에서 상태를 로드합니다.
            //    }

            //    // 현재 창에 프레임 넣기
            //    Window.Current.Content = rootFrame;
            //}

            //if (rootFrame.Content == null)
            //{
            //    // 탐색 스택이 복원되지 않으면 첫 번째 페이지로 돌아가고
            //    // 필요한 정보를 탐색 매개 변수로 전달하여 새 페이지를
            //    // 구성합니다.
            //    if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
            //    {
            //        throw new Exception("Failed to create initial page");
            //    }
            //}
            //// 현재 창이 활성 창인지 확인
            //Window.Current.Activate();
            #endregion
        }

        internal async Task PerformDataFetch() //추가
        {
            await Task.Delay(TimeSpan.FromSeconds(3)); // 그냥 프로그래스링 보여주기 위함. 3초딜레이 (실제서비스땐 제거)

            if (AboutNetwork.IsInternet()) // /Common/Default 폴더에 Util 클래스 내에 인터넷 연결 유무 판단 메서드 호출 (static(정적메서드)로 만드는게 좋을듯)
            {
                SQLiteProc sqliteProc = new SQLiteProc();
                sqliteProc.existsSqliteDb();

                RemoveExtendedSplash(); //true [네트워크에 연결되어 있다면] 반환시 RemoveExtendedSplash 메서드 호출 (아래...)
            }
            else //false 반환했을시 Message를 보여준다
            {
                var msg = new MessageDialog("인터넷 연결이 되지 않았습니다. 인터넷 연결 후 다시 시도하십시오."); //다이어그램 메세지 설정
                msg.Commands.Add(new UICommand("확인", (UICommandInvokeHandler) => // 버튼을 추가. 그 버튼을 눌렀을때의 이벤트 핸들러(UICommandInvokedHandler) 를 파라미터로 전달
                {
                    Application.Current.Exit(); //어플리케이션 종료
                }));
                msg.ShowAsync(); // 실제 메세지를 보여준다
            }
        }

        internal void RemoveExtendedSplash() //추가
        {
            if (_rootFrame != null) _rootFrame.Navigate(typeof(MainPage)); // MainPage로 이동
        }

        /// <summary>
        /// 응용 프로그램 실행이 일시 중지된 경우 호출됩니다. 응용 프로그램이 종료될지
        /// 또는 메모리 콘텐츠를 변경하지 않고 다시 시작할지 여부를 결정하지 않은 채
        /// 응용 프로그램 상태가 저장됩니다.
        /// </summary>
        /// <param name="sender">일시 중지된 요청의 소스입니다.</param>
        /// <param name="e">일시 중지된 요청에 대한 세부 정보입니다.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 응용 프로그램 상태를 저장하고 백그라운드 작업을 모두 중지합니다.
            deferral.Complete();
        }
    }
}
