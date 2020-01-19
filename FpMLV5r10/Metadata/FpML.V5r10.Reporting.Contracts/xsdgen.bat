    @echo off

    rem Ensure VS env vars are set for VS2010
    call :setenv "C:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    call :setenv "C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    if /i "%DevEnvDir%" == "" goto :err_devenv
        
    echo %~n0: Generating...
    echo %~n0: Generating... >xsdgen.log

    rem --------------- main ---------------
    set _ns=FpML.V5r10.Reporting.Contracts
    call :gen XmlContracts
    rem ------------------------------------
    
    echo %~n0: Done.
    echo %~n0: Done. >>xsdgen.log

    goto :eof

:gen
    echo %~n0:   %1.xsd ...
    echo %~n0:   %1.xsd ... >>xsdgen.log
    xsd /c /n:%_ns% %1.xsd  >>xsdgen.log
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
