    @echo off

    rem Ensure VS env vars are set for VS2010
    call :setenv "C:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    call :setenv "D:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    call :setenv "C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    call :setenv "D:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    if /i "%DevEnvDir%" == "" goto :err_devenv

    echo %~n0: Generating...

    set xsd_options=/c /n:FpML.V5r3.Codelist Codelist.xsd
    call :call_xsd
    set xsd_options=/c /n:FpML.V5r3.Confirmation Confirmation.xsd xmldsig-core-schema.xsd
    call :call_xsd
    set xsd_options=/c /n:FpML.V5r3.Reporting Reporting.xsd xmldsig-core-schema.xsd
    call :call_xsd
    set xsd_options=/c /n:FpML.V5r3.Transparency Transparency.xsd xmldsig-core-schema.xsd
    call :call_xsd
    set xsd_options=/c /n:FpML.V5r3.RecordKeeping RecordKeeping.xsd xmldsig-core-schema.xsd
    call :call_xsd

    echo %~n0: Done.

    goto :eof

:call_xsd
    xsd %xsd_options%
    set _errnum=%errorlevel%
    if /i %_errnum% EQU 0 goto :eof

    echo %~n0: XSD returned: %_errnum%
    xsd /?
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
