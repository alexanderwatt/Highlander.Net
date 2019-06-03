    @echo off

    rem Ensure VS env vars are set for VS2010
    call :setenv "C:\Program Files\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    call :setenv "C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools\vsvars32.bat"
    if /i "%DevEnvDir%" == "" goto :err_devenv

    echo %~n0: Generating...
    echo %~n0: Generating... >xsdgen.log

    rem rename input xsd files to stop xsd command line overflow
    set _xsd_flist=
    set _xsd_oname=
    call :xsdlist pre_xsd
    
    xsd /c /n:FpML.V5r10.Reporting %_xsd_flist% >>xsdgen.log
    
    set _errnum=%errorlevel%
    if /i %_errnum% GEQ 1 goto :xsd_err
    
    if exist XsdClasses.cs del /q XsdClasses.cs
    rename %_xsd_oname%.cs XsdClasses.cs
    call :xsdlist post_xsd

    echo %~n0: Done. >>xsdgen.log
    echo %~n0: Done.

    goto :eof
    
:pre_xsd
    echo %~n0:   (%1) %2.xsd >>xsdgen.log
    echo %~n0:   (%1) %2.xsd
    copy /v %2.xsd %1.xsd  >nul
    set _xsd_flist=%_xsd_flist% %1.xsd
    if /i "%_xsd_oname%" NEQ "" set _xsd_oname=%_xsd_oname%_%1
    if /i "%_xsd_oname%" EQU "" set _xsd_oname=%1
    goto :eof
    
:post_xsd
    del /f /q %1.xsd
    goto :eof
    
:xsdlist
    call :%1 a fpml-main-5-10
    call :%1 b qrscext-instruments
    call :%1 c xmldsig-core-schema
    call :%1 d fpmlext-shared
    call :%1 e fpmlext-collective-investment-vehicles
    call :%1 f fpmlext-equities
    call :%1 g fpmlext-fixed-income
    call :%1 h fpmlext-futures-and-options
    call :%1 i fpmlext-interest-rate-derivatives
    call :%1 j fpmlext-repo
    call :%1 k fpmlext-trade
    call :%1 l fpmlext-warrant
    goto :eof

:xsd_err
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
