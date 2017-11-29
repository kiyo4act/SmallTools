using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UtilitiesLib;

namespace AutoHostsUpdateService
{
    public class EditHosts
    {
        private EventLog eventLog;
        private string HostsFilePath { get; set; }
        private bool IsCreateNewFile { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="hostsFilePath"></param>
        public EditHosts(string hostsFilePath, EventLog ev)
        {
            eventLog = ev;
            eventLog.Source = LocalResources.m_gstrEventLogSource;
            HostsFilePath = hostsFilePath;


        }

        /// <summary>
        /// 文字列をマージして書き出す
        /// </summary>
        /// <param name="importStrings"></param>
        /// <param name="separate"></param>
        /// <param name="searchNum"></param>
        /// <returns></returns>
        public bool MargeString(string importStrings, char separate, int searchNum)
        {
            bool fResult = true;

            string buffer = string.Empty;
            string[] importLines = importStrings.Split('\n');

            // ファイルから読み込む
            string readAllStrings = this.readAllStrings();

            // 新規作成フラグを確認
            if (readAllStrings == null)
            {
                fResult = false;
            }
            else if (readAllStrings == string.Empty)
            {
                //eventLog.WriteEntry("m_readAllStrings is empty", EventLogEntryType.Information);
                // importSrtingsをそのままバッファーへコピー
                buffer = importStrings;
            }
            else
            {
                // m_readAllStringsを行で分割(空白行を削除)
                IEnumerable<string> readLinesCollection = readAllStrings.Split(new char[] { '\n' }).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));

                // readLinesの各行について
                foreach (string readLine in readLinesCollection)
                {
                    // 重複フラグ
                    bool fIsRedundancy = false;
                    //eventLog.WriteEntry("readLine ==> " + readLine, EventLogEntryType.Information);
                    // importLinesから1行ずつ読み出す
                    foreach (string importLine in importLines)
                    {
                        // separateでimportLineを分割、指定した番号の文字列をsearchWordとして格納
                        string searchWord = importLine.Split(new char[] { separate }).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ElementAt(searchNum);
                        //eventLog.WriteEntry("searchWord ==> " + searchWord, EventLogEntryType.Information);
                        // 読み出した中にsearchWordの文字があった時
                        if (readLine.IndexOf(searchWord) != -1)
                        {
                            // 読み出した行はバッファーに入れない
                            //eventLog.WriteEntry("searchWord is detected in readline", EventLogEntryType.Information);
                            fIsRedundancy = true;
                            break;
                        }                    
                    }

                    // 読み出した行がimportLinesに一つも無かった場合 
                    if (!fIsRedundancy)
                    {
                        //読み出した行をバッファーに入れる
                        if (buffer == string.Empty)
                        {
                            buffer = readLine;
                            //eventLog.WriteEntry("buffer = readLine ==> " + buffer, EventLogEntryType.Information);
                        }
                        else
                        {
                            // 末尾に\rが残っていた場合
                            if (buffer.EndsWith("\r"))
                            {
                                buffer += "\n" + readLine;
                            }
                            else
                            {
                                buffer += System.Environment.NewLine + readLine;
                            }
                        }
                    }
                }
                // 最終的にバッファーが空でない時は改行を追加
                if (buffer != string.Empty)
                {
                    // 末尾に\rが残っていた場合
                    if (buffer.EndsWith("\r"))
                    {
                        buffer += "\n";
                    }
                    else
                    {
                        buffer += System.Environment.NewLine;
                    }
                }
                // 引数の文字列を加える
                buffer += importStrings;
            }
            // 読み込みが成功していれば、バッファをファイルに書き出す
            if (fResult)
            {
                fResult = overwriteStrings(buffer);
            }
            return fResult;
        }

        public string readAllStrings()
        {
            IsCreateNewFile = false;
            string readAllStrings = string.Empty;
            try
            {
                StreamReader streamReader = new StreamReader(HostsFilePath);
                readAllStrings = streamReader.ReadToEnd();
                streamReader.Close();
                IsCreateNewFile = false;
            }
            catch (FileNotFoundException)
            {
                // ファイルが存在しない場合は新規作成フラグを立てる
                IsCreateNewFile = true;
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
                readAllStrings = null;
            }
            return readAllStrings;
        }


        /// <summary>
        /// 指定した文字列でファイルを上書きする
        /// </summary>
        /// <param name="importStrings"></param>
        /// <returns></returns>
        public bool overwriteStrings(string importStrings)
        {
            bool fResult = true;

            try
            {
                StreamWriter streamWriter = new StreamWriter(HostsFilePath, false);
                streamWriter.Write(importStrings);
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                fResult = false;
                eventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
            IsCreateNewFile = false;
            return fResult;
        }
    }
}
