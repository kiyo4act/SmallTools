#pragma once

#include "resource.h"

BOOL CheckCommandLine(int argc, TCHAR* argv[]);
BOOL CheckFilePathOfSrcAndDest();

BOOL ExecutePdbcopy();
BOOL CreateDestPdbFilePath();
BOOL CreateDestPdbFilePathFromSrcDir(CString strSrc, CString strDest);
