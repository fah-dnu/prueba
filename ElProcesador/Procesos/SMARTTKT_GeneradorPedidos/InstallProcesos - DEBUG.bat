@echo off
cls
echo Instalando
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\gacutil.exe" /i E:\TFS_CodigoFuente_2\Procesador\ElProcesador\Core\Procesador\bin\Debug\SMARTTKT_GeneradorPedidos\SMARTTKT_GeneradorPedidos.dll
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\gacutil.exe" /i E:\TFS_CodigoFuente_2\Procesador\ElProcesador\Dependencias\Newtonsoft.Json.dll
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\gacutil.exe" /i E:\TFS_CodigoFuente_2\Procesador\ElProcesador\Dependencias\RestSharp.dll
pause