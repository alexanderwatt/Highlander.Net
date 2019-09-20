    @echo off
    echo %~n0: Starting...

    rem Ensure VS env vars are set for VS2010
    call :setenv "C:\Program Files\Microsoft Visual Studio 10.0\Common7\Tools\vsvars32.bat"
    call :setenv "C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\Tools\vsvars32.bat"
    if /i "%DevEnvDir%" == "" goto :err_devenv

    echo %~n0: DevEnvDir is %DevEnvDir%
    call :find_in_path xsd.exe
    if not exist %_file% goto err_fnf
    set _xsdpath=%_file%
    echo %~n0: Found XSD at %_xsdpath%
    echo %~n0: Generating source...

    set _ns=Orion.Util.Expressions
    call  :gen_src  ExprFormat     %_ns%

    echo %~n0: Done.

pause
    goto :eof

:gen_src
    set _cmd="%_xsdpath%" /c /n:%2 %1.xsd
    %_cmd%
    set _errnum=%errorlevel%
    if /i %_errnum% GTR 0 goto err_cmd
    goto :eof

:find_in_path
    set _file=%~$PATH:1
    goto :eof

:err_fnf
    echo %~n0: ERROR: File "%_file%" not found!
    pause
    goto :eof

:err_cmd
    echo %~n0: ERROR: Command failed.
    echo %~n0:   Error  : %_errnum%
    echo %~n0:   Command: "%_cmd%"
pause
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
