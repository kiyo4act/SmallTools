echo %PATH%
IsCmdBld -p AutoHostsUpdate.ism -a Configuration1 -r "Release"
Copy Configuration1\Release\DiskImages\DISK1\setup.exe setup.exe
@pause