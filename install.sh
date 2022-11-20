#!/bin/bash

sudo systemctl stop Rtu.service
echo "сервис RTU остановлен"
sleep 1s

tar -xzf rtu.tar.gz -C /var/fibertest
echo "файлы распакованы"

sudo systemctl start Rtu.service
echo "сервис запущен"
sleep 2s

sudo systemctl status Rtu.service
