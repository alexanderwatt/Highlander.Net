    @echo off

    rem process arguments
    rem - default settings
    set _unattended=0
    set _verbosity=1
    set _keeplogs=0

:args_next
    if /i "%1" == "" goto args_done
    if /i "%1" == "/auto"     set _unattended=1
    if /i "%1" == "/quiet"    set _verbosity=0
    if /i "%1" == "/noisy"    set _verbosity=2
    if /i "%1" == "/keeplogs" set _keeplogs=1
    shift
    goto args_next

:args_done

    rem clean temp directories
    if %_verbosity% GEQ 1 title Clean: Removing temp dirs ...
    for /r %%d in (.) do call :checkdir "%%d"
    if %_verbosity% GEQ 1 title Clean: Removing build output ...

    if exist "%_outpath%" call :cleandir "%_outpath%"

    rem clean temp files
    if %_verbosity% GEQ 1 title Clean: Deleting temp files ...
    if exist _rc.txt  del /q /f _rc.txt 1>nul 2>nul
    set _files=*.tmp *.bld *.ncb *.pch *.scc *.user *.cache *.resharper TestResult.xml UpgradeLog*.xml
    if %_keeplogs% LEQ 0 set _files=%_files% *.log
    dir /a-d/s/b %_files% >_rc.txt 2>nul
    for /f %%f in (_rc.txt) do call :delfile "%%f"
    if exist _rc.txt  del /q _rc.txt 1>nul 2>nul

    rem done
    goto :eof

:checkdir
    if /i "%~nx1" == "bin"					goto cleandir
    if /i "%~nx1" == "obj"					goto cleandir
    if /i "%~nx1" == "Debug"				goto cleandir
    if /i "%~nx1" == "Release"				goto cleandir
    if /i "%~nx1" == "Output"				goto cleandir
    if /i "%~nx1" == "ClientBin"			goto cleandir
    if /i "%~nx1" == "_UpgradeReport_Files"	goto cleandir
    if /i "%~nx1" == "TestResults"			goto cleandir
    if /i "%~nx1" == "Visual Studio 2010"	goto cleandir
    goto :eof

:cleandir
    if not exist "%~f1\." goto :eof
    if %_verbosity% GEQ 1 echo Clean: Removing %~f1
    if %_verbosity% GEQ 2 pause
    rmdir /s /q "%~f1%"
    goto :eof

:delfile
    if %_verbosity% GEQ 1 echo Clean: Deleting %~f1
    if %_verbosity% GEQ 2 pause
    attrib -h -s "%~f1%" 1>nul 2>nul
    del /q /f "%~f1%"    1>nul 2>nul
    if exist "%~f1" echo Clean: Cannot delete %~f1!
    goto :eof
