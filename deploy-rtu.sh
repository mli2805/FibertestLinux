#!/bin/bash

echo ""
echo "запускать из каталога солюшена"
echo ""
sudo dotnet publish --configuration Release

rm -rf /home/leanid/FibertestLinux/Deploy
mkdir -p /home/leanid/FibertestLinux/Deploy/rtu
cp /home/leanid/FibertestLinux/Rtu/Rtu/bin/Release/net6.0/publish/* /home/leanid/FibertestLinux/Deploy/rtu

echo ""
echo "не забудь выкачать свежие исходники и сбилдить iit_otdr & Co "
echo "    инструкция в спредшите Fibertest 3.0 features -> компиляция iit_otdr"
echo ""

mkdir -p /home/leanid/FibertestLinux/Deploy/rtu/OtdrMeasEngine
mkdir -p /home/leanid/FibertestLinux/Deploy/rtu/OtdrMeasEngine/etc_default
mkdir -p /home/leanid/FibertestLinux/Deploy/rtu/OtdrMeasEngine/etc
mkdir -p /home/leanid/FibertestLinux/Deploy/rtu/OtdrMeasEngine/share

# сами либки сбилженные на этой машине
cp -a /home/leanid/Sources/OtdrMeasEngine/__Lin64Out/* /home/leanid/FibertestLinux/Deploy/rtu/OtdrMeasEngine/
# остальные файлы без изменений из репозитория
cp /home/leanid/Sources/OtdrMeasEngine/__WinOut/ETC_default/* /home/leanid/FibertestLinux/Deploy/rtu/OtdrMeasEngine/etc_default/
cp /home/leanid/Sources/OtdrMeasEngine/__WinOut/ETC_default/* /home/leanid/FibertestLinux/Deploy/rtu/OtdrMeasEngine/etc/
cp /home/leanid/Sources/OtdrMeasEngine/__WinOut/SHARE/* /home/leanid/FibertestLinux/Deploy/rtu/OtdrMeasEngine/share/

cd Deploy
tar -czf rtu.tar.gz rtu

echo "Результат в архиве Deploy/rtu.tar.gz"

