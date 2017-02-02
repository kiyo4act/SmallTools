using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    /// iniファイル取り扱いのためのユーティリティクラス
    /// </summary>
    public class InifileUtils
    {
        /// <summary>
        /// iniファイルのパスを保持
        /// </summary>
        private String filePath { get; set; }

        // ==========================================================
        
        // ==========================================================

        /// <summary>
        /// コンストラクタ(デフォルト)
        /// </summary>
        public InifileUtils()
        {
            this.filePath = AppDomain.CurrentDomain.BaseDirectory + "hogehoge.ini";
        }

        /// <summary>
        /// コンストラクタ(fileパスを指定する場合)
        /// </summary>
        /// <param name="filePath">iniファイルパス</param>
        public InifileUtils(String filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>
        /// iniファイル中のセクションのキーを指定して、文字列を返す
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public String getValueString(String section, String key)
        {
            StringBuilder sb = new StringBuilder(1024);

            NativeMethods.GetPrivateProfileString(
                section,
                key,
                "",
                sb,
                Convert.ToUInt32(sb.Capacity),
                filePath);

            return sb.ToString();
        }

        /// <summary>
        /// iniファイル中のセクションのキーを指定して、整数値を返す
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public int getValueInt(String section, String key)
        {
            return (int)NativeMethods.GetPrivateProfileInt(section, key, 0, filePath);
        }

        /// <summary>
        /// 指定したセクション、キーに数値を書き込む
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void setValue(String section, String key, int val)
        {
            setValue(section, key, val.ToString());
        }

        /// <summary>
        /// 指定したセクション、キーに文字列を書き込む
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void setValue(String section, String key, String val)
        {
            NativeMethods.WritePrivateProfileString(section, key, val, filePath);
        }
    }
}
