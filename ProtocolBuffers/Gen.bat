@echo off

rem 设置路径变量
set PROTOC_PATH="protoc.exe"
set PROTO_DIR="Protos"
set OUTPUT_DIR="ProtocolCodes"

rem 检查目录是否存在
if not exist %PROTO_DIR% (
    echo Error: Protocols directory does not exist.
    exit /b
)

rem 创建输出目录
if not exist %OUTPUT_DIR% mkdir %OUTPUT_DIR%

rem 批量处理 .proto 文件
for %%f in (%PROTO_DIR%\*.proto) do (
    %PROTOC_PATH% --proto_path=%PROTO_DIR% --csharp_out=%OUTPUT_DIR% %%f
)

echo Code generation complete.