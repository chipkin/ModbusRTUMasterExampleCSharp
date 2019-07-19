# Modbus RTU Master Example CSharp

A basic Modbus RTU Slave example written in CSharp using the [CAS Modbus Stack](https://store.chipkin.com/products/tools/cas-modbus-scanner)

## User input

- **Q** - Quit
- **R** - Read 40001, fnk=3, add=1, count=3
- **W** - Write 40001, fnk=16, add=1, count=3

## Example output

```txt

Starting Modbus RTU Master Example version 0.0.1.0
https://github.com/chipkin/ModbusTCPMasterExampleCSharp
FYI: CAS Modbus Stack version: 2.3.11.0
FYI: CAS Modbus Stack Setup, successfuly
Available Ports:
   COM1
   COM4
   COM5
Enter COM port value (Default: COM1): com4
Baud Rate(default:9600):
Available Parity options:
   None
   Odd
   Even
   Mark
   Space
Enter Parity value (Default: None):
Enter DataBits value (Default: 8):
Available StopBits options:
   None
   One
   Two
   OnePointFive
Enter StopBits value (None is not supported and
raises an ArgumentOutOfRangeException.
 (Default: One):
Available Handshake options:
   None
   XOnXOff
   RequestToSend
   RequestToSendXOnXOff
Enter Handshake value (Default: None):

FYI: Key Enter pressed.
FYI: Modbus Stack version: 2.3.11.0
Help:
 Q - Quit
 R - Read 40001, fnk=3, add=1, count=3
 W - Write 40001, fnk=16, add=1, count=3



FYI: Key Enter pressed.
FYI: Modbus Stack version: 2.3.11.0
Help:
 Q - Quit
 R - Read 40001, fnk=3, add=1, count=3
 W - Write 40001, fnk=16, add=1, count=3



FYI: Key R pressed.
FYI: ReadExample
FYI: Sending ReadRegisters message
FYI: Sending 8 bytes
    01 03 00 01 00 03 54 0B
FYI: Recived 1 bytes
    01
FYI: Recived 10 bytes
    03 06 00 00 00 00 00 00 21 75
FYI: Data: 0, 0, 0,

FYI: Key Enter pressed.
FYI: Modbus Stack version: 2.3.11.0
Help:
 Q - Quit
 R - Read 40001, fnk=3, add=1, count=3
 W - Write 40001, fnk=16, add=1, count=3



FYI: Key W pressed.
FYI: WriteExample
FYI: Sending WriteRegisters message
FYI: Sending 21 bytes
    01 10 00 01 00 06 0C 00 63 00 96 01 32 89 E0 E1 76 36 DA B0 F9
FYI: Recived 8 bytes
    01 10 00 01 00 06 11 CB
Write was successful.

FYI: Key Enter pressed.
FYI: Modbus Stack version: 2.3.11.0
Help:
 Q - Quit
 R - Read 40001, fnk=3, add=1, count=3
 W - Write 40001, fnk=16, add=1, count=3



FYI: Key R pressed.
FYI: ReadExample
FYI: Sending ReadRegisters message
FYI: Sending 8 bytes
    01 03 00 01 00 03 54 0B
FYI: Recived 1 bytes
    01
FYI: Recived 10 bytes
    03 06 63 00 96 00 32 00 10 0E
FYI: Data: 25344, 38400, 12800,

FYI: Key Enter pressed.
FYI: Modbus Stack version: 2.3.11.0
Help:
 Q - Quit
 R - Read 40001, fnk=3, add=1, count=3
 W - Write 40001, fnk=16, add=1, count=3

```


## Building

1. Copy *CASModbusStack_Win32_Debug.dll*, *CASModbusStack_Win32_Release.dll*, *CASModbusStack_x64_Debug.dll*, and *CASModbusStack_x64_Release.dll* from the [CAS Modbus Stack](https://store.chipkin.com/services/stacks/modbus-stack) project  into the /bin/ folder.
2. Use [Visual Studios 2019](https://visualstudio.microsoft.com/vs/) to build the project. The solution can be found in the */ModbusRTUMasterExampleCSharp/* folder.

Note: The project is automaticly build on every checkin using GitlabCI.

