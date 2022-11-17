#!/bin/bash

sudo dotnet build

cd bin/Debug

ls -l

tar -czvf com.tar.gz net6.0

ls -l

pscp com.tar.gz leanid@192.168.96.56:/var/fibertest/comport

echo ""
read -p "Готово, иди на МАК"

