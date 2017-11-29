// pdbcopyex.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <ImageHlp.h>
#include <atltime.h>
#include "pdbcopyex.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#pragma comment(lib, "imagehlp.lib")


// The one and only application object

CWinApp theApp;

using namespace std;

BOOL g_bSrcIsDirectory = FALSE;

CString g_strInputedSrc = _T("");
CString g_strInputedDest = _T("");
CString g_strInputedOpt = _T("");

CStringArray g_strSrcPdbPaths;
CStringArray g_strDestPdbPaths;


int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	int nRetCode = 0;

	HMODULE hModule = ::GetModuleHandle(NULL);

	if (hModule != NULL)
	{
		// initialize MFC and print and error on failure
		if (!AfxWinInit(hModule, NULL, ::GetCommandLine(), 0))
		{
			// TODO: change error code to suit your needs
			_tprintf(_T("Fatal Error: MFC initialization failed\n"));
			nRetCode = 1;
		}
		else
		{
			// TODO: code your application's behavior here.
			if (!CheckCommandLine(argc, argv))
			{
				_tprintf(_T("Command Line is invalid."));
				nRetCode = 1;
			}
			else
			{
				nRetCode = (ExecutePdbcopy() == TRUE) ? 0 : 1;
			}
		}
	}
	else
	{
		// TODO: change error code to suit your needs
		_tprintf(_T("Fatal Error: GetModuleHandle failed\n"));
		nRetCode = 1;
	}

	return nRetCode;
}

BOOL CheckCommandLine(int argc, TCHAR* argv[])
{
	if (argc > 1) g_strInputedSrc = argv[1];
	if (argc > 2) g_strInputedDest = argv[2];

	if (g_strInputedSrc == _T("")) return FALSE;
	if (g_strInputedDest == _T("")) return FALSE;

	for (int i = 3; i < argc; i++)
	{
		g_strInputedOpt += argv[i];
		g_strInputedOpt += _T(" ");
	}

	return CheckFilePathOfSrcAndDest();
}

BOOL CheckFilePathOfSrcAndDest()
{
	BOOL bSrcIsDirectory = ::PathIsDirectory(g_strInputedSrc);
	BOOL bDestIsDirectory = ::PathIsDirectory(g_strInputedDest);

	g_bSrcIsDirectory = bSrcIsDirectory;

	if (bSrcIsDirectory && bDestIsDirectory) return TRUE;
	if (!bSrcIsDirectory && !bDestIsDirectory) return TRUE;

	_tprintf(_T("SrcPath and DestPath are invalid path."));
	return FALSE;
}

BOOL ExecutePdbcopy()
{
	if (!CreateDestPdbFilePath()) return FALSE;

	STARTUPINFO si = { 0 };
	PROCESS_INFORMATION pi = { 0 };
	GetStartupInfo(&si);

	CString strCmd;

	_tprintf(_T("=== Start(%s) ===\r\n"), CTime::GetCurrentTime().Format(_T("%Y/%m/%d %H:%M:%S")));

	for (INT_PTR i = 0; i < g_strSrcPdbPaths.GetCount(); i++)
	{
		_tprintf(_T("=== %d/%d (%s) ===\r\n"), i+1, g_strSrcPdbPaths.GetCount(), g_strSrcPdbPaths.GetAt(i));

		strCmd.Format(_T("pdbcopy.exe \"%s\" \"%s\" %s"),
							g_strSrcPdbPaths.GetAt(i),
							g_strDestPdbPaths.GetAt(i),
							g_strInputedOpt);

		CStringA strPath(g_strDestPdbPaths.GetAt(i));
		::MakeSureDirectoryPathExists(strPath);

		if (!CreateProcess(NULL, strCmd.GetBuffer(),
							NULL, NULL, FALSE, 0,
							NULL, NULL, &si, &pi)) return FALSE;

		WaitForSingleObject(pi.hProcess, INFINITE);

		::CloseHandle(pi.hThread);
		::CloseHandle(pi.hProcess);
	}
	
	_tprintf(_T("=== End(%s) ===\r\n"), CTime::GetCurrentTime().Format(_T("%Y/%m/%d %H:%M:%S")));
	return TRUE;
}

BOOL CreateDestPdbFilePath()
{
	if (!g_bSrcIsDirectory)
	{
		g_strSrcPdbPaths.Add(g_strInputedSrc);
		g_strDestPdbPaths.Add(g_strInputedDest);
		return TRUE;
	}

	return CreateDestPdbFilePathFromSrcDir(g_strInputedSrc, g_strInputedDest);
}

BOOL CreateDestPdbFilePathFromSrcDir(CString strSrc, CString strDest)
{
	CFileFind Finder;
	CString strFindPath(strSrc);
	strFindPath += _T("\\*.*");

	if (Finder.FindFile(strFindPath, 0))
	{
		int iResult = 1;
		while (iResult)
		{
			iResult = Finder.FindNextFileW();

			if (Finder.IsDots()) continue;

			CString strFindSrc = strSrc + '\\' + Finder.GetFileName();
			CString strFindDest = strDest + '\\' + Finder.GetFileName();

			if (Finder.IsDirectory())
			{
				if (!CreateDestPdbFilePathFromSrcDir(strFindSrc, strFindDest)) return FALSE;
			}
			else
			{
				CString strExt = Finder.GetFileName();
				strExt.Replace(Finder.GetFileTitle(), _T(""));

				if (strExt == _T(".pdb"))
				{
					g_strSrcPdbPaths.Add(strFindSrc);
					g_strDestPdbPaths.Add(strFindDest);
				}
			}
		}
		
		Finder.Close();
		return TRUE;
	}

	_tprintf(_T("FindFile failed. 0x%08x."), GetLastError());
	return FALSE;
}
