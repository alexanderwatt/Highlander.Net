    @echo off

    rem Ensure VS env vars are set for VS2010
    call :setenv "C:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    call :setenv "C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    if /i "%DevEnvDir%" == "" goto :err_devenv
    
    rem set parameters
    set _cfg=Debug
    call :get_path ..\..\..
    set _tree=%_cdf%
    
    rem build all solutions found in this dir
    for %%f in (*.sln) do call :buildsln %%f
    goto :eof

:buildsln
    title Build: Rebuilding %~1 (%_cfg%) ...
    MSBuild "%~1" /t:Rebuild /p:Configuration=%_cfg%;OutDir=%_tree%\Output\%_cfg%\
    set _errnum=%errorlevel%
    if %_errnum% EQU 0 goto :eof
    rem error occurred
    pause
    goto :eof

:get_path
    set _cdf=%~f1
    set _cddp=%~dp1
    set _cdp=%~p1
    set _cdd=%~d1
    set _cdn=%~n1
    goto :eof

:setenv
    rem if /i "%DevEnvDir%" NEQ "" goto :eof
    if not exist "%~1" goto :eof
    call "%~1"
    goto :eof

:err_devenv
    echo Cannot find vsvars32.bat!
    pause
    goto :eof
