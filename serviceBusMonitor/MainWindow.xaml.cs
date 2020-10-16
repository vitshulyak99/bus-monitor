using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System;
using System.Threading;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using serviceBusMonitor.Models;
using serviceBusMonitor.ViewModels;

using serviceBusMonitor.BackgroundServices.WebSockets;

using serviceBusMonitor.Models.Entities.ServiceBus;

namespace serviceBusMonitor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly public string wsPath = "ws://0.0.0.0:8282";
        readonly string sbPath = @"JSON_CONFIGS\serviceBusConfig.json";

        readonly MyWebSocket socket = null;




        #region streaming variable

        Streaming streamingProd;
        Streaming streamingStage;
        Streaming streaming;
        CancellationTokenSource streamingTokenSource = new CancellationTokenSource();
        CancellationTokenSource serviceBusTokenSource = new CancellationTokenSource();
        ObservableCollection<LPandOB> streamingValues = new ObservableCollection<LPandOB>();
        
         bool isStreamingUpdate = false;

        #endregion

        #region servicebus variables
        ServiceBus serviceBus;
        object lockS = new object();
        object lockCollectionQ = new object();

        IEnumerable<object> streamingV = new List<LPandOB>();

        bool isUpdatingServiceBus = false;

        #endregion

        public MainWindow()
        {

            InitializeComponent();
            // SBConfig = ServiceBusConfig.CreateConfigs(sbPath);
            StreamingListView.ItemsSource = streamingValues;
            xWS.Content = $"Url  {wsPath}";
            
            socket = new MyWebSocket(wsPath);
            socket.Start();
            serviceBus = new ServiceBus(ServiceBusConfig.Create(sbPath));
            streamingProd = new Streaming(Streaming.Prod);
            streamingStage = new Streaming(Streaming.Stage);
        }

        #region streaming methods
        private void StreamingRun(bool? whySer)
        {
            streaming = (whySer ?? throw new ArgumentNullException(nameof(whySer))) ? streamingProd : streamingStage;
        }

        private void StreamingViewUpdateStart()
        {
            if (!isStreamingUpdate)
                Task.Run(() =>
                {
                    isStreamingUpdate = true;

                    while (true)
                    {
                        IEnumerable<LPandOB> sv = streaming.Collection;
                        socket.SendLogToUser("streaming", sv);
                        List<LPandOB> collection = new List<LPandOB>(sv);
                        if (collection != null)
                            Dispatcher.Invoke(
                                    () =>
                                    {
                                        foreach (var i in collection)
                                            SetStreamingValue(i);
                                        StreamingListView.Items.Refresh();
                                    },
                                    System.Windows.Threading.DispatcherPriority.Background);
                        Task.Delay(TimeSpan.FromSeconds(4)).Wait();
                    }
                });
        }

        private void SetStreamingValue(LPandOB lPandOB)
        {
            lPandOB.SetDelays();
            lock (svlocker)
            {
                var x = streamingValues.FirstOrDefault(l => l.ShortName == lPandOB.ShortName);
                if (x == null)
                {
                    streamingValues.Add(lPandOB);
                }
                else
                {
                    streamingValues[streamingValues.IndexOf(x)] = lPandOB;
                }
                //StreamingListView.Items.Refresh();
            }
        }

        #endregion

        #region streaming view sorting
        private GridViewColumnHeader _lastHeaderClicked;
        private ListSortDirection _lastDirection;
        private object svlocker = new object();

       

        void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                    Sort(sortBy, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
             CollectionViewSource.GetDefaultView(StreamingListView.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        #endregion

        #region servicebus methods
        private void ServiceBusViewUpdate(int i)
        {
                serviceBus.SetServer(i);
                if(!isUpdatingServiceBus)
                
                Task.Run(async () =>
                {
                    if (serviceBusTokenSource.Token.IsCancellationRequested)
                        serviceBusTokenSource.Token.ThrowIfCancellationRequested();
                    else
                    {
                        List<BusQueue> bq = null;
                        while (true)
                        {
                            if (serviceBus.IsRunning == false) await Task.Delay(TimeSpan.FromSeconds(5));
                            lock (lockCollectionQ)
                            {
                                //bq = serviceBus.GetGetQueues();
                            }

                            socket.SendLogToUser("bus", bq);

                            var x1 = new ObservableCollection<BusQueue>(bq).OrderByDescending(x => x.Active);


                            var x2 = new ObservableCollection<BusQueue>(bq).OrderByDescending(x => x.DeadLetter);
                            if (bq != null && bq.Count() > 0)
                                Dispatcher.Invoke(() =>
                                {
                                    ActiveMessageListView.ItemsSource = x1;
                                    ActiveMessageListView.Items.Refresh();
                                    DeadletterView.ItemsSource = x2;
                                    DeadletterView.Items.Refresh();
                                }, System.Windows.Threading.DispatcherPriority.Background);
                            Task.Delay(TimeSpan.FromSeconds(7)).Wait();
                        }
                    }
                },serviceBusTokenSource.Token);

            Task.Run(async () =>
            {
                if (serviceBusTokenSource.Token.IsCancellationRequested)
                    serviceBusTokenSource.Token.ThrowIfCancellationRequested();
                else
                {
                    List<BusTopic> bq = null;
                    while (true)
                    {
                        if (serviceBus.IsRunning == false) await Task.Delay(TimeSpan.FromSeconds(5));
                        lock (lockCollectionQ)
                        {
                            bq = new List<BusTopic>( serviceBus.GetGetTopics());
                        }

                        socket.SendLogToUser("topics", bq);

                        try
                        {

                            //var x1 = new ObservableCollection<BusTopic>(bq.GroupBy(g => g.Worker)
                            //        .Select(s =>

                            //            new BusTopic(
                            //                s.Key,
                            //                s.Where(f => f.Name == s.Key)
                            //                    .Select(l => l.Name)
                            //                    .FirstOrDefault(),
                            //                s.Sum(ss => ss.Active),
                            //                s.Sum(ss => ss.DeadLetter)
                            //                )
                            //        ))
                            //        .OrderByDescending(order => order.Active);
                            var x2 = new ObservableCollection<BusTopic>(bq)
                                    .OrderByDescending(x => x.Active);

                            if (bq != null && bq.Count() > 0)
                                Dispatcher.Invoke(() =>
                                {
                                    //TopicsView.ItemsSource = x1;
                                    //TopicsView.Items.Refresh();
                                    TopicSubscriptionsView.ItemsSource = x2;
                                    TopicSubscriptionsView.Items.Refresh();
                                }, System.Windows.Threading.DispatcherPriority.Background);
                            await Task.Delay(TimeSpan.FromMinutes(1));

                        }catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                }
            }, serviceBusTokenSource.Token);
        }
        #endregion


        #region window events
        private void ChoiceProd_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
           
                ServiceBusViewUpdate(0);
                isUpdatingServiceBus = true;

                if (streaming != null)
                    streaming.Stop();
                lock (lockS)
                    streaming = streamingProd;
               Dispatcher.Invoke(()=> xState.Content = "Prod", System.Windows.Threading.DispatcherPriority.Background);
                streaming.Run();
                StreamingViewUpdateStart();

            });
        }

        private void ChoiceStage_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                if (streaming != null)
                    streaming.Stop();
              
                ServiceBusViewUpdate(1);
                isUpdatingServiceBus = true;
                lock (lockS)
                    streaming = streamingStage;
                Dispatcher.Invoke(() => xState.Content = "Stage", System.Windows.Threading.DispatcherPriority.Background);
                streaming.Run();
                StreamingViewUpdateStart();

              
            });
        }

        private void StopStreamingbtn_Click(object sender, RoutedEventArgs e)
        {
            streaming.Stop();
        }

        private void StopBusBtn_Click(object sender, RoutedEventArgs e)
        {
            serviceBusTokenSource.Cancel();
        }

        #endregion

    }
}