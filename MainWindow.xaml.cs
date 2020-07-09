﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Arthas.Controls;
using BodDetect.BodDataManage;
using BodDetect.Event;
using BodDetect.UDP;

namespace BodDetect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        BodHelper bodHelper;
        public Dictionary<byte, MetroProgressBar> metroProgressBars = new Dictionary<byte, MetroProgressBar>();

        public Dictionary<byte, EventHandler> ProcessHandlers = new Dictionary<byte, EventHandler>();

        public Dictionary<byte, MetroSwitch> ValveDic = new Dictionary<byte, MetroSwitch>();

        public Dictionary<byte, MetroSwitch> MultiValveDic = new Dictionary<byte, MetroSwitch>();

        //readonly FinsClient finsClient;

        DispatcherTimer timer = new DispatcherTimer();

        DispatcherTimer BodTimer = new DispatcherTimer();

        DispatcherTimer WaterSampleTimer = new DispatcherTimer();

        DispatcherTimer RunTimer = new DispatcherTimer();


        public BodData bodData = new BodData();

        MainWindow_Model mainWindow_Model = new MainWindow_Model();

        Thread BodDetectRun;

        ConfigData configData = new ConfigData();

        public MainWindow()
        {
            InitializeComponent();

            metroProgressBars.Add(PLCConfig.WaterValveBit, WaterView);
            metroProgressBars.Add(PLCConfig.bufferValveBit, CacheView);
            metroProgressBars.Add(PLCConfig.StandardValveBit, StandardView);
            metroProgressBars.Add(Convert.ToByte((ushort)100), PumpView);
            metroProgressBars.Add(PLCConfig.AirValveBit, AirView);
            metroProgressBars.Add(PLCConfig.NormalValveBit, NormalView);
            metroProgressBars.Add(PLCConfig.SampleValveBit, SampleView);
            metroProgressBars.Add(PLCConfig.DepositValveBit, StoreWaterView);
            metroProgressBars.Add(Convert.ToByte((ushort)101), WaterSampleView);


            ProcessHandlers.Add(PLCConfig.WaterValveBit, RefeshWaterProcessEvent);
            ProcessHandlers.Add(PLCConfig.bufferValveBit, RefeshCachProcessEvent);
            ProcessHandlers.Add(PLCConfig.StandardValveBit, RefeshStandProcessEvent);
            ProcessHandlers.Add(Convert.ToByte((ushort)100), RefeshPumpProcessEvent);
            ProcessHandlers.Add(PLCConfig.AirValveBit, RefeshAirProcessEvent);
            ProcessHandlers.Add(PLCConfig.NormalValveBit, RefeshNormalProcessEvent);
            ProcessHandlers.Add(PLCConfig.SampleValveBit, RefeshSampleProcessEvent);
            ProcessHandlers.Add(PLCConfig.DepositValveBit, RefeshStoreWaterProcessEvent);
            ProcessHandlers.Add(Convert.ToByte((ushort)101), RefeshWaterSampleProcessEvent);


            ValveDic.Add(PLCConfig.WaterValveBit, WaterValve);
            ValveDic.Add(PLCConfig.bufferValveBit, CacheValve);
            ValveDic.Add(PLCConfig.StandardValveBit, StandValve);
            ValveDic.Add(PLCConfig.AirValveBit, AirValve);
            ValveDic.Add(PLCConfig.NormalValveBit, NormalValve);
            ValveDic.Add(PLCConfig.SampleValveBit, Valve);
            ValveDic.Add(PLCConfig.DepositValveBit, StoreValve);


            //ValveDic.Add(PLCConfig.CisternValveBit, RowValve);
            //ValveDic.Add(PLCConfig.WashValveBit, WashValve);
            //ValveDic.Add(PLCConfig.BodDrainValveBit, BodRowValve);


            init();



        }


        public void init()
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    SysStatusList.Items.Add(new SysStatusMsg(i, "test0", "test1", "test2"));
                }

                for (int i = 0; i < 10; i++)
                {
                    AlarmList.Items.Add(new AlarmData(i, "test0", i + 10, "test1", "test2", true));
                }

                for (int i = 0; i < 10; i++)
                {
                    HisAlarmList.Items.Add(new AlarmData(i, "test0", i + 10, "test1", "test2", true));
                }


                logList.Items.Add("test");

                string ip = IP_textbox.Text;

                string[] value = ip.Split('.');
                if (value.Length < 4)
                {
                    MessageBox.Show("异常ip!");
                }

                int port = Convert.ToInt32(Port_TextBox.Text);

                bodHelper = new BodHelper(ip, port);
                bool success = bodHelper.ConnectPlc();



                bodHelper.refreshProcess = new BodHelper.RefreshUI(RefeshProcess);
                bodHelper.refreshStaus = new BodHelper.RefreshStaus(RefreshStatus);

                bodHelper.refreshData = new BodHelper.RefreshData(RefreshData);


                bodHelper.mainWindow = this;
            }
            catch (Exception)
            {
                MessageBox.Show("连接PLC异常!");
            }
        }

        private void MetroButton_Click(object sender, RoutedEventArgs e)
        {

            float[] DoDota = bodHelper.GetDoData();
            uint[] TurbidityData = bodHelper.GetTurbidityData();
            float[] PHData = bodHelper.GetPHData();
            ushort[] CODData = bodHelper.GetCodData();

            bodData.TemperatureData = DoDota[0];
            bodData.DoData = DoDota[1];
            bodData.TurbidityData = (float)TurbidityData[0] / 1000;
            bodData.PHData = PHData[1];
            bodData.CodData = (float)CODData[0] / 100;
            //      if (finsClient == null)
            //      {
            //          MessageBox.Show("PLC 未连接");
            //          return;
            //      }


            //      ushort Address = Convert.ToUInt16(address_TextBox.Text);

            //      byte bitAddress = Convert.ToByte(Bit_TextBox.Text);

            //      ushort Count = Convert.ToUInt16(DataCount_TextBox.Text);

            ////      string area = MemoryAreaCode_combo.SelectedItem.ToString();


            //      byte AreaCode = 0X82;

            //      ushort[] data = finsClient.ReadData(Address, bitAddress, Count, AreaCode);
            //      foreach (var item in data)
            //      {
            //          string value = Convert.ToString(item) + "\r\n";
            //          Data_TextBox.AppendText(value);
            //      }

        }

        private void MetroButton_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                string ip = IP_textbox.Text;

                string[] value = ip.Split('.');
                if (value.Length < 4)
                {
                    MessageBox.Show("异常ip!");
                }

                int port = Convert.ToInt32(Port_TextBox.Text);

                bodHelper = new BodHelper(ip, port);
                bool success = bodHelper.ConnectPlc();

                bodHelper.refreshProcess = new BodHelper.RefreshUI(RefeshProcess);
                bodHelper.refreshStaus = new BodHelper.RefreshStaus(RefreshStatus);

                bodHelper.refreshData = new BodHelper.RefreshData(RefreshData);
                bodHelper.mainWindow = this;

                

                //bodData.TemperatureData = (float)16.0;
                //bodData.DoData = (float)4.3;
                //bodData.TurbidityData = (float)101;
                //bodData.PHData = (float)7.20;
                //bodData.CodData = 250;
                //bodData.Bod = 150;
                //bodData.Uv254Data = 200;
                //RefreshData(bodData);

            }
            catch (Exception)
            {
                MessageBox.Show("连接PLC异常!");
            }

        }

        private void RefreshData(BodData data)
        {
            DOTem.Content = data.Uv254Data.ToString();
            DO.Content = data.DoData.ToString();
            PH.Content = data.PHData.ToString();
            COD.Content = data.CodData.ToString();
            PHTem.Content = data.TemperatureData.ToString();
            Turbidity.Content = data.TurbidityData.ToString();

            BOD.Content = data.Bod.ToString();

        }

        private void MetroButton_Click_2(object sender, RoutedEventArgs e)
        {

            float[] floatData = bodHelper.GetDoData();

            floatData = bodHelper.GetPHData();

            uint[] value = bodHelper.GetTurbidityData();

            byte[] IOCmd = { PLCConfig.WashValveBit, PLCConfig.BodDrainValveBit };

            bodHelper.ValveControl(PLCConfig.Valve2Address, IOCmd);

            byte[] IOCmd2 = { PLCConfig.DepositValveBit, PLCConfig.StandardValveBit };
            bodHelper.ValveControl(PLCConfig.Valve1Address, IOCmd2);

            //Dictionary<UInt16, UInt16> dic = new Dictionary<ushort, ushort>();

            //dic.Add(32300, 3);
            //dic.Add(32301, 4);
            //dic.Add(32302, 4);
            //dic.Add(32303, 1);
            //dic.Add(32304, 2);

            //byte bitAdress = 0X00;

            //ushort[] value = { 3, 4, 4, 1, 2 };
            //finsClient.WriteData(32300, bitAdress, value, Dr);

            //bitAdress = 0X08;
            //byte[] wValue = { 0X01 };
            //finsClient.WriteBitData(0, bitAdress, wValue, Wr);

            //bitAdress = 0X00;
            //float[] floatData = finsClient.ReadBigFloatData(596, bitAdress, 2, Dr);
        }

        private void Sampling_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("sssss");
            return;

            Thread BodDectThread = new Thread(new ThreadStart(bodHelper.StartBodDetect));
            BodDectThread.IsBackground = true;
            BodDectThread.Start();
        }


        public void RefeshProcess(DelegateParam param)
        {
            try
            {
                if (metroProgressBars.Count < 0)
                    return;
                switch (param.State)
                {
                    case ProcessState.ShowData:
                        metroProgressBars[param.Uid].Value = (double)param.Data;
                        break;

                    case ProcessState.AutoAdd:
                        ProcessAutoAdd(param);
                        break;
                    case ProcessState.AutoRed:
                        break;
                    case ProcessState.Hidden:
                        break;
                    case ProcessState.Show:
                        break;
                }

            }
            catch (Exception)
            {

                return;
            }
        }

        /// <summary>
        /// 多通阀进度条控制
        /// </summary>
        /// <param name="param"></param>
        public void ProcessAutoAdd(DelegateParam param)
        {
            while (true)
            {
                if (!timer.IsEnabled)
                {
                    timer.Tick += ProcessHandlers[param.Uid];
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                    timer.Start();
                    return;
                }
            }
        }

        /// <summary>
        /// Bod部分的进度条委托
        /// </summary>
        /// <param name="param"></param>
        public void BodProcessCtrl(DelegateParam param)
        {


        }


        public void RefeshWaterProcessEvent(object sender, EventArgs e)
        {
            if (WaterView.Value >= WaterView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshWaterProcessEvent;
            }

            WaterView.Value++;
        }

        public void RefeshCachProcessEvent(object sender, EventArgs e)
        {
            if (CacheView.Value >= CacheView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshCachProcessEvent;
            }

            CacheView.Value++;
        }

        public void RefeshStandProcessEvent(object sender, EventArgs e)
        {
            if (StandardView.Value >= StandardView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshStandProcessEvent;
            }

            StandardView.Value++;
        }

        public void RefeshPumpProcessEvent(object sender, EventArgs e)
        {
            if (PumpView.Value >= PumpView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshPumpProcessEvent;
            }

            PumpView.Value++;
        }
        public void RefeshAirProcessEvent(object sender, EventArgs e)
        {
            if (AirView.Value >= AirView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshAirProcessEvent;
            }

            AirView.Value++;
        }

        public void RefeshNormalProcessEvent(object sender, EventArgs e)
        {
            if (NormalView.Value >= NormalView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshNormalProcessEvent;
            }

            NormalView.Value++;
        }

        public void RefeshSampleProcessEvent(object sender, EventArgs e)
        {
            if (SampleView.Value >= SampleView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshSampleProcessEvent;
            }

            SampleView.Value++;
        }

        public void RefeshStoreWaterProcessEvent(object sender, EventArgs e)
        {
            if (StoreWaterView.Value >= StoreWaterView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshStoreWaterProcessEvent;
            }

            StoreWaterView.Value++;
        }

        public void RefeshWaterSampleProcessEvent(object sender, EventArgs e)
        {
            if (WaterSampleView.Value >= WaterSampleView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshWaterSampleProcessEvent;
            }

            WaterSampleView.Value++;
        }

        private void StoreValve_Checked(object sender, RoutedEventArgs e)
        {
            MetroSwitch a = (MetroSwitch)sender;
            a.IsChecked = false;

            return;

            try
            {

                if (bodHelper.IsSampling == true)
                {
                    MessageBox.Show(" 现在正在采样过程中,禁止相关操作.", "提示", MessageBoxButton.OK);
                    return;
                }

                if (StoreValve.IsChecked == true)
                {
                    bool hasChecked = ValveDic.Any(t => t.Value.IsChecked == true && t.Key != PLCConfig.DepositValveBit);

                    if (hasChecked)
                    {
                        if (MessageBox.Show("有其他的阀门打开,是否关闭其他阀门", "提示", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                        {
                            foreach (var item in ValveDic)
                            {
                                if (item.Key != PLCConfig.DepositValveBit)
                                {
                                    item.Value.IsChecked = false;
                                }
                            }

                            byte[] data = { PLCConfig.DepositValveBit };
                            bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                        }
                    }

                }
            }
            catch (Exception)
            {
                StoreValve.IsChecked = false;
                MessageBox.Show(" 沉淀池阀门打开失败.", "提示", MessageBoxButton.OK);
            }

        }


        private void Valve_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Valve.IsChecked == true)
                {
                    byte[] data = { PLCConfig.SampleValveBit };
                    bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                }
            }
            catch (Exception)
            {
                Valve.IsChecked = false;
                MessageBox.Show(" 送样(样液)阀门打开失败.");

            }
        }

        private void NormalValve_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NormalValve.IsChecked == true)
                {
                    byte[] data = { PLCConfig.NormalValveBit };
                    bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                }
            }
            catch (Exception)
            {
                NormalValve.IsChecked = false;
                MessageBox.Show(" 送样(标液)阀门打开失败.");

            }
        }


        private void Valves_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MetroSwitch valve = (MetroSwitch)sender;

                if (bodHelper.IsSampling == true)
                {
                    MessageBox.Show(" 现在正在采样过程中,禁止相关操作.", "提示", MessageBoxButton.OK);
                    valve.IsChecked = false;
                    return;
                }

                if (valve.IsChecked == true)
                {

                    foreach (var item in ValveDic)
                    {
                        if (item.Value != valve)
                        {
                            item.Value.IsChecked = false;
                        }
                    }

                    var key = ValveDic.FirstOrDefault(t => t.Value == valve).Key;

                    byte[] data = { key };
                    bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                }
            }
            catch (Exception)
            {
                StoreValve.IsChecked = false;
                MessageBox.Show(" 阀门打开失败.", "提示", MessageBoxButton.OK);
            }
        }

        private void MetroButton_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bodHelper.IsSampling == true)
                {
                    MessageBox.Show(" 现在正在采样过程中,禁止相关操作.", "提示", MessageBoxButton.OK);
                    return;
                }

                var CheckedVavle = ValveDic.Where(t => t.Value.IsChecked == true).ToList();

                if (CheckedVavle == null)
                {
                    MessageBox.Show(" 请打开任意一个阀门.", "提示", MessageBoxButton.OK);
                    return;
                }

                if (CheckedVavle.Count > 2)
                {
                    MessageBox.Show(" 只能打开一个阀门,请关闭阀门.", "提示", MessageBoxButton.OK);
                    return;
                }

                var key = CheckedVavle[0].Key;

                timer.Tick += ProcessHandlers[key];

                bodHelper.PunpAbsorb(PunpCapType.fiveml);
                timer.Start();

            }
            catch (Exception)
            {

            }
        }

        private void PumpDrain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (bodHelper.IsSampling == true)
                {
                    MessageBox.Show(" 现在正在采样过程中,禁止相关操作.", "提示", MessageBoxButton.OK);
                    return;
                }

                var CheckedVavle = ValveDic.Where(t => t.Value.IsChecked == true).ToList();

                if (CheckedVavle == null)
                {
                    MessageBox.Show(" 请打开任意一个阀门.", "提示", MessageBoxButton.OK);
                    return;
                }

                if (CheckedVavle.Count > 2)
                {
                    MessageBox.Show(" 只能打开一个阀门,请关闭阀门.", "提示", MessageBoxButton.OK);
                    return;
                }

                bodHelper.PumpDrain();

            }
            catch (Exception)
            {

            }
        }

        private void PumpDrain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bodHelper.IsSampling == true)
                {
                    MessageBox.Show(" 现在正在采样过程中,禁止相关操作.", "提示", MessageBoxButton.OK);
                    return;
                }

                var CheckedVavle = ValveDic.Where(t => t.Value.IsChecked == true).ToList();

                if (CheckedVavle == null)
                {
                    MessageBox.Show(" 请打开任意一个阀门.", "提示", MessageBoxButton.OK);
                    return;
                }

                if (CheckedVavle.Count > 2)
                {
                    MessageBox.Show(" 只能打开一个阀门,请关闭阀门.", "提示", MessageBoxButton.OK);
                    return;
                }

                bodHelper.PumpDrain();

            }
            catch (Exception)
            {

            }
        }

        private void PumpWaterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (bodHelper.IsSampling == true)
                {
                    MessageBox.Show(" 现在正在采样过程中,禁止相关操作.", "提示", MessageBoxButton.OK);
                    return;
                }

                PumpWaterButton.Visibility = Visibility.Collapsed;
                PumpStopButton.Visibility = Visibility.Visible;
                byte[] data = { PLCConfig.CisternPumpBit };

                bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void PumpStopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (bodHelper.IsSampling == true)
                {
                    MessageBox.Show(" 现在正在采样过程中,禁止相关操作.", "提示", MessageBoxButton.OK);
                    return;
                }

                PumpWaterButton.Visibility = Visibility.Visible;
                PumpStopButton.Visibility = Visibility.Collapsed;
                byte[] data = { 0 };

                bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void RowValve_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bodHelper.IsSampling == true)
                {
                    MessageBox.Show(" 现在正在采样过程中,禁止相关操作.", "提示", MessageBoxButton.OK);
                    return;
                }

                byte[] data = { PLCConfig.DepositValveBit };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void StandCap_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void MetroComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PunpStand_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = PunpStand.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.StandardValveBit };

                byte[] StandBodValve = { PLCConfig.NormalValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);


                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }


        private void PumpProcess(List<byte[]> data, List<ushort> address, PunpCapType punpCapType)
        {
            if (data == null || data.Count < 2 || address == null || address.Count < 2)
            {
                return;
            }

            bool success = false;

            success = bodHelper.ValveControl(address[0], data[0]);

            if (!success)
            {
                MessageBox.Show(" 阀门打开失败.", "提示", MessageBoxButton.OK);
            }

            success = bodHelper.PunpAbsorb(punpCapType);

            if (!success)
            {
                MessageBox.Show(" 注射泵抽水失败.", "提示", MessageBoxButton.OK);
            }

            Thread.Sleep(5000);

            success = bodHelper.ValveControl(address[1], data[1]);
            if (!success)
            {
                MessageBox.Show(" 阀门打开失败.", "提示", MessageBoxButton.OK);
            }
            success = bodHelper.PumpDrain();

            if (!success)
            {
                MessageBox.Show(" 注射泵放水失败.", "提示", MessageBoxButton.OK);
            }

            Thread.Sleep(5000);
        }

        private void PumpCache_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = PumpCache.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.bufferValveBit };

                byte[] StandBodValve = { PLCConfig.NormalValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);


                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }

        private void DrainCahce_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = DrainCahce.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData;


                byte[] StandValve = { PLCConfig.WaterValveBit };

                byte[] StandBodValve = { PLCConfig.NormalValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);


                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }

        private void PunpSample_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = PunpSample.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.DepositValveBit };

                byte[] StandBodValve = { PLCConfig.SampleValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }

        private void PumpWater_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = PumpWater.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.WaterValveBit };

                byte[] StandBodValve = { PLCConfig.SampleValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }

        private void PumpCache1_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = PumpCache1.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.bufferValveBit };

                byte[] StandBodValve = { PLCConfig.SampleValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }

        private void DrainSample_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = DrainSample.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData;


                byte[] StandValve = { PLCConfig.SampleValveBit };

                byte[] StandBodValve = { PLCConfig.AirValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }


                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }

        private void wash_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = wash.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.WaterValveBit };

                byte[] StandBodValve = { PLCConfig.AirValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }

        private void sample_start_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { PLCConfig.CisternPumpBit };

            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {
                MessageBox.Show(" 抽水样泵打开失败.", "提示", MessageBoxButton.OK);
            }
        }

        private void sample_end_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { 0 };

            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {
                MessageBox.Show(" 抽水样泵关闭失败.", "提示", MessageBoxButton.OK);
            }
        }

        private void drain_start_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { PLCConfig.CisternValveBit };

            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {
                MessageBox.Show(" 沉淀池排水阀打开失败.", "提示", MessageBoxButton.OK);
            }
        }

        private void drain_end_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { 0 };

            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {
                MessageBox.Show(" 沉淀池排水阀关闭失败.", "提示", MessageBoxButton.OK);
            }
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string value = textBox.Text;

            int a = SysStatusList.Items.Count;
            SysStatusList.Items.Add(new SysStatusMsg(a + 1, "2", value, value));
        }

        private void StandDilution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cap = StandAll.Text;
                string zoom = Dilution.Text;

                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);
                int zoomdata = Convert.ToInt32(zoom);

                int waterData = capData * (zoomdata - 1);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.StandardValveBit };

                byte[] StandBodValve = { PLCConfig.NormalValveBit };

                byte[] waterValve = { PLCConfig.WaterValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }


                times = waterData / 5;
                extraTimes = waterData % 5;
                data[0] = waterValve;
                data[1] = StandBodValve;

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }


                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }

        private void SampleDilution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cap = SampleAll.Text;
                string zoom = DilutionSamp.Text;

                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);
                int zoomdata = Convert.ToInt32(zoom);

                int waterData = capData * (zoomdata - 1);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.DepositValveBit };

                byte[] StandBodValve = { PLCConfig.SampleValveBit };

                byte[] waterValve = { PLCConfig.WaterValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                times = waterData / 5;
                extraTimes = waterData % 5;
                data[0] = waterValve;
                data[1] = StandBodValve;

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }


                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Tool.ShowInputPanel();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Tool.HideInputPanel();
        }

        private void PreButton_Click(object sender, RoutedEventArgs e)
        {
            if (bodHelper == null || bodHelper.finsClient == null || bodHelper.finsClient == null)
            {
                MessageBox.Show(" PLC未连接请连接.", "提示", MessageBoxButton.OK);
            }

            bodHelper.PreInit();
        }

        private void PreButton2ml_Click(object sender, RoutedEventArgs e)
        {
            if (bodHelper == null || bodHelper.finsClient == null || bodHelper.finsClient == null)
            {
                MessageBox.Show(" PLC未连接请连接.", "提示", MessageBoxButton.OK);
            }

            byte[] StandBodValve = { PLCConfig.NormalValveBit };

            byte[] waterValve = { PLCConfig.WaterValveBit };

            byte[] AirValve = { PLCConfig.AirValveBit };

            List<byte[]> data = new List<byte[]>();
            List<ushort> address = new List<ushort>();
            data.Add(waterValve);
            data.Add(StandBodValve);

            address.Add(PLCConfig.Valve2Address);
            address.Add(PLCConfig.Valve2Address);

            PumpProcess(data, address, PunpCapType.Point2ml);

            data[0] = AirValve;

            PumpProcess(data, address, PunpCapType.fiveml);

        }

        private void Pre2ml_Click(object sender, RoutedEventArgs e)
        {
            bodHelper.PunpAbsorb(PunpCapType.Point2ml);
        }

        private void Pumpdrain2ml_Click(object sender, RoutedEventArgs e)
        {
            bodHelper.PumpDrain();
        }

        private void PrePumpAri_Click(object sender, RoutedEventArgs e)
        {
            //bodHelper.ChangePunpValve(PumpValveType.pre);
            //bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);


            byte[] data = { PLCConfig.AirValveBit };

            bodHelper.ValveControl(PLCConfig.Valve2Address, data);
            data[0] = 0X01;

            bodHelper.ChangePunpValve(PumpValveType.pre);
            bodHelper.finsClient.WriteBitData(1, 0, data, PLCConfig.Wr);

            //bodHelper.ValveControl(PLCConfig.Valve1Address, PLCConfig.PrePumpValveAir, data);
            bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);
            Thread.Sleep(5000);
            bodHelper.PumpDrain();

        }

        private void PrePumpwater_Click(object sender, RoutedEventArgs e)
        {
            //bodHelper.ChangePunpValve(PumpValveType.pre);
            //bodHelper.PrePumpCtrl(PrePumpWork.preDrain);

            byte[] data = { PLCConfig.WaterValveBit };

            bodHelper.ValveControl(PLCConfig.Valve2Address, data);
            data[0] = 0X01;

            bodHelper.ChangePunpValve(PumpValveType.pre);

            bodHelper.finsClient.WriteBitData(0, 15, data, PLCConfig.Wr);

            //bodHelper.ValveControl(PLCConfig.Valve1Address, PLCConfig.PrePumpValveAir, data);
            bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);

            Thread.Sleep(5000);
            bodHelper.PumpDrain();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //this.WindowState = WindowState.Maximized;

            //this.WindowStyle = WindowStyle.None;

            ////this.ResizeMode = ResizeMode.NoResize;


            //this.Topmost = true;

        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if ((bool)toggleButton.IsChecked)
            {
                StartBod(sender, e);
            }
            else
            {

                AbortBod(sender, e);
            }

        }


        public void BodRun(object sender, EventArgs e)
        {
            if (BodDetectRun != null && BodDetectRun.IsAlive)
            {
                BodDetectRun.Abort();
            }

            BodDetectRun = new Thread(new ThreadStart(bodHelper.StartBodDetect));
            BodDetectRun.IsBackground = true;
            BodDetectRun.Start();


        }


        public void AbortBod(object sender, RoutedEventArgs e)
        {
            bodHelper.manualevent.Reset();
        }

        public void StartBod(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BodDetectRun == null || !BodDetectRun.IsAlive || !bodHelper.IsSampling)
                {

                    initConfig();
                    RunTimer.Tick += BodRun;
                    RunTimer.Interval = new TimeSpan(0, configData.SpaceHour, 0, 0, 0);
                    RunTimer.Start();

                    BodRun(sender, e);

                    _loading.Visibility = Visibility.Visible;
                }
                else
                {
                    bodHelper.manualevent.Set();
                    _loading.Visibility = Visibility.Collapsed;

                }
            }
            catch (Exception)
            {

            }

        }


        public void initConfig()
        {
            if (string.IsNullOrEmpty(sampleSpac.Text) ||
                string.IsNullOrEmpty(InietTime.Text) ||
                string.IsNullOrEmpty(PrecipitateTime.Text) ||
                string.IsNullOrEmpty(EmptyTime.Text) ||
                string.IsNullOrEmpty(WarmUpTime.Text))
            {
                MessageBox.Show(" 流程配置有参数未设置,请设置后再启动.", "提示", MessageBoxButton.OK);
            }


            configData.SampDil = Convert.ToInt32(sampleSpac.Text);
            configData.SampVol = Convert.ToInt32(StandVol.Text);
            configData.StandDil = Convert.ToInt32(StandDil.Text);
            configData.StandVol = Convert.ToInt32(SampVol.Text);
            configData.EmptyTime = Convert.ToInt32(SampDil.Text);
            configData.InietTime = Convert.ToInt32(InietTime.Text);
            configData.PrecipitateTime = Convert.ToInt32(PrecipitateTime.Text);
            configData.SpaceHour = Convert.ToInt32(EmptyTime.Text);
            configData.WarmUpTime = Convert.ToInt32(WarmUpTime.Text);

            bodHelper.configData = configData;
        }

        private void start_Checked(object sender, RoutedEventArgs e)
        {
            string path = @"pack://application:,,,/Resources/zanting.png";
            BitmapImage image = new BitmapImage(new Uri(path, UriKind.Absolute));
            BodRunImg.Source = image;

            RunStatuLab.Content = "暂停运行";
        }

        private void start_Unchecked(object sender, RoutedEventArgs e)
        {
            string path = @"pack://application:,,,/Resources/icon_player.png";
            BitmapImage image = new BitmapImage(new Uri(path, UriKind.Absolute));
            BodRunImg.Source = image;

            RunStatuLab.Content = "开始运行";
        }



        private void restart_Click(object sender, RoutedEventArgs e)
        {
            _loading.Visibility = Visibility.Visible;
        }

        private void DrainEmpty_Click(object sender, RoutedEventArgs e)
        {
            byte[] Valves = { PLCConfig.NormalValveBit, PLCConfig.SampleValveBit };

            List<byte[]> data = new List<byte[]>();
            byte[] tem = { PLCConfig.SampleValveBit };
            byte[] tem1 = { PLCConfig.AirValveBit };
            data.Add(tem);
            data.Add(tem1);

            List<ushort> Address = new List<ushort>();
            Address.Add(PLCConfig.Valve2Address);
            Address.Add(PLCConfig.Valve2Address);

            foreach (var item in Valves)
            {
                data[0][0] = item;
                for (int i = 0; i < 10; i++)
                {
                    PumpProcess(data, Address, PunpCapType.fiveml);
                }
            }

            byte[] data1 = { PLCConfig.CisternValveBit };
            bodHelper.ValveControl(PLCConfig.Valve1Address, data1);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (SingleProcessTab == null)
            {
                return;
            }
            // mainWindow_Model.DebugMode = true;

            SingleProcessTab.Visibility = Visibility.Visible;
            SingleProcessTab.IsSelected = false;
            DevStaus.IsSelected = true;
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (SingleProcessTab == null)
            {
                return;
            }

            // mainWindow_Model.DebugMode = false;

            SingleProcessTab.Visibility = Visibility.Collapsed;
            SingleProcessTab.IsSelected = false;
            DevStaus.IsSelected = true;

        }

        private void WashValve_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { 0 };
            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);

        }

        private void BodSample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] data = { PLCConfig.WashValveBit, PLCConfig.SelectValveBit };
                bodHelper.ValveControl(PLCConfig.Valve1Address, data);

                SerialPortHelp serialPortHelp = new SerialPortHelp();
                serialPortHelp.OpenPort();
                serialPortHelp.StartSampleMes();
                serialPortHelp.ClosePort();
            }
            catch (Exception)
            {

            }

        }

        private void BodStand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] data = { PLCConfig.WashValveBit };
                bodHelper.ValveControl(PLCConfig.Valve1Address, data);
                SerialPortHelp serialPortHelp = new SerialPortHelp();
                serialPortHelp.OpenPort();
                serialPortHelp.StartStandMeas();
                serialPortHelp.ClosePort();
            }
            catch (Exception)
            {


            }


        }

        private void BodWash_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                byte[] data = { 0 };

                bodHelper.ValveControl(PLCConfig.Valve1Address, data);

                SerialPortHelp serialPortHelp = new SerialPortHelp();
                serialPortHelp.OpenPort();
                serialPortHelp.StartWash();
                serialPortHelp.ClosePort();
            }
            catch (Exception)
            {


            }

        }


        public void RefreshStatus(SysStatus sysStatus)
        {
            switch (sysStatus)
            {
                case SysStatus.Sampling:
                    start.IsChecked = true;
                    break;
                case SysStatus.Pause:
                    start.IsChecked = false;
                    break;
                case SysStatus.Complete:
                    start.IsChecked = false;
                    break;
                default:
                    break;
            }

        }

        private void GetBodData_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter streamWriter = File.CreateText("D:\\test1111.txt");
            try
            {
                byte[] data = { 0 };



                bodHelper.ValveControl(PLCConfig.Valve1Address, data);

                //bodHelper.serialPortHelp.OpenPort();
                streamWriter.WriteLine("开始读数据");

                bodHelper.serialPortHelp.ReadBodFun(RegStatus.CurrentBod, 3);

                data = bodHelper.serialPortHelp.ReadData();
                streamWriter.WriteLine("读取数据完成");

                if (data == null)
                {
                    streamWriter.WriteLine("data == null");

                    return;
                }

                int iLength = data.Length;
                foreach (var item in data)
                {
                    streamWriter.WriteLine(item.ToString());
                }

                if (iLength != 11)
                {

                    streamWriter.WriteLine("iLength != 11");
                    return;
                }

                int iCount = Convert.ToInt32(data[2]);

                byte[] BodValue = { data[3], data[4], data[5], data[6] };
                float Bod = Tool.ToInt32(BodValue)[0];


            //    float Bod = bodHelper.serialPortHelp.BodCurrentData();



                streamWriter.WriteLine(Bod.ToString());
                streamWriter.Close();

                //bodHelper.serialPortHelp.ClosePort();
            }
            catch (Exception ex)
            {
                streamWriter.WriteLine(ex.Message);
                streamWriter.WriteLine(ex.StackTrace);

                streamWriter.Close();
            }

        }

        private void UpdataData_Click(object sender, RoutedEventArgs e)
        {
            byte[] Temp = { PLCConfig.SensorPower };
            bool success = bodHelper.ValveControl(100, Temp);
            if (!success)
            {
                return;
            }
            //int warmUpTime = Convert.ToInt32(WarmUpTime.Text);
            //Thread.Sleep(warmUpTime * 1000);
            float[] DoDota = bodHelper.GetDoData();
            uint[] TurbidityData = bodHelper.GetTurbidityData();
            float[] PHData = bodHelper.GetPHData();
            ushort[] CODData = bodHelper.GetCodData();



            bodData.TemperatureData = DoDota[0]; 
            bodData.DoData = DoDota[1];
            bodData.TurbidityData = (float)TurbidityData[0] / 1000;
            bodData.PHData = PHData[1];
            bodData.CodData = (float)CODData[0] / 100;


            DOTem.Content = bodData.Uv254Data.ToString("F1");
            DO.Content = bodData.DoData.ToString("F1");
            PH.Content = bodData.PHData.ToString();
            COD.Content = bodData.CodData.ToString();
            PHTem.Content = bodData.TemperatureData.ToString();
            Turbidity.Content = bodData.TurbidityData.ToString();

            BOD.Content = bodData.Bod.ToString();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            timer.Stop();
            BodTimer.Stop();
            WaterSampleTimer.Stop();
            RunTimer.Stop();
            _loading.animationTimer.Stop();
            bodHelper.Dispose();
            if (BodDetectRun != null) 
            {
                BodDetectRun.Abort();

            }

            //DispatcherTimer timer = new DispatcherTimer();
            //DispatcherTimer BodTimer = new DispatcherTimer();
            //DispatcherTimer WaterSampleTimer = new DispatcherTimer();
            //DispatcherTimer RunTimer = new DispatcherTimer();
        }
    }
}
