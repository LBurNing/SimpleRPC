@echo off

rem 设置路径变量
set PROTOC_PATH="protoc.exe"
set PROTO_DIR="Protos"
set OUTPUT_DIR="ProtocolCodes"

rem 创建日志头
echo .......................proto2C#.......................
echo.

rem 检查目录是否存在
if not exist %PROTO_DIR% (
    echo Error: Protocols directory does not exist.
    echo Please create the Protocols directory and place your .proto files in it.
    echo.
    pause
    exit /b
)

rem 创建输出目录
if not exist %OUTPUT_DIR% mkdir %OUTPUT_DIR%

rem 批量处理 .proto 文件
for %%f in (%PROTO_DIR%\*.proto) do (
    echo %%f complete
    %PROTOC_PATH% --proto_path=%PROTO_DIR% --csharp_out=%OUTPUT_DIR% %%f
)

echo code generation complete. Press any key to close.
pause > nul
