# HenuNetTools

适用于在河南大学计算机与信息工程学院的机房，能够做到同时连接 172.20.X.X 和 10.X.X.X , 既能正常上网又能连接ftp上交作业。

# 软件原理

```bash
netsh int ipv4 set interface "XXX" dhcpstaticipcoexistence=enabled  #启用DHCP
netsh int ipv4 add address "XXX" 172.20.X.X 255.255.255.0 172.20.X.1  #添加172网段的固定ip
route add 172.20.X.X mask 255.255.0.0 172.20.X.1 metric 1 #添加路由
```

# 使用方法

打开电脑之后不要设置自动获取ip，直接点开软件 点击应用设置即可

# 说明

不支持win7和xp系统