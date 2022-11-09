#!/bin/bash

sudo systemctl stop Rtu.service

tar -xzf rtu.tar.gz -C /var/fibertest


sudo systemctl start Rtu.service
echo "Rtu.service started"
sleep 2s
sudo systemctl status Rtu.service


