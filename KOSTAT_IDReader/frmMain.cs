using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Xml;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Discovery;
using Cognex.DataMan.SDK.Utils;

namespace KOSTAT_IDReader
{
    public partial class frmMain : Form
    {
        DataManSystem _system = null;
        EthSystemConnector _CamConnector = null;
        ResultCollector _results;
        SynchronizationContext _syncContext = null;
        private CNITcpServer _tcpServer;

        // 시간 표시를 위한 타이머 추가
        private System.Timers.Timer dateTimeTimer;

        // DataGridView 메모리 관리를 위한 최대 행 수 설정
        private const int MAX_DATAGRIDVIEW_ROWS = 1000; // 최대 1000행으로 제한
        private const int ROWS_TO_REMOVE = 200; // 한 번에 삭제할 행 수

        // 매칭 시퀀스를 위한 클래스 변수들
        private Queue<string[]> tcpDataQueue = new Queue<string[]>();
        private object queueLock = new object();

        public frmMain()
        {
            InitializeComponent();
            _syncContext = WindowsFormsSynchronizationContext.Current;

            // 자동 트리거 타이머 초기화
            InitializeAutoTriggerTimer();
        }

        #region Form Events
        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                CNILog.Message += CNILog_Message;

                // 시간 표시 타이머 초기화
                InitializeDateTimeTimer();

                if (!CNIParams.Load())
                {
                    CNILog.Write("Load Ini File Error", true);
                    tb_Path.Text = CNIParams.SaveImagePath;
                    nd_NRead.Value = 4;
                }
                else
                {
                    // Dataman 카메라 초기화
                    InitializeDatamanCamera();

                    // TCP 서버 초기화
                    InitializeTcpServer();

                    tb_Path.Text = CNIParams.SaveImagePath;
                    nd_NRead.Value = CNIParams.ReadCount;

                    //_system.SendCommand("");
                }
            }
            catch (Exception ex)
            {
                CNILog.Write($"Form Load Error: {ex.Message}", true);
            }
        }
        // 시간 표시 타이머 초기화 메서드 추가 (적절한 위치에)
        private void InitializeDateTimeTimer()
        {
            try
            {
                // 타이머 생성 및 설정
                dateTimeTimer = new System.Timers.Timer();
                dateTimeTimer.Interval = 1000; // 1초마다 업데이트
                dateTimeTimer.Elapsed += DateTimeTimer_Tick;
                // 초기 시간 설정
                UpdateDateTime();

                // 타이머 시작
                dateTimeTimer.Start();

                CNILog.Write("DateTime 타이머 초기화 완료", true);
            }
            catch (Exception ex)
            {
                CNILog.Write($"DateTime 타이머 초기화 오류: {ex.Message}", true);
            }
        }
        // 타이머 이벤트 핸들러
        private void DateTimeTimer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }
        // 시간 업데이트 메서드
        private void UpdateDateTime()
        {
            try
            {
                if (lb_DateTime.InvokeRequired)
                {
                    lb_DateTime.Invoke(new Action(UpdateDateTime));
                    return;
                }

                // 현재 시간을 yyyy/MM/dd HH:mm:ss 형식으로 표시
                lb_DateTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                CNILog.Write($"DateTime 업데이트 오류: {ex.Message}", true);
            }
        }
        private void InitializeDatamanCamera()
        {
            _CamConnector = new EthSystemConnector(IPAddress.Parse(CNIParams.CamIP), CNIParams.CamPort);
            _CamConnector.UserName = "admin";
            _CamConnector.Password = "";

            _system = new DataManSystem(_CamConnector);
            _system.DefaultTimeout = 10000;

            _system.SystemConnected += new SystemConnectedHandler(OnSystemConnected);
            _system.SystemDisconnected += new SystemDisconnectedHandler(OnSystemDisconnected);

            ResultTypes requested_result_types = ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;
            _results = new ResultCollector(_system, requested_result_types);
            _results.ComplexResultCompleted += Results_ComplexResultCompleted;
            _results.SimpleResultDropped += Results_SimpleResultDropped;

            _system.SetKeepAliveOptions(true, 3000, 1000);
            _system.Connect();

            try
            {
                _system.SetResultTypes(requested_result_types);
            }
            catch (Exception ex)
            {
                CNILog.Write($"Set Result Types Error: {ex.Message}", true);
            }
        }
        private void InitializeTcpServer()
        {
            IPAddress ipAddress = IPAddress.Parse(CNIParams.LaserIP);
            _tcpServer = new CNITcpServer(ipAddress, CNIParams.LaserPort);

            _tcpServer.Connected += TcpServer_Connected;
            _tcpServer.Disconnected += TcpServer_Disconnected;
            _tcpServer.Received += TcpServer_Received;
            _tcpServer.Message += TcpServer_Message;

            _tcpServer.Listen();
        }
        private void CleanupConnection()
        {
            if (null != _system)
            {
                _system.SystemConnected -= OnSystemConnected;
                _system.SystemDisconnected -= OnSystemDisconnected;
            }

            _CamConnector = null;
            _system = null;

            _tcpServer?.Dispose();
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            CleanupConnection();
        }
        #endregion

        #region TCP Server Events
        private void TcpServer_Connected(string ip)
        {
            CNILog.Write($"Client connected from {ip}", true);
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { lb_bLaserCon.ForeColor = Color.Lime; }));
            }
            else
            {
                lb_bLaserCon.ForeColor = Color.Lime;
            }
        }
        private void TcpServer_Disconnected()
        {
            CNILog.Write("Client disconnected", true);
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { lb_bLaserCon.ForeColor = Color.Red; }));
            }
            else
            {
                lb_bLaserCon.ForeColor = Color.Red;
            }
        }
        private void TcpServer_Received(string data)
        {
            try
            {
                CNILog.Write($"TCP 데이터 수신: " + data, true);

                // "RESET" 명령어 처리
                if (data.Trim().ToUpper() == "RESET")
                {
                    lock (queueLock)
                    {
                        tcpDataQueue.Clear();
                        CNILog.Write("RESET 명령어 수신 - 모든 TCP 데이터 큐 클리어 완료", true);
                    }
                    return;
                }

                string[] dataSet = data.Split(',');

                // TCP 데이터를 미래 단계로 큐에 저장
                lock (queueLock)
                {
                    tcpDataQueue.Enqueue(dataSet);

                    // 큐 크기 제한 (최대 10개 유지)
                    while (tcpDataQueue.Count > 50)
                    {
                        tcpDataQueue.Dequeue();
                    }
                }

                // TCP 데이터를 즉시 그리드에 추가하지 않음 - 바코드 리딩 시점에 추가
            }
            catch (Exception ex)
            {
                CNILog.Write($"TCP 데이터 처리 오류: {ex.Message}", true);
            }
        }
        private void TcpServer_Message(string message)
        {
            CNILog.Write($"Server message: {message}", true);
        }
        #endregion

        #region Dataman Events...
        private void OnSystemConnected(object sender, EventArgs args)
        {
            _syncContext.Post(
            delegate
            {
                lb_CamName.Text = "DM474";
                lb_CamName.ForeColor = Color.Black;
                lb_bCamCon.ForeColor = Color.Lime;
            },
            null);
        }
        private void OnSystemDisconnected(object sender, EventArgs args)
        {
            _syncContext.Post(
            delegate
            {
                lb_bCamCon.ForeColor = Color.Red;
            },
            null);
        }
        private void Results_ComplexResultCompleted(object sender, ComplexResult e)
        {
            _syncContext.Post(
            delegate
            {
                ShowResult(e);
            },
            null);
        }
        private void Results_SimpleResultDropped(object sender, SimpleResult e)
        {
            _syncContext.Post(
            delegate
            {
                ReportDroppedResult(e);
            },
            null);
        }
        private void ReportDroppedResult(SimpleResult result)
        {
            string str = string.Format("Partial result dropped: {0}, id={1}", result.Id.Type.ToString(), result.Id.Id);
            CNILog.Write(str, true);
        }
        #endregion

        #region Data Processing...
        private string[] ProcessBarcodeData(string[] barcodeData)
        {
            // 이미지 크기 정보 가져오기 (MainDisplay 또는 현재 이미지에서)
            float imageWidth = 0;
            if (MainDisplay.Image != null)
            {
                imageWidth = 2048;
            }
            else
            {
                // 기본값 설정 (필요에 따라 조정)
                imageWidth = 2048;
            }

            // x축을 N등분한 구간 크기 계산
            float sectionWidth = imageWidth / CNIParams.ReadCount;

            // 결과 배열 초기화 - 항상 N_Read 크기로 생성
            string[] processedData = new string[CNIParams.ReadCount];

            // 각 구간을 "NG"로 초기화 (모자란 위치는 자동으로 NG)
            for (int i = 0; i < CNIParams.ReadCount; i++)
            {
                processedData[i] = "NG";
            }

            // 바코드 데이터를 x축 기준으로 해당 구간에 배치
            foreach (string data in barcodeData)
            {
                if (!string.IsNullOrEmpty(data) && data != "0")
                {
                    string[] parts = data.Split('_');
                    if (parts.Length >= 2)
                    {
                        // 첫 번째 코너의 x 좌표를 기준으로 사용
                        string[] cornerCoords = parts[1].Split(',');
                        if (cornerCoords.Length >= 2 && float.TryParse(cornerCoords[0], out float x))
                        {
                            // x 좌표가 속하는 구간 계산 (x축 기준 N등분)
                            int sectionIndex = (int)(x / sectionWidth);

                            // 구간 인덱스가 유효한 범위 내에 있는지 확인
                            if (sectionIndex >= 0 && sectionIndex < CNIParams.ReadCount)
                            {
                                // 해당 구간에 실제 바코드 데이터 배치
                                if (!string.IsNullOrEmpty(parts[0]) && parts[0].Trim() != "")
                                {
                                    // 인덱스 충돌 처리: 해당 인덱스가 이미 사용 중이면 다음 빈 인덱스 찾기
                                    int targetIndex = sectionIndex;
                                    while (targetIndex < CNIParams.ReadCount && processedData[targetIndex] != "NG")
                                    {
                                        targetIndex++;
                                    }

                                    // 빈 인덱스를 찾았으면 배치
                                    if (targetIndex < CNIParams.ReadCount)
                                    {
                                        processedData[targetIndex] = parts[0].Trim();
                                        if (targetIndex != sectionIndex)
                                        {
                                            CNILog.Write($"인덱스 충돌로 인해 구간 {sectionIndex + 1}에서 구간 {targetIndex + 1}로 이동하여 바코드 '{parts[0].Trim()}' 배치 (x좌표: {x})", true);
                                        }
                                        else
                                        {
                                            CNILog.Write($"구간 {targetIndex + 1}에 바코드 '{parts[0].Trim()}' 배치 (x좌표: {x})", true);
                                        }
                                    }
                                    else
                                    {
                                        CNILog.Write($"모든 구간이 사용 중이어서 바코드 '{parts[0].Trim()}'를 배치할 수 없음 (x좌표: {x})", true);
                                    }
                                }
                                else
                                {
                                    // 빈 바코드의 경우 원래 인덱스에 NG 유지
                                    CNILog.Write($"구간 {sectionIndex + 1}에 빈 바코드로 인해 NG 유지 (x좌표: {x})", true);
                                }
                            }
                            else
                            {
                                CNILog.Write($"바코드 x좌표 {x}가 유효 범위를 벗어남 (구간 인덱스: {sectionIndex})", true);
                            }
                        }
                        else
                        {
                            CNILog.Write($"바코드 좌표 파싱 실패: {data}", true);
                        }
                    }
                    else
                    {
                        CNILog.Write($"바코드 데이터 형식 오류: {data}", true);
                    }
                }
            }

            // 최종 결과 확인 - 항상 N_Read 크기 보장
            if (processedData.Length != CNIParams.ReadCount)
            {
                CNILog.Write($"오류: processedData 크기가 N_Read({CNIParams.ReadCount})와 다름: {processedData.Length}", true);
                // 크기 보정
                string[] correctedData = new string[CNIParams.ReadCount];
                for (int i = 0; i < CNIParams.ReadCount; i++)
                {
                    correctedData[i] = i < processedData.Length ? processedData[i] : "NG";
                }
                processedData = correctedData;
            }

            // 로그 출력 (디버깅용)
            CNILog.Write($"이미지 폭: {imageWidth}, 구간 크기: {sectionWidth}, 총 구간 수: {CNIParams.ReadCount}", true);
            CNILog.Write("=== 최종 처리된 바코드 데이터 (x축 기준 정렬) ===", true);
            for (int i = 0; i < processedData.Length; i++)
            {
                float startX = i * sectionWidth;
                float endX = (i + 1) * sectionWidth;
                CNILog.Write($"구간 {i + 1} (x: {startX:F1}~{endX:F1}): {processedData[i]}", true);
            }
            CNILog.Write($"총 {processedData.Length}개 구간 처리 완료", true);

            return processedData;
        }

        private object _currentResultInfoSyncLock = new object();
        private void ShowResult(ComplexResult complexResult)
        {
            List<Image> images = new List<Image>();
            List<string> image_graphics = new List<string>();
            string read_result = null;
            int result_id = -1;
            ResultTypes collected_results = ResultTypes.None;

            lock (_currentResultInfoSyncLock)
            {
                foreach (var simple_result in complexResult.SimpleResults)
                {
                    collected_results |= simple_result.Id.Type;

                    switch (simple_result.Id.Type)
                    {
                        case ResultTypes.Image:
                            Image image = ImageArrivedEventArgs.GetImageFromImageBytes(simple_result.Data);
                            if (image != null)
                                images.Add(image);
                            break;
                        case ResultTypes.ImageGraphics:
                            image_graphics.Add(simple_result.GetDataAsString());
                            break;
                        case ResultTypes.ReadXml:
                            read_result = GetReadStringFromResultXml(simple_result.GetDataAsString());
                            result_id = simple_result.Id.Id;
                            break;
                        case ResultTypes.ReadString:
                            read_result = simple_result.GetDataAsString();
                            result_id = simple_result.Id.Id;
                            break;
                    }
                }
            }

            if (read_result != null)
            {
                string[] originalBarcodeData = read_result.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string[] processedBarcodeData = ProcessBarcodeData(originalBarcodeData);

                // 매칭할 TCP 데이터 찾기
                string[] tcpMatchingData = null;
                lock (queueLock)
                {
                    if (tcpDataQueue.Count > 3)
                    {
                        tcpMatchingData = tcpDataQueue.Dequeue();
                    }
                }

                if (images.Count > 0)
                {
                    // 이미지가 있는 경우 - 매칭 수행 및 이미지 처리
                    ProcessImageWithMatching(images[0], originalBarcodeData, processedBarcodeData, tcpMatchingData);
                }
                else
                {
                    // 이미지가 없는 경우 - 매칭만 수행
                    ProcessMatching(processedBarcodeData, tcpMatchingData);
                }
            }
        }
        private void ProcessImageWithMatching(Image image, string[] originalBarcodeData, string[] processedBarcodeData, string[] tcpMatchingData)
        {
            try
            {
                Bitmap originalBitmap = new Bitmap(image);
                Bitmap fittedBitmap = new Bitmap(originalBitmap);

                bool isMatch = false;
                if (tcpMatchingData != null)
                {
                    isMatch = PerformMatching(processedBarcodeData, tcpMatchingData);

                    // 매칭 결과를 DataGridView에 추가
                    UpdateDataGridViewWithMatchingResult(processedBarcodeData, tcpMatchingData, isMatch);
                }
                else
                {
                    // TCP 데이터가 없는 경우 바코드 데이터만 추가
                    UpdateDataGridViewWithBarcodeOnly(processedBarcodeData);
                }

                // 이미지에 폴리곤 그리기 - TCP 데이터를 전달하여 개별 매칭 확인
                DrawPolygonsOnImage(fittedBitmap, originalBitmap, originalBarcodeData, tcpMatchingData);

                // 이미지 표시
                DisplayImage(fittedBitmap);
            }
            catch (Exception ex)
            {
                CNILog.Write($"이미지 처리 오류: {ex.Message}", true);
            }
        }
        private void ProcessMatching(string[] processedBarcodeData, string[] tcpMatchingData)
        {
            try
            {
                if (tcpMatchingData != null)
                {
                    bool isMatch = PerformMatching(processedBarcodeData, tcpMatchingData);

                    // 매칭 결과를 DataGridView에 추가
                    UpdateDataGridViewWithMatchingResult(processedBarcodeData, tcpMatchingData, isMatch);
                }
                else
                {
                    // TCP 데이터가 없는 경우 바코드 데이터만 추가
                    UpdateDataGridViewWithBarcodeOnly(processedBarcodeData);
                }
            }
            catch (Exception ex)
            {
                CNILog.Write($"매칭 처리 오류: {ex.Message}", true);
            }
        }
        private bool PerformMatching(string[] barcodeData, string[] tcpData)
        {
            try
            {
                // 유효한 바코드 데이터 추출 ("0"과 "NG" 제외)
                var validBarcodeData = barcodeData.Where(data => !string.IsNullOrEmpty(data) && data != "0" && data != "NG")
                                                 .Select(data => data.ToUpper().Trim())
                                                 .ToList();

                // 유효한 TCP 데이터 추출 (빈 값 제외)
                var validTcpData = tcpData.Where(data => !string.IsNullOrEmpty(data))
                                          .Select(data => data.ToUpper().Trim())
                                          .ToList();

                CNILog.Write($"매칭 비교 - 바코드: [{string.Join(", ", validBarcodeData)}], TCP: [{string.Join(", ", validTcpData)}]", true);

                // 모든 바코드 데이터가 TCP 데이터에 포함되어 있는지 확인
                bool isMatch = validBarcodeData.Count > 0 && validBarcodeData.All(barcode => validTcpData.Contains(barcode));

                CNILog.Write($"매칭 결과: {(isMatch ? "성공" : "실패")}", true);

                return isMatch;
            }
            catch (Exception ex)
            {
                CNILog.Write($"매칭 수행 오류: {ex.Message}", true);
                return false;
            }
        }
        //데이터 그리드 데이터 업데이트
        private void UpdateDataGridViewWithMatchingResult(string[] barcodeData, string[] tcpData, bool isMatch)
        {
            try
            {
                if (dgv_Data.InvokeRequired)
                {
                    dgv_Data.Invoke(new Action(() => UpdateDataGridViewWithMatchingResult(barcodeData, tcpData, isMatch)));
                    return;
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 유효한 TCP 데이터 리스트 생성 (매칭용)
                var validTcpData = tcpData.Where(data => !string.IsNullOrEmpty(data))
                                          .Select(data => data.ToUpper().Trim())
                                          .ToList();

                // N_Read 크기만큼 행을 추가 (배열 구조에 맞게)
                for (int i = 0; i < CNIParams.ReadCount; i++)
                {
                    string barcodeValue = i < barcodeData.Length ? barcodeData[i] : "NG";
                    string tcpValue = i < tcpData.Length ? tcpData[i] : "";

                    // 개별 매칭 상태 확인 - 전체 TCP 데이터와 비교
                    string status = "";
                    Color backgroundColor = Color.White;

                    if (!string.IsNullOrEmpty(barcodeValue) && barcodeValue != "NG")
                    {
                        // 바코드가 유효한 경우 - 전체 TCP 데이터에서 찾기
                        bool foundInTcp = validTcpData.Contains(barcodeValue.ToUpper().Trim());

                        if (foundInTcp)
                        {
                            status = "매칭 성공";
                            backgroundColor = Color.LightGreen;
                        }
                        else
                        {
                            status = "매칭 실패";
                            backgroundColor = Color.LightCoral;
                        }
                    }
                    else if (barcodeValue == "NG")
                    {
                        status = "바코드 NG";
                        backgroundColor = Color.LightGray;
                    }
                    else
                    {
                        status = "데이터 없음";
                        backgroundColor = Color.LightGray;
                    }

                    int rowIndex = dgv_Data.Rows.Add(timestamp, tcpValue, barcodeValue, status);
                    dgv_Data.Rows[rowIndex].DefaultCellStyle.BackColor = backgroundColor;
                }

                // 메모리 관리 실행
                ManageDataGridViewMemory();

                // 마지막 행으로 스크롤
                dgv_Data.FirstDisplayedScrollingRowIndex = dgv_Data.RowCount - 1;

                // 데이터를 파일에 저장
                SaveDataGridViewToFile();

                CNILog.Write($"매칭 결과 DataGridView 업데이트 완료 - {CNIParams.ReadCount}개 행 추가, 전체 매칭: {(isMatch ? "성공" : "실패")}", true);
            }
            catch (Exception ex)
            {
                CNILog.Write($"매칭 결과 DataGridView 업데이트 오류: {ex.Message}", true);
            }
        }
        private void UpdateDataGridViewWithBarcodeOnly(string[] barcodeData)
        {
            try
            {
                if (dgv_Data.InvokeRequired)
                {
                    dgv_Data.Invoke(new Action(() => UpdateDataGridViewWithBarcodeOnly(barcodeData)));
                    return;
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // N_Read 크기만큼 행을 추가 (배열 구조에 맞게)
                for (int i = 0; i < CNIParams.ReadCount; i++)
                {
                    string barcodeValue = i < barcodeData.Length ? barcodeData[i] : "NG";

                    string status = "";
                    Color backgroundColor = Color.White;

                    if (!string.IsNullOrEmpty(barcodeValue) && barcodeValue != "NG")
                    {
                        // 유효한 바코드 데이터가 있는 경우
                        status = "매칭 데이터 없음";
                        backgroundColor = Color.LightYellow;
                    }
                    else
                    {
                        // 바코드가 없거나 NG인 경우
                        status = barcodeValue == "NG" ? "바코드 NG" : "바코드 읽기 실패";
                        backgroundColor = Color.LightGray;
                    }

                    int rowIndex = dgv_Data.Rows.Add(timestamp, "", barcodeValue, status);
                    dgv_Data.Rows[rowIndex].DefaultCellStyle.BackColor = backgroundColor;
                }

                // 메모리 관리 실행
                ManageDataGridViewMemory();

                // 마지막 행으로 스크롤
                dgv_Data.FirstDisplayedScrollingRowIndex = dgv_Data.RowCount - 1;

                // 데이터를 파일에 저장
                SaveDataGridViewToFile();

                CNILog.Write($"바코드 전용 DataGridView 업데이트 완료 - {CNIParams.ReadCount}개 행 추가", true);
            }
            catch (Exception ex)
            {
                CNILog.Write($"바코드 전용 DataGridView 업데이트 오류: {ex.Message}", true);
            }
        }
        // DataGridView 메모리 관리 메서드 추가 (적절한 위치에)
        private void ManageDataGridViewMemory()
        {
            try
            {
                if (dgv_Data.InvokeRequired)
                {
                    dgv_Data.Invoke(new Action(ManageDataGridViewMemory));
                    return;
                }

                // 최대 행 수를 초과하면 오래된 행들을 삭제
                if (dgv_Data.Rows.Count > MAX_DATAGRIDVIEW_ROWS)
                {
                    int rowsToRemove = Math.Min(ROWS_TO_REMOVE, dgv_Data.Rows.Count - MAX_DATAGRIDVIEW_ROWS + ROWS_TO_REMOVE);

                    // 상위 행들부터 삭제 (오래된 데이터부터)
                    for (int i = 0; i < rowsToRemove && dgv_Data.Rows.Count > 0; i++)
                    {
                        dgv_Data.Rows.RemoveAt(0);
                    }

                    CNILog.Write($"DataGridView 메모리 관리: {rowsToRemove}개 행 삭제, 현재 행 수: {dgv_Data.Rows.Count}", true);

                    // 가비지 컬렉션 강제 실행 (선택적)
                    if (dgv_Data.Rows.Count % 500 == 0) // 500행마다 한 번씩
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        CNILog.Write("가비지 컬렉션 실행 완료", true);
                    }
                }
            }
            catch (Exception ex)
            {
                CNILog.Write($"DataGridView 메모리 관리 오류: {ex.Message}", true);
            }
        }
        private void DrawPolygonsOnImage(Bitmap fittedImage, Bitmap originalImage, string[] barcodeData, string[] tcpData)
        {
            float scaleX = (float)fittedImage.Width / originalImage.Width;
            float scaleY = (float)fittedImage.Height / originalImage.Height;

            // TCP 데이터를 대문자로 변환하여 비교용으로 준비
            var tcpDataUpper = tcpData?.Where(data => !string.IsNullOrEmpty(data))
                                      .Select(data => data.ToUpper().Trim())
                                      .ToList() ?? new List<string>();

            bool hasNgOrFailure = false; // NG 또는 매칭 실패 감지 플래그
            List<string> barcodeValues = new List<string>(); // 바코드 값들을 저장할 리스트

            using (Graphics g = Graphics.FromImage(fittedImage))
            {
                foreach (string data in barcodeData)
                {
                    if (data != "0")
                    {
                        string[] parts = data.Split('_');
                        if (parts.Length >= 5)
                        {
                            // 바코드 값 추출
                            string barcodeValue = parts[0].ToUpper().Trim();
                            barcodeValues.Add(barcodeValue); // 리스트에 추가

                            // 개별 바코드가 TCP 데이터에 있는지 확인
                            bool isIndividualMatch = !string.IsNullOrEmpty(barcodeValue) &&
                                                   barcodeValue != "NG" &&
                                                   tcpDataUpper.Contains(barcodeValue);

                            // NG 또는 매칭 실패 감지
                            if (barcodeValue == "NG" || !isIndividualMatch)
                            {
                                hasNgOrFailure = true;
                            }

                            // 매칭되지 않은 바코드만 빨간색, 매칭된 바코드는 초록색
                            Color boxColor = isIndividualMatch ? Color.Lime : Color.Red;

                            PointF[] polygonPoints = new PointF[4];
                            bool allPointsValid = true;

                            for (int i = 0; i < 4; i++)
                            {
                                string[] cornerCoords = parts[i + 1].Split(',');
                                if (cornerCoords.Length >= 2 &&
                                    float.TryParse(cornerCoords[0], out float x) &&
                                    float.TryParse(cornerCoords[1], out float y))
                                {
                                    polygonPoints[i] = new PointF(x * scaleX, y * scaleY);
                                }
                                else
                                {
                                    allPointsValid = false;
                                    break;
                                }
                            }

                            if (allPointsValid)
                            {
                                using (Pen pen = new Pen(boxColor, 2))
                                {
                                    g.DrawPolygon(pen, polygonPoints);
                                }
                            }
                        }
                    }
                }

                // 바코드 데이터를 좌상단에 순서대로 표시
                if (barcodeValues.Count > 0)
                {
                    // 4배 크기의 폰트 생성
                    using (Font largeFont = new Font(SystemFonts.DefaultFont.FontFamily, SystemFonts.DefaultFont.Size * 4, FontStyle.Bold))
                    {
                        float startX = 10; // 좌상단 시작 X 위치
                        float startY = 10; // 좌상단 시작 Y 위치
                        float lineHeight = largeFont.Height + 5; // 줄 간격

                        for (int i = 0; i < barcodeValues.Count; i++)
                        {
                            string barcodeValue = barcodeValues[i];

                            // 텍스트 위치 계산
                            float textX = startX;
                            float textY = startY + (i * lineHeight);

                            // 텍스트 크기 측정
                            SizeF textSize = g.MeasureString(barcodeValue, largeFont);

                            // 매칭 상태에 따른 배경색 결정
                            Brush backgroundBrush;
                            Brush textBrush;

                            if (barcodeValue == "NG")
                            {
                                // 매칭 데이터 없는 경우 - 회색 배경
                                backgroundBrush = Brushes.Gray;
                                textBrush = Brushes.White;
                            }
                            else if (tcpDataUpper.Contains(barcodeValue))
                            {
                                // 매칭 성공 - 초록색 배경
                                backgroundBrush = Brushes.Green;
                                textBrush = Brushes.White;
                            }
                            else
                            {
                                // 매칭 실패 - 빨간색 배경
                                backgroundBrush = Brushes.Red;
                                textBrush = Brushes.White;
                            }

                            // 텍스트 배경 그리기
                            RectangleF backgroundRect = new RectangleF(
                                textX - 2, textY - 2,
                                textSize.Width + 4, textSize.Height + 4);
                            g.FillRectangle(backgroundBrush, backgroundRect);

                            // 텍스트 그리기
                            g.DrawString(barcodeValue, largeFont, textBrush, textX, textY);
                        }
                    }
                }
            }

            // NG 또는 매칭 실패가 있는 경우 이미지 저장
            if (hasNgOrFailure)
            {
                SaveFailureImage(fittedImage);
            }
        }
        // dgv_Data 저장을 위한 헬퍼 메서드 추가 (클래스 내 적절한 위치에)
        private void SaveDataGridViewToFile()
        {
            try
            {
                // Data 폴더 경로 생성
                string dataFolderPath = Path.Combine(CNIParams.SaveImagePath, "Data");

                // 폴더가 없으면 생성
                if (!Directory.Exists(dataFolderPath))
                {
                    Directory.CreateDirectory(dataFolderPath);
                }

                // 오늘 날짜로 기본 파일명 생성
                string baseFileName = $"DataLog_{DateTime.Now:yyyy-MM-dd}";
                string fileName = $"{baseFileName}.csv";
                string filePath = Path.Combine(dataFolderPath, fileName);
                
                // 파일이 존재하고 10000줄을 초과하는지 확인
                int fileNumber = 1;
                while (File.Exists(filePath))
                {
                    int lineCount = CountLinesInFile(filePath);
                    if (lineCount >= 10000)
                    {
                        // 새로운 파일명 생성 (순번 추가)
                        fileName = $"{baseFileName}_{fileNumber:D3}.csv";
                        filePath = Path.Combine(dataFolderPath, fileName);
                        fileNumber++;
                    }
                    else
                    {
                        break; // 현재 파일을 사용
                    }
                }

                // CSV 형태로 데이터 저장
                using (StreamWriter writer = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    // 파일이 비어있으면 헤더 추가
                    if (new FileInfo(filePath).Length == 0)
                    {
                        writer.WriteLine("시간,TCP데이터,바코드데이터,상태");
                    }

                    // 마지막에 추가된 데이터들만 저장 (N_Read 개수만큼)
                    int startIndex = Math.Max(0, dgv_Data.Rows.Count - CNIParams.ReadCount);
                    for (int i = startIndex; i < dgv_Data.Rows.Count; i++)
                    {
                        if (dgv_Data.Rows[i].Cells.Count >= 4)
                        {
                            string timestamp = dgv_Data.Rows[i].Cells[0].Value?.ToString() ?? "";
                            string tcpData = dgv_Data.Rows[i].Cells[1].Value?.ToString() ?? "";
                            string barcodeData = dgv_Data.Rows[i].Cells[2].Value?.ToString() ?? "";
                            string status = dgv_Data.Rows[i].Cells[3].Value?.ToString() ?? "";

                            // CSV 형식으로 저장 (쉼표가 포함된 데이터는 따옴표로 감싸기)
                            writer.WriteLine($"\"{timestamp}\",\"{tcpData}\",\"{barcodeData}\",\"{status}\"");
                        }
                    }
                }

                CNILog.Write($"DataGridView 데이터가 {filePath}에 저장되었습니다.", true);
            }
            catch (Exception ex)
            {
                CNILog.Write($"DataGridView 데이터 저장 오류: {ex.Message}", true);
            }
        }
        
        // 파일의 줄 수를 세는 헬퍼 메서드 추가
        private int CountLinesInFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return 0;
                    
                int lineCount = 0;
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    while (reader.ReadLine() != null)
                    {
                        lineCount++;
                    }
                }
                return lineCount;
            }
            catch (Exception ex)
            {
                CNILog.Write($"파일 줄 수 계산 오류: {ex.Message}", true);
                return 0;
            }
        }
        private void SaveFailureImage(Bitmap image)
        {
            try
            {
                // SaveImages/오늘날짜/ 폴더 경로 생성
                string todayFolder = DateTime.Now.ToString("yyyy-MM-dd");
                string saveDirectory = !string.IsNullOrEmpty(CNIParams.SaveImagePath)
                    ? Path.Combine(CNIParams.SaveImagePath, "SaveImages", todayFolder)
                    : Path.Combine(Application.StartupPath, "SaveImages", todayFolder);

                // 디렉토리가 없으면 생성
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                // 파일명 생성 (타임스탬프 포함)
                string fileName = $"Failure_{DateTime.Now:yyyyMMdd_HHmmss_fff}.jpg";
                string fullPath = Path.Combine(saveDirectory, fileName);

                // 이미지 저장
                image.Save(fullPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                CNILog.Write($"매칭 실패/NG 이미지 저장 완료: {fullPath}", true);
            }
            catch (Exception ex)
            {
                CNILog.Write($"이미지 저장 오류: {ex.Message}", true);
            }
        }
        private void DisplayImage(Bitmap image)
        {
            if (MainDisplay.InvokeRequired)
            {
                MainDisplay.Invoke(new Action(() =>
                {
                    if (MainDisplay.Image != null)
                    {
                        MainDisplay.Image.Dispose();
                    }
                    MainDisplay.Image = image;
                    MainDisplay.SizeMode = PictureBoxSizeMode.Zoom;
                    MainDisplay.Refresh();
                }));
            }
            else
            {
                if (MainDisplay.Image != null)
                {
                    MainDisplay.Image.Dispose();
                }
                MainDisplay.Image = image;
                MainDisplay.Refresh();
            }
        }
        private string GetReadStringFromResultXml(string resultXml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(resultXml);

                XmlNode full_string_node = doc.SelectSingleNode("result/general/full_string");

                if (full_string_node != null && _system != null && _system.State == Cognex.DataMan.SDK.ConnectionState.Connected)
                {
                    XmlAttribute encoding = full_string_node.Attributes["encoding"];
                    if (encoding != null && encoding.InnerText == "base64")
                    {
                        if (!string.IsNullOrEmpty(full_string_node.InnerText))
                        {
                            byte[] code = Convert.FromBase64String(full_string_node.InnerText);
                            return _system.Encoding.GetString(code, 0, code.Length);
                        }
                        else
                        {
                            return "";
                        }
                    }

                    return full_string_node.InnerText;
                }
            }
            catch (Exception ex)
            {
                CNILog.Write($"XML 파싱 오류: {ex.Message}", true);
            }
            return "";
        }
        #endregion

        #region Log Events
        private void CNILog_Message(string str, bool bShow)
        {
            try
            {
                if (rtxLog != null && bShow)
                {
                    _syncContext.Post(delegate
                    {
                        if (rtxLog.Lines.Length > 1000)
                        {
                            string[] lines = rtxLog.Lines;
                            string[] newLines = new string[500];
                            Array.Copy(lines, lines.Length - 500, newLines, 0, 500);
                            rtxLog.Lines = newLines;
                        }

                        rtxLog.AppendText("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + str);
                        rtxLog.ScrollToCaret();
                    }, null);
                }
            }
            catch
            {
                CNILog.Write("Write TextBox Log Error", false);
            }
        }
        #endregion

        #region Button Events
        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void bt_Live_Click(object sender, EventArgs e)
        {
            // Live 모드 구현
        }
        private void bt_Trigger_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _system.SendCommand("TRIGGER ON");
            }
            catch (Exception ex)
            {
                CNILog.Write("Failed to send TRIGGER ON command: " + ex.ToString(), true);
            }
        }
        private void bt_Trigger_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _system.SendCommand("TRIGGER OFF");
            }
            catch (Exception ex)
            {
                CNILog.Write("Failed to send TRIGGER OFF command: " + ex.ToString(), true);
            }
        }
        private void bt_SelectPath_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "이미지 저장 경로를 선택하세요";
                fbd.ShowNewFolderButton = true;

                // 현재 설정된 경로가 있으면 초기 경로로 설정
                if (!string.IsNullOrEmpty(CNIParams.SaveImagePath) && Directory.Exists(CNIParams.SaveImagePath))
                {
                    fbd.SelectedPath = CNIParams.SaveImagePath;
                }

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    // 선택된 경로를 CNIParams.SaveImagePath에 할당
                    CNIParams.SaveImagePath = fbd.SelectedPath;

                    // config 파일에 저장
                    if (CNIParams.Save())
                    {
                        tb_Path.Text = CNIParams.SaveImagePath;
                        CNILog.Write($"이미지 저장 경로 설정 완료: {CNIParams.SaveImagePath}", true);
                        MessageBox.Show($"이미지 저장 경로가 설정되었습니다.\n{CNIParams.SaveImagePath}", "경로 설정 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        CNILog.Write("이미지 저장 경로 config 파일 저장 실패", true);
                        MessageBox.Show("설정 저장에 실패했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                CNILog.Write("Failed to Select Path " + ex.ToString(), true);
                MessageBox.Show($"경로 선택 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ng_NRead_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                NumericUpDown numericUpDown = sender as NumericUpDown;
                if (numericUpDown == null) return;

                // CNIParams.N_Read 값 업데이트
                CNIParams.ReadCount = (int)numericUpDown.Value;

                // Config.ini 파일에 저장
                CNIParams.Save();

                CNILog.Write($"N_Read 값이 {CNIParams.ReadCount}로 변경되었습니다.", false);

                // Dataman 카메라에 리딩 갯수 명령 전송
                if (_system != null && _system.State == Cognex.DataMan.SDK.ConnectionState.Connected)
                {
                    try
                    {
                        // 멀티코드 리딩 활성화


                        // 최대 코드 수 설정
                        _system.SendCommand($"SET MULTICODE.NUM-CODES {CNIParams.ReadCount}");
                        _system.SendCommand($"SET MULTICODE.MAX-NUM-CODES 1 {CNIParams.ReadCount}");
                        _system.SendCommand("SET MULTICODE.PARTIAL-RESULTS ON");

                        CNILog.Write($"Dataman 카메라에 리딩 갯수 {CNIParams.ReadCount} 설정 완료", false);
                    }
                    catch (Exception cameraEx)
                    {
                        CNILog.Write($"Dataman 카메라 명령 전송 실패: {cameraEx.Message}", true);
                    }
                }
                else
                {
                    CNILog.Write("Dataman 카메라가 연결되지 않아 명령을 전송할 수 없습니다.", true);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"ng_NRead_ValueChanged 오류: {ex.Message}";
                CNILog.Write(errorMessage, true);
            }
        }
        #endregion
        // 자동 트리거를 위한 타이머 추가

        #region  Test...
        private System.Timers.Timer autoTriggerTimer;
        private bool isAutoTriggerRunning = false;
        private void InitializeAutoTriggerTimer()
        {
            autoTriggerTimer = new System.Timers.Timer(1000); // 1초 간격
            autoTriggerTimer.Elapsed += AutoTriggerTimer_Tick;
            autoTriggerTimer.AutoReset = true;
            autoTriggerTimer.Enabled = false;
        }
        private void AutoTriggerTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_syncContext != null)
                {
                    _syncContext.Post(_ =>
                    {
                        try
                        {
                            // 트리거 ON 명령 전송
                            _system?.SendCommand("TRIGGER ON");

                            // 짧은 지연 후 트리거 OFF
                            Task.Delay(100).ContinueWith(t =>
                            {
                                _syncContext.Post(__ =>
                                {
                                    try
                                    {
                                        _system?.SendCommand("TRIGGER OFF");
                                    }
                                    catch (Exception ex)
                                    {
                                        CNILog.Write("Failed to send auto TRIGGER OFF command: " + ex.ToString(), true);
                                    }
                                }, null);
                            });
                        }
                        catch (Exception ex)
                        {
                            CNILog.Write("Failed to send auto TRIGGER ON command: " + ex.ToString(), true);
                        }
                    }, null);
                }
            }
            catch (Exception ex)
            {
                CNILog.Write("Auto trigger timer error: " + ex.ToString(), true);
            }
        }
        private void lb_bCamCon_Click(object sender, EventArgs e)
        {
            try
            {
                if (isAutoTriggerRunning)
                {
                    // 자동 트리거 정지
                    autoTriggerTimer.Stop();
                    isAutoTriggerRunning = false;
                    lb_bCamCon.Text = "Auto Trigger: OFF";
                    lb_bCamCon.BackColor = Color.LightGray;
                    CNILog.Write("Auto trigger stopped", false);
                }
                else
                {
                    // 자동 트리거 시작
                    autoTriggerTimer.Start();
                    isAutoTriggerRunning = true;
                    lb_bCamCon.Text = "Auto Trigger: ON";
                    lb_bCamCon.BackColor = Color.LightGreen;
                    CNILog.Write("Auto trigger started (1 second interval)", false);
                }
            }
            catch (Exception ex)
            {
                CNILog.Write("Failed to toggle auto trigger: " + ex.ToString(), true);
            }
        }
        #endregion
    }
}



