..\..\Bonsai\Bonsai64.exe Warmup.bonsai --start --noeditor
:while1
..\..\Bonsai\Bonsai64.exe Exhibit.bonsai --start --noeditor >> Log.txt 2>&1
(echo %DATE% %TIME%) >> Log.txt
goto :while1