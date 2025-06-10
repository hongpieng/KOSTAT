using System;
using System.Collections.Generic;
using System.Drawing;
using Cognex.DataMan.SDK;

namespace KOSTAT_IDReader
{
    /// <summary>
    /// 시스템 매개변수 관리 클래스
    /// </summary>
    public static class CNIParams
    {
        #region Camera Configuration
        /// <summary>데이터맨 카메라 IP 주소</summary>
        public static string CamIP { get; set; } = "192.168.1.5";
        
        /// <summary>데이터맨 카메라 포트</summary>
        public static int CamPort { get; set; } = 23;
        
        /// <summary>카메라 명칭</summary>
        public static string CameraName { get; set; } = string.Empty;
        
        /// <summary>이미지 저장 경로</summary>
        public static string SaveImagePath { get; set; } = string.Empty;
        #endregion

        #region Laser Configuration
        /// <summary>레이저 마킹기 IP 주소</summary>
        public static string LaserIP { get; set; } = "192.168.1.70";
        
        /// <summary>레이저 마킹기 포트</summary>
        public static int LaserPort { get; set; } = 2000;
        #endregion

        #region Data Configuration
        /// <summary>매칭 데이터 리스트</summary>
        public static List<string[]> MatchingDatas { get; set; } = new List<string[]>();
        
        /// <summary>리딩 개수</summary>
        public static int ReadCount { get; set; } = 5;
        
        /// <summary>매칭/비매칭 표시 색상</summary>
        public static Color BoxColor { get; set; } = Color.Lime;
        
        /// <summary>리더기 데이터 배열</summary>
        public static string[] ReaderData { get; set; }
        
        /// <summary>레이저 마킹기 데이터 배열</summary>
        public static string[] LaserData { get; set; }
        #endregion

        /// <summary>
        /// INI 파일에서 설정값을 로드합니다.
        /// </summary>
        /// <returns>로드 성공 여부</returns>
        public static bool Load()
        {
            try
            {
                // Camera settings
                CamIP = CNIiniControl.IniReadValue("CAMERA", "IP");
                CamPort = int.Parse(CNIiniControl.IniReadValue("CAMERA", "PORT"));
                SaveImagePath = CNIiniControl.IniReadValue("CAMERA", "PATH");

                // Laser settings
                LaserIP = CNIiniControl.IniReadValue("LASER", "IP");
                LaserPort = int.Parse(CNIiniControl.IniReadValue("LASER", "PORT"));
                ReadCount = int.Parse(CNIiniControl.IniReadValue("LASER", "NREAD"));

                return true;
            }
            catch (Exception ex)
            {
                CNILog.Write($"Configuration load error: {ex.Message}", false);
                return false;
            }
        }

        /// <summary>
        /// 설정값을 INI 파일에 저장합니다.
        /// </summary>
        /// <returns>저장 성공 여부</returns>
        public static bool Save()
        {
            try
            {
                // Camera settings
                CNIiniControl.IniWriteValue("CAMERA", "IP", CamIP);
                CNIiniControl.IniWriteValue("CAMERA", "PORT", CamPort.ToString());
                CNIiniControl.IniWriteValue("CAMERA", "PATH", SaveImagePath);

                // Laser settings
                CNIiniControl.IniWriteValue("LASER", "IP", LaserIP);
                CNIiniControl.IniWriteValue("LASER", "PORT", LaserPort.ToString());
                CNIiniControl.IniWriteValue("LASER", "NREAD", ReadCount.ToString());

                return true;
            }
            catch (Exception ex)
            {
                CNILog.Write($"Configuration save error: {ex.Message}", false);
                return false;
            }
        }
    }
}
