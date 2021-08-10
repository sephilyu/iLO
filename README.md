# iLO LCD
HP iLO Plugin for LCD Smartie

LCD Smartie的插件，连接 HP iLO 来显示服务器状态信息，测试用的机器是我的 MicroServer Gen8。  
iLO 数据的读取代码来自于 iLO Viewer。

LCD Smartie：https://sourceforge.net/projects/lcdsmartie/<br/>
iLO Viewer：https://github.com/angieduan/iLOViewer<br/>

<img src="https://vkceyugu.cdn.bspapp.com/VKCEYUGU-566e2d83-057c-4e54-b404-e1ecae6881c7/0f2e0db5-2455-4714-a646-9987d20126f6.jpg" width="600px"></img>
<img src="https://vkceyugu.cdn.bspapp.com/VKCEYUGU-566e2d83-057c-4e54-b404-e1ecae6881c7/beb7c14a-6c0b-4a8d-944d-da4b1e27e332.jpg" width="600px"></img>
<img src="https://vkceyugu.cdn.bspapp.com/VKCEYUGU-566e2d83-057c-4e54-b404-e1ecae6881c7/a396db04-0b79-4ae5-a0bd-18bc1a29c433.jpg" width="600px"></img>

## FUNCTIONS
5 functions available in current version.


###### Function 1
 * param1: property path, eg: fan/fans[1]/label, temp.temperature[label=CPU].currentreading  
 * param2: use pretty value  
 * return: postfix if value exists

 examples:
 * $dll(iLO,1,fan/fans[1]/label,)  returns fan 1 speed  
 * $dll(iLO,1,temp.temperature[label=CPU].currentreading,)  returns current temperature of CPU


###### Function 2: Overview shortcut
 * param1: property path, eg: server_name means Overview.server_name  
 * param2: use pretty value  
 * return: postfix if value exists

 examples:
 * $dll(iLO,2,server_name,)  returns server_name
 

###### Function 3: fan shortcut
 * param1: property path, eg: Fan 1.speed means Fan/fans[label=Fan 1].speed  
 * param2: use pretty value  
 * return: postfix if value exists

 examples:  
 * $dll(iLO,3,Fan 1.speed,)  returns server_name


###### Function 4: temperature shortcut
 * param1: property path, eg: CPU.caution means Temp/temperature[label=CPU].caution  
 * param2: use pretty value  
 * return: postfix if value exists

 examples:  
 * $dll(iLO,4,CPU.caution,)  returns CPU caution temperature
 

###### Function 20: loading status
 * param1: not used  
 * param2: not used  
 * return: status

 examples:  
 * $dll(iLO,20,,)



## Sample
>* $dll(iLO,3,Fan 1.speed,%)$Fill(6) $dll(iLO,4,CPU.currentreading,$dll(iLO,4,CPU.temp_unit,))$Fill(12) $dll(iLO,4,PCI.currentreading,$dll(iLO,4,PCI.temp_unit,))$Fill(18) $dll(iLO,4,iLO.currentreading,$dll(iLO,4,iLO.temp_unit,))$Fill(24) $dll(iLO,4,Chipset.currentreading,$dll(iLO,4,Chipset.temp_unit,))  
>* $dll(iLO,20,,) $dll(iLO,3,Fan 1.speed,%)$Fill(6) $dll(iLO,4,CPU.currentreading,C)$Fill(12) $dll(iLO,4,PCI.currentreading,C)$Fill(18) $dll(iLO,4,iLO.currentreading,C)$Fill(24) $dll(iLO,4,Chipset.currentreading,C)
