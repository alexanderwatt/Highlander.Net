    @echo off

    rem Ensure VS env vars are set for VS2010
    call :setenv "C:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    call :setenv "C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    if /i "%DevEnvDir%" == "" goto :err_devenv

    call FpMLCodeListTool.exe /regen
    
    echo %~n0: Generating...
    echo %~n0: Generating... >xsdgen.log

    set _ns=FpML.V5r3.Codes

    call :gen FpMLCodes

    echo %~n0: Done.
    echo %~n0: Done. >>xsdgen.log

    goto :eof

:gen
    echo %~n0:   %~n1.xsd ...
    echo %~n0:   %~n1.xsd ... >>xsdgen.log
    xsd /c /n:%_ns% %~n1.xsd  >>xsdgen.log
    set _errnum=%errorlevel%
    if /i %_errnum% EQU 0 goto :eof

    type xsdgen.log
    echo %~n0: ERROR: XSD returned: %_errnum%
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
