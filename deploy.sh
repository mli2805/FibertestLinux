#!/bin/bash

sudo dotnet publish --configuration Release

echo ""
echo "запускать из каталога солюшена"

rm -rf /home/leanid/FibertestLinux/Deploy/fibertest
mkdir -p /home/leanid/FibertestLinux/Deploy/fibertest/rtu
cp /home/leanid/FibertestLinux/Rtu/Rtu/bin/Release/net6.0/publish/* /home/leanid/FibertestLinux/Deploy/fibertest/rtu

echo ""
echo ""
echo "не забудь выкачать свежие исходники и сбилдить iit_otdr & Co "
echo "    инструкция в спредшите Fibertest 3.0 features -> компиляция iit_otdr"

mkdir -p /home/leanid/FibertestLinux/Deploy/fibertest/rtu/OtdrMeasEngine
mkdir -p /home/leanid/FibertestLinux/Deploy/fibertest/rtu/OtdrMeasEngine/etc_default
mkdir -p /home/leanid/FibertestLinux/Deploy/fibertest/rtu/OtdrMeasEngine/etc
mkdir -p /home/leanid/FibertestLinux/Deploy/fibertest/rtu/OtdrMeasEngine/share

# сами либки сбилженные на этой машине
cp -a /home/leanid/Sources/OtdrMeasEngine/__Lin64Out/* /home/leanid/FibertestLinux/Deploy/fibertest/rtu/OtdrMeasEngine/
# остальные файлы без изменений из репозитория
cp /home/leanid/Sources/OtdrMeasEngine/__WinOut/ETC_default/* /home/leanid/FibertestLinux/Deploy/fibertest/rtu/OtdrMeasEngine/etc_default/
cp /home/leanid/Sources/OtdrMeasEngine/__WinOut/ETC_default/* /home/leanid/FibertestLinux/Deploy/fibertest/rtu/OtdrMeasEngine/etc/
cp /home/leanid/Sources/OtdrMeasEngine/__WinOut/SHARE/* /home/leanid/FibertestLinux/Deploy/fibertest/rtu/OtdrMeasEngine/share/

cd Deploy/fibertest
tar -czf fibertest-rtu.tar.gz rtu

echo "Результат в архиве fibertest-rtu.tar.gz"

