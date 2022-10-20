using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

using RP_YOLO.YOLO.Models;
using System.Collections.ObjectModel;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.Threading;
using RP_YOLO.Model;
using System.Drawing.Imaging;
using RP_YOLO.YOLO;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RP_YOLO.View
{
    /// <summary>
    /// Window_SingleImageDetect.xaml 的交互逻辑
    /// </summary>
    public partial class Window_CameraStreamDetect : Window
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public ObservableCollection<DetectResult> detectResults;

        // 相机采集图像******
        private MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        private MyCamera m_MyCamera = new MyCamera();
        private bool m_bGrabbing = false;
        private Thread m_hReceiveThread = null;
        private MyCamera.MV_FRAME_OUT_INFO_EX m_stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
        private static object BufForDriverLock = new object();
        private IntPtr m_BufForFrame = IntPtr.Zero;
        // ch:用于从驱动获取图像的缓存 | en:Buffer for getting image from driver
        private uint m_nBufSizeForDriver = 0;

        YOLOV5<YoloV5FestoModel> yolov5;
        private bool m_isRunning = false; //运行flag

        public Window_CameraStreamDetect()
        {
            InitializeComponent();

            detectResults = new ObservableCollection<DetectResult>();
            dg_detectResult.DataContext = detectResults;

            // 初始化控件
            btn_connectCamera.IsEnabled = true;
            btn_disconnCamera.IsEnabled = false;
            btn_grabImage.IsEnabled = false;
            btn_stopGrabbing.IsEnabled = false;

            // 添加事件
            this.Closing += Window_CameraStreamDetect_Closing;

            // loadingMask
            bd_loadingMask.Visibility = Visibility.Collapsed;
        }

        private void Window_CameraStreamDetect_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 关闭程序前的操作
            // - 停止采集图像
            if (m_bGrabbing)
            {
                e.Cancel = true;
                System.Windows.MessageBox.Show("正在采集图像，无法退出程序。");
                return;
            }

            if (System.Windows.MessageBox.Show("确定退出程序吗？", "确定", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                e.Cancel = false;
                // - 断开相机连接
                DisconnCamera();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private async void btn_browse_modelFile_Click_Async(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "onnx files(*.onnx)|*.onnx" };
            openFileDialog.Title = "请选择模型onnx文件";

            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // 显示正在加载画面
                bd_loadingMask.Visibility = Visibility.Visible;

                string onnxPath = tbx_modelFile.Text = openFileDialog.FileName;
                // 加载模型
                if (await Task.Run(() => LoadModel(onnxPath)))
                {
                    bd_loadingMask.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool LoadModel(string onnxPath)
        {
            yolov5 = new YOLOV5<YoloV5FestoModel>(onnxPath);
            return yolov5 != null;
        }

        private void btn_run_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbx_modelFile.Text))
            {
                System.Windows.MessageBox.Show("请先选择onnx文件");
                return;
            }

            if (!m_isRunning)
            {
                m_isRunning = true;
                btn_run.Content = "停止";
                detectResults.Add(new DetectResult());
            }
            else
            {
                m_isRunning = false;
                btn_run.Content = "运行";
            }
        }

        private void RunDetect(Bitmap bitmap)
        {
            try
            {
                yolov5.ObjectDetect(bitmap, out DetectResult result);
                _ = Dispatcher.InvokeAsync(new Action(delegate
                  {
                      if (detectResults.Count == 0)
                      {
                          detectResults.Add(new DetectResult());
                      }
                      detectResults.RemoveAt(detectResults.Count - 1);
                      detectResults.Add(result);
                  }), System.Windows.Threading.DispatcherPriority.Render);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(System.IO.FileNotFoundException))
                {
                    System.Windows.MessageBox.Show("请选择源文件");
                }
            }
        }

        private void btn_searchCamera_Click(object sender, RoutedEventArgs e)
        {
            // ch:创建设备列表 | en:Create Device List
            System.GC.Collect();
            cbb_cameraList.Items.Clear();
            m_stDeviceList.nDeviceNum = 0;
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", 0);
                return;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                    if (gigeInfo.chUserDefinedName != "")
                    {
                        cbb_cameraList.Items.Add("GEV: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbb_cameraList.Items.Add("GEV: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (usbInfo.chUserDefinedName != "")
                    {
                        cbb_cameraList.Items.Add("U3V: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbb_cameraList.Items.Add("U3V: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                    }
                }
            }

            // ch:选择第一项 | en:Select the first item
            if (m_stDeviceList.nDeviceNum != 0)
            {
                cbb_cameraList.SelectedIndex = 0;
            }
        }
        private async void btn_connectCameraAsync_Click(object sender, RoutedEventArgs e)
        {
            if (m_stDeviceList.nDeviceNum == 0 || cbb_cameraList.SelectedIndex == -1)
            {
                ShowErrorMsg("No device, please select", 0);
                return;
            }

            // 显示正在加载画面
            bd_loadingMask.Visibility = Visibility.Visible;

            // 连接相机
            // ch:获取选择的设备信息 | en:Get selected device information
            MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                m_stDeviceList.pDeviceInfo[cbb_cameraList.SelectedIndex], typeof(MyCamera.MV_CC_DEVICE_INFO));

            bool connected = await Task.Run(() => OpenCamera(device));

            if (connected)
            {
                // 设置控件
                btn_connectCamera.IsEnabled = false;
                btn_disconnCamera.IsEnabled = true;
                btn_grabImage.IsEnabled = true;
                btn_stopGrabbing.IsEnabled = false;
                // - 触发模式
                GetTriggerMode();
                // - 获取相机参数
                txb_exposure.Text = GetCameraParamValue_Float("ExposureTime");
                txb_gain.Text = GetCameraParamValue_Float("Gain");
                txb_frameRate.Text = GetCameraParamValue_Float("ResultingFrameRate");
                // - 获取像素格式
                GetPixelFormats();

                // loadingMask
                bd_loadingMask.Visibility = Visibility.Collapsed;

            }
        }

        private bool OpenCamera(MyCamera.MV_CC_DEVICE_INFO device)
        {
            // ch:打开设备 | en:Open device
            int nRet = m_MyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                return false;
            }

            nRet = m_MyCamera.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                m_MyCamera.MV_CC_DestroyDevice_NET();
                ShowErrorMsg("Device open fail!", nRet);
                return false;
            }

            // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
            if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int nPacketSize = m_MyCamera.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    nRet = m_MyCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                    if (nRet != MyCamera.MV_OK)
                    {
                        ShowErrorMsg("Set Packet Size failed!", nRet);
                    }
                }
                else
                {
                    ShowErrorMsg("Get Packet Size failed!", nPacketSize);
                }
            }

            // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
            m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
            m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);

            return true;
        }

        private void GetPixelFormats()
        {
            MyCamera.MVCC_ENUMVALUE stParam = new MyCamera.MVCC_ENUMVALUE();
            int nRet = m_MyCamera.MV_CC_GetEnumValue_NET("PixelFormat", ref stParam);
            if (MyCamera.MV_OK == nRet)
            {
                // 获取所有支持的像素格式
                uint[] results = stParam.nSupportValue;

                for (int i = 0; i < stParam.nSupportedNum; i++)
                    cbb_pixelFormat.Items.Add((MyCamera.MvGvspPixelType)results[i]);
            }
            // 设置当前选择的像素格式
            cbb_pixelFormat.SelectedItem = (MyCamera.MvGvspPixelType)stParam.nCurValue;
        }

        private void GetTriggerMode()
        {
            MyCamera.MVCC_ENUMVALUE stParam = new MyCamera.MVCC_ENUMVALUE();
            int nRet = m_MyCamera.MV_CC_GetTriggerMode_NET(ref stParam);
            if (MyCamera.MV_OK == nRet)
            {
                if (stParam.nCurValue == (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF)
                    cbb_triggerMode.SelectedIndex = 0;
                else
                {
                    nRet = m_MyCamera.MV_CC_GetTriggerSource_NET(ref stParam);
                    if (MyCamera.MV_OK == nRet)
                    {
                        if (stParam.nCurValue == (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE)
                            cbb_triggerMode.SelectedIndex = 1;
                        if (stParam.nCurValue == (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0)
                            cbb_triggerMode.SelectedIndex = 2;
                    }
                }
            }
        }

        private string GetCameraParamValue_Float(string paramKey)
        {
            string result = "";

            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_MyCamera.MV_CC_GetFloatValue_NET(paramKey, ref stParam);
            if (MyCamera.MV_OK == nRet)
            {
                result = stParam.fCurValue.ToString("F1");
            }

            return result;
        }

        private int GetCameraParamValue_Enum(string paramKey)
        {
            int result = -1;

            MyCamera.MVCC_ENUMVALUE stParam = new MyCamera.MVCC_ENUMVALUE();
            int nRet = m_MyCamera.MV_CC_GetEnumValue_NET(paramKey, ref stParam);
            if (MyCamera.MV_OK == nRet)
            {
                result = (int)stParam.nCurValue;
            }

            return result;
        }

        private void SetCameraParamValue_Float(string paramKey, string strValue)
        {
            if (!string.IsNullOrEmpty(strValue))
            {
                m_MyCamera.MV_CC_SetFloatValue_NET(paramKey, float.Parse(strValue));
            }
        }

        private void SetCameraParamValue_Enum(string paramKey, uint value)
        {
            m_MyCamera.MV_CC_SetEnumValue_NET(paramKey, value);
        }

        private void btn_disconnCamera_Click(object sender, RoutedEventArgs e)
        {
            DisconnCamera();

            // 设置控件
            // - 按钮
            btn_connectCamera.IsEnabled = true;
            btn_disconnCamera.IsEnabled = false;
            btn_grabImage.IsEnabled = false;
            btn_stopGrabbing.IsEnabled = false;
            // - 图像
            uct_image.ShowImage("");
            // - 文本框
            txb_exposure.Text = "";
            txb_frameRate.Text = "";
            txb_gain.Text = "";
            // - 下拉框
            cbb_pixelFormat.Items.Clear();
        }

        private void DisconnCamera()
        {
            // ch:取流标志位清零 | en:Reset flow flag bit
            if (m_bGrabbing == true)
            {
                m_bGrabbing = false;
                m_hReceiveThread.Join();
            }

            if (m_BufForFrame != IntPtr.Zero)
            {
                Marshal.Release(m_BufForFrame);
            }

            // ch:关闭设备 | en:Close Device
            m_MyCamera.MV_CC_CloseDevice_NET();
            m_MyCamera.MV_CC_DestroyDevice_NET();
        }

        private void cbb_cameraList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btn_grabImage_Click(object sender, RoutedEventArgs e)
        {
            // ch:标志位置位true | en:Set position bit true
            m_bGrabbing = true;

            m_hReceiveThread = new Thread(ReceiveThreadProcess);
            m_hReceiveThread.Start();

            m_stFrameInfo.nFrameLen = 0;//取流之前先清除帧长度
            m_stFrameInfo.enPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined;
            // ch:开始采集 | en:Start Grabbing
            int nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                m_bGrabbing = false;
                m_hReceiveThread.Join();
                ShowErrorMsg("Start Grabbing Fail!", nRet);
                return;
            }

            // 设置控件
            btn_connectCamera.IsEnabled = false;
            btn_disconnCamera.IsEnabled = true;
            btn_grabImage.IsEnabled = false;
            btn_stopGrabbing.IsEnabled = true;
        }

        private void btn_stopGrabbing_Click(object sender, RoutedEventArgs e)
        {
            // ch:标志位设为false | en:Set flag bit false
            m_bGrabbing = false;
            m_isRunning = false;
            m_hReceiveThread.Join();

            // ch:停止采集 | en:Stop Grabbing
            int nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Stop Grabbing Fail!", nRet);
            }
            // 设置控件
            btn_connectCamera.IsEnabled = true;
            btn_disconnCamera.IsEnabled = true;
            btn_grabImage.IsEnabled = true;
            btn_stopGrabbing.IsEnabled = false;
        }

        public void ReceiveThreadProcess()
        {
            //获取 Payload Size
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            int nRet = m_MyCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);

            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("获取负载大小失败。", nRet);
                return;
            }
            uint nPayloadSize = stParam.nCurValue;
            //如果负载更大，重新分配缓存
            if (nPayloadSize > m_nBufSizeForDriver)
            {
                if (m_BufForFrame != IntPtr.Zero)
                    Marshal.Release(m_BufForFrame);
                m_nBufSizeForDriver = nPayloadSize;
                m_BufForFrame = Marshal.AllocHGlobal((int)m_nBufSizeForDriver);
            }
            if (m_BufForFrame == IntPtr.Zero)
            {
                return;
            }

            MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();

            while (m_bGrabbing)
            {
                lock (BufForDriverLock)
                {
                    nRet = m_MyCamera.MV_CC_GetOneFrameTimeout_NET(m_BufForFrame, nPayloadSize, ref stFrameInfo, 1000);
                    if (nRet == MyCamera.MV_OK)
                    {
                        m_stFrameInfo = stFrameInfo;
                    }
                }

                if (nRet == MyCamera.MV_OK)
                {
                    if (RemoveCustomPixelFormats(stFrameInfo.enPixelType))
                    {
                        continue;
                    }

                    Bitmap bitmap = new Bitmap(m_stFrameInfo.nWidth, m_stFrameInfo.nHeight, PixelFormat.Format24bppRgb);
                    Rectangle rect = new Rectangle(0, 0, m_stFrameInfo.nWidth, m_stFrameInfo.nHeight);
                    BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    unsafe
                    {
                        Buffer.MemoryCopy(m_BufForFrame.ToPointer(), bitmapData.Scan0.ToPointer(), m_nBufSizeForDriver, m_nBufSizeForDriver);
                    }
                    bitmap.UnlockBits(bitmapData);


                    if (m_isRunning)
                    {
                        RunDetect(bitmap);
                    }
                    // 异步调用显示图像
                    _ = Dispatcher.InvokeAsync(new Action(delegate
                      {
                          uct_image.ShowImage(bitmap);
                      }), System.Windows.Threading.DispatcherPriority.Render);
                }
            }
        }


        // ch:去除自定义的像素格式 | en:Remove custom pixel formats
        private bool RemoveCustomPixelFormats(MyCamera.MvGvspPixelType enPixelFormat)
        {
            Int32 nResult = ((int)enPixelFormat) & (unchecked((Int32)0x80000000));
            if (0x80000000 == nResult)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // ch:显示错误信息 | en:Show error message
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: errorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: errorMsg += " Network error "; break;
            }

            System.Windows.MessageBox.Show(errorMsg, "PROMPT");
        }

        private void txb_exposure_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_MyCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
            SetCameraParamValue_Float("ExposureTime", txb_exposure.Text);
        }

        private void txb_gain_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_MyCamera.MV_CC_SetEnumValue_NET("GainAuto", 0);
            SetCameraParamValue_Float("Gain", txb_gain.Text);
        }

        private void lsb_triggerMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_softTrigger.IsEnabled = false;

            // cbb_triggerMode.Items
            // - 0 连续
            // - 1 软触发
            // - 2 Line0
            if (cbb_triggerMode.SelectedIndex == 0)
            {
                m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
            }
            else
            {
                // ch:打开触发模式 | en:Open Trigger Mode
                m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);

                // ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0;
                //           1 - Line1;
                //           2 - Line2;
                //           3 - Line3;
                //           4 - Counter;
                //           7 - Software;
                if (cbb_triggerMode.SelectedIndex == 1)
                {
                    m_MyCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                    btn_softTrigger.IsEnabled = true;
                }
                else if (cbb_triggerMode.SelectedIndex == 2)
                {
                    m_MyCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
                }
            }
        }

        private void btn_softTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (!m_bGrabbing || cbb_triggerMode.SelectedIndex != 1)
                return;

            // ch:触发命令 | en:Trigger command
            int nRet = m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Trigger Software Fail!", nRet);
            }
        }

        private void cbb_pixelFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbb_pixelFormat.Items.IsEmpty)
                return;

            int nRet = m_MyCamera.MV_CC_SetPixelFormat_NET((uint)(MyCamera.MvGvspPixelType)cbb_pixelFormat.SelectedItem);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Fail to change pixel format!", nRet);
            }
        }
    }
}
