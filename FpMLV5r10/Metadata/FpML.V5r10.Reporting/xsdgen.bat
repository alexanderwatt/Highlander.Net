    @echo off

    rem Open a command tool in VS2019 and navigate to the appropriate directory
	rem Download the merged schema from the FpML site.
	rem Rename the original file reporting_mergedschema.xsd to Reporting.xsd
	rem Type - xsd /p:xsdreporting.xml
	rem rename the output files to Reporting.
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
    call :%1 a Reporting
	rem call :%1 b fpmlext-main	
    call :%1 b xmldsig-core-schema
    call :%1 c fpmlext-shared
    call :%1 d fpmlext-collective-investment-vehicles
    call :%1 e fpmlext-equities
    call :%1 f fpmlext-fixed-income
    call :%1 g fpmlext-futures-and-options
    call :%1 h fpmlext-interest-rate-derivatives
	call :%1 i fpmlext-property
    call :%1 j fpmlext-repo
    call :%1 k fpmlext-trade
    call :%1 l fpmlext-warrant
	rem call :%1 m qrscext-instruments
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
