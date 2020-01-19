@echo off
rem
rem The following will reset the current trade importer settings on the machine
rem that this script is run, for the "svc_qrbuild" account.
rem ------------------------------------------------------------
SetAppCfg /a:ImportGWMLTrades /u:svc_qrbuild /h:%computername% /replace /debug
rem ------------------------------------------------------------
rem SetAppCfg /a:ImportGWMLTrades /u:svc_qrbuild /h:sydwadqur01 /replace /debug
rem SetAppCfg /a:ImportGWMLTrades /replace /debug
rem
pause
