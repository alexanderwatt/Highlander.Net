    @echo off

    rem Open a command tool in VS2019 and navigate to the appropriate directory
	rem Download the merged schema from the FpML site.
	rem Rename the original file reporting_mergedschema.xsd to Reporting.xsd
	rem Type - "xsd /parameters"xsdreporting.xml
	rem rename the output files to Reporting.

    echo %~n0: Generating...

	set xsd_options=/parameters:xsdreporting.xml
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
    echo Cannot find the xsd tool!
    pause
    goto :eof
