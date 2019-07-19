/**
 * Modbus RTU Master Example CSharp
 * ----------------------------------------------------------------------------
 * Creates a simple Modbus RTU Master application that polls specific Modbus 
 * registers. 
 *
 * More information https://github.com/chipkin/ModbusRTUMasterExampleCSharp
 * 
 * Created by: Steven Smethurst 
 * Created on: June 18, 2019 
 * Last updated: July 18, 2019 
 */

using Chipkin;
using ModbusExample;
using System;
using System.Runtime.InteropServices;
using System.IO.Ports;


namespace ModbusRTUMasterExampleCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            ModbusMaster modbusMaster = new ModbusMaster();
            modbusMaster.Run();
        }

        unsafe class ModbusMaster
        {
            // Version 
            const string APPLICATION_VERSION = "0.0.1";
            
            // Configuration Options 
            const System.Byte SETTING_MODBUS_CLIENT_DEVICE_ID = 1;
            const ushort SETTING_MODBUS_CLIENT_ADDRESS = 1;
            const ushort SETTING_MODBUS_CLIENT_OFFSET = 0;
            const ushort SETTING_MODBUS_CLIENT_LENGTH = 3;

            // Database to hold the current values. 
            UInt16[] database;

            // Serial port 
            SerialPort _serialPort;

            public void Run()
            {
                // Prints the version of the application and the CAS BACnet stack. 
                Console.WriteLine("Starting Modbus RTU Master Example version {0}.{1}", APPLICATION_VERSION, CIBuildVersion.CIBUILDNUMBER);
                Console.WriteLine("https://github.com/chipkin/ModbusTCPMasterExampleCSharp");
                Console.WriteLine("FYI: CAS Modbus Stack version: {0}.{1}.{2}.{3}",
                    CASModbusAdapter.GetAPIMajorVersion(),
                    CASModbusAdapter.GetAPIMinorVersion(),
                    CASModbusAdapter.GetAPIPatchVersion(),
                    CASModbusAdapter.GetAPIBuildVersion());

                // Set up the API and callbacks.
                uint returnCode = CASModbusAdapter.Init(CASModbusAdapter.TYPE_RTU, SendMessage, RecvMessage, CurrentTime);
                if (returnCode != CASModbusAdapter.STATUS_SUCCESS)
                {
                    Console.WriteLine("Error: Could not init the Modbus Stack, returnCode={0}", returnCode);
                    return;
                }

                // All done with the Modbus setup. 
                Console.WriteLine("FYI: CAS Modbus Stack Setup, successfuly");

                // Create a new SerialPort object with default settings.
                _serialPort = new SerialPort();

                // Allow the user to set the appropriate properties.
                _serialPort.PortName = SetPortName(_serialPort.PortName);
                _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
                _serialPort.Parity = SetPortParity(_serialPort.Parity);
                _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
                _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
                _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

                // Set the read/write timeouts
                _serialPort.ReadTimeout = 10;
                _serialPort.WriteTimeout = 10;

                _serialPort.Open();

                // Program loop. 
                while (true)
                {
                    // Check for user input 
                    this.DoUserInput();

                    // Run the Modbus loop proccessing incoming messages.
                    CASModbusAdapter.Loop();

                    // Give some time to other applications. 
                    System.Threading.Thread.Sleep(1);
                }
            }

            public bool SendMessage(System.UInt16 connectionId, System.Byte* payload, System.UInt16 payloadSize)
            {
                if(!_serialPort.IsOpen)
                {
                    return false; // Serial port is not open, can't send any bytes.
                }
                Console.WriteLine("FYI: Sending {0} bytes", payloadSize);

                // Copy from the unsafe pointer to a Byte array. 
                byte[] message = new byte[payloadSize];
                Marshal.Copy((IntPtr)payload, message, 0, payloadSize);

                try
                {
                    _serialPort.Write(message, 0, payloadSize);

                    // Message sent 
                    Console.Write("    ");
                    Console.WriteLine(BitConverter.ToString(message).Replace("-", " ")); // Convert bytes to HEX string. 
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false; 
                }
            }
            public int RecvMessage(System.UInt16* connectionId, System.Byte* payload, System.UInt16 maxPayloadSize)
            {
                if (!_serialPort.IsOpen)
                {
                    return 0; // Serial port is not open, can't recive any bytes.
                }

                try
                {
                    byte[] incommingMessage = new byte[maxPayloadSize];
                    int incommingMessageSize = _serialPort.Read(incommingMessage, 0, maxPayloadSize);
                    if (incommingMessageSize <= 0)
                    {
                        // Nothing recived. 
                        return 0;
                    }

                    // Copy from the unsafe pointer to a Byte array. 
                    byte[] message = new byte[incommingMessageSize];
                    Marshal.Copy(incommingMessage, 0, (IntPtr)payload, incommingMessageSize);

                    // Debug Show the data on the console.  
                    Console.WriteLine("FYI: Recived {0} bytes", incommingMessageSize);
                    Console.Write("    ");
                    Console.WriteLine(BitConverter.ToString(incommingMessage).Replace("-", " ").Substring(0, incommingMessageSize * 3)); // Convert bytes to HEX string. 
                    return incommingMessageSize;
                }
                catch (TimeoutException) {
                    // Timeout while wating 
                    return 0; 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return 0;
                }
            }
            public ulong CurrentTime()
            {
                // https://stackoverflow.com/questions/9453101/how-do-i-get-epoch-time-in-c
                return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }

            public void PrintHelp()
            {
                Console.WriteLine("FYI: Modbus Stack version: {0}.{1}.{2}.{3}",
                    CASModbusAdapter.GetAPIMajorVersion(),
                    CASModbusAdapter.GetAPIMinorVersion(),
                    CASModbusAdapter.GetAPIPatchVersion(),
                    CASModbusAdapter.GetAPIBuildVersion());

                Console.WriteLine("Help:");
                Console.WriteLine(" Q - Quit");
                Console.WriteLine(" R - Read {0}, fnk={1}, add={2}, count={3}", 40001 + SETTING_MODBUS_CLIENT_OFFSET, CASModbusAdapter.FUNCTION_03_READ_HOLDING_REGISTERS, SETTING_MODBUS_CLIENT_DEVICE_ID, SETTING_MODBUS_CLIENT_LENGTH);
                Console.WriteLine(" W - Write {0}, fnk={1}, add={2}, count={3}", 40001 + SETTING_MODBUS_CLIENT_OFFSET, CASModbusAdapter.FUNCTION_10_FORCE_MULTIPLE_REGISTERS, SETTING_MODBUS_CLIENT_DEVICE_ID, SETTING_MODBUS_CLIENT_LENGTH);
                Console.WriteLine("\n");
            }

            private void DoUserInput()
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    Console.WriteLine("");
                    Console.WriteLine("FYI: Key {0} pressed. ", key.Key);

                    switch (key.Key)
                    {
                        case ConsoleKey.Q:
                            Console.WriteLine("FYI: Quit");
                            Environment.Exit(0);
                            break;
                        case ConsoleKey.R:
                            ReadExample(); 
                            break;
                        case ConsoleKey.W:
                            WriteExample();
                            break;
                        default:
                            this.PrintHelp();
                            break;
                    }
                }
            }

            // https://stackoverflow.com/questions/28595903/copy-from-intptr-16-bit-array-to-managed-ushort
            public static void CopyBytesIntoPointer(IntPtr source, ushort[] destination, int startIndex, int length)
            {
                unsafe
                {
                    var sourcePtr = (ushort*)source;
                    for (int i = startIndex; i < startIndex + length; ++i)
                    {
                        destination[i] = *sourcePtr++;
                    }
                }
            }
            public static void CopyBytesIntoPointer(ushort[] source, IntPtr destination)
            {
                unsafe
                {
                    var sourcePtr = (ushort*)destination;
                    foreach (System.UInt16 value in source)
                    {
                        *sourcePtr++ = value;
                    }
                }
            }


            public void WriteExample()
            {
                Console.WriteLine("FYI: WriteExample");


                System.UInt16[] data = new System.UInt16[SETTING_MODBUS_CLIENT_LENGTH];
                data[0] = 99;
                data[1] = 150;
                data[2] = 306;

                System.Byte exceptionCode;
                unsafe
                {
                    // Set some unmanaged memory up. 
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(SETTING_MODBUS_CLIENT_LENGTH);
                    CopyBytesIntoPointer(data, unmanagedPointer);

                    // Send the message 
                    Console.WriteLine("FYI: Sending WriteRegisters message");
                    uint resultWriteRegisters = CASModbusAdapter.WriteRegisters(1, SETTING_MODBUS_CLIENT_DEVICE_ID, CASModbusAdapter.FUNCTION_10_FORCE_MULTIPLE_REGISTERS, SETTING_MODBUS_CLIENT_ADDRESS, unmanagedPointer, (ushort)(data.Length * sizeof(System.UInt16)), &exceptionCode);
                    Marshal.FreeHGlobal(unmanagedPointer);

                    // Print the results. 
                    if (resultWriteRegisters == CASModbusAdapter.STATUS_SUCCESS)
                    {
                        Console.WriteLine("Write was successful.");
                    }
                    else if (resultWriteRegisters == CASModbusAdapter.STATUS_ERROR_MODBUS_EXCEPTION)
                    {
                        Console.WriteLine("Modbus.Exception={0}", exceptionCode);
                    }
                    else
                    {
                        Console.WriteLine("ModbusStack.Error={0}", resultWriteRegisters);
                    }
                }
            }
            public void ReadExample()
            {
                Console.WriteLine("FYI: ReadExample");

                System.UInt16[] data = new System.UInt16[SETTING_MODBUS_CLIENT_LENGTH];
                System.Byte exceptionCode;
                unsafe
                {
                    // Set some unmanaged memory up. 
                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(SETTING_MODBUS_CLIENT_LENGTH);

                    // Send the message 
                    Console.WriteLine("FYI: Sending ReadRegisters message");
                    uint resultReadRegisters = CASModbusAdapter.ReadRegisters(1, SETTING_MODBUS_CLIENT_DEVICE_ID, CASModbusAdapter.FUNCTION_03_READ_HOLDING_REGISTERS, SETTING_MODBUS_CLIENT_ADDRESS, SETTING_MODBUS_CLIENT_LENGTH, unmanagedPointer, (ushort)(data.Length * sizeof(System.UInt16)), &exceptionCode);

                    // 1 success 
                    if (resultReadRegisters == CASModbusAdapter.STATUS_SUCCESS)
                    {
                        // Extract the data from the unmannged memory into a managed buffer. 
                        CopyBytesIntoPointer(unmanagedPointer, data, 0, SETTING_MODBUS_CLIENT_LENGTH);
                        Marshal.FreeHGlobal(unmanagedPointer);

                        // Print the data to the screen. 
                        Console.Write("FYI: Data: ");
                        foreach (System.UInt16 value in data)
                        {
                            Console.Write(value);
                            Console.Write(", ");
                        }
                        Console.WriteLine("");
                    }
                    else if (resultReadRegisters == CASModbusAdapter.STATUS_ERROR_MODBUS_EXCEPTION)
                    {
                        Console.WriteLine("Modbus.Exception={0}", exceptionCode);
                    }
                    else
                    {
                        Console.WriteLine("ModbusStack.Error={0}", resultReadRegisters);
                    }
                }
            }





            // Display Port values and prompt user to enter a port.
            public static string SetPortName(string defaultPortName)
            {
                string portName;

                Console.WriteLine("Available Ports:");
                foreach (string s in SerialPort.GetPortNames())
                {
                    Console.WriteLine("   {0}", s);
                }

                Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
                portName = Console.ReadLine();

                if (portName == "" || !(portName.ToLower()).StartsWith("com"))
                {
                    portName = defaultPortName;
                }
                return portName;
            }
            // Display BaudRate values and prompt user to enter a value.
            public static int SetPortBaudRate(int defaultPortBaudRate)
            {
                string baudRate;

                Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
                baudRate = Console.ReadLine();

                if (baudRate == "")
                {
                    baudRate = defaultPortBaudRate.ToString();
                }

                return int.Parse(baudRate);
            }

            // Display PortParity values and prompt user to enter a value.
            public static Parity SetPortParity(Parity defaultPortParity)
            {
                string parity;

                Console.WriteLine("Available Parity options:");
                foreach (string s in Enum.GetNames(typeof(Parity)))
                {
                    Console.WriteLine("   {0}", s);
                }

                Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
                parity = Console.ReadLine();

                if (parity == "")
                {
                    parity = defaultPortParity.ToString();
                }

                return (Parity)Enum.Parse(typeof(Parity), parity, true);
            }
            // Display DataBits values and prompt user to enter a value.
            public static int SetPortDataBits(int defaultPortDataBits)
            {
                string dataBits;

                Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
                dataBits = Console.ReadLine();

                if (dataBits == "")
                {
                    dataBits = defaultPortDataBits.ToString();
                }

                return int.Parse(dataBits.ToUpperInvariant());
            }

            // Display StopBits values and prompt user to enter a value.
            public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
            {
                string stopBits;

                Console.WriteLine("Available StopBits options:");
                foreach (string s in Enum.GetNames(typeof(StopBits)))
                {
                    Console.WriteLine("   {0}", s);
                }

                Console.Write("Enter StopBits value (None is not supported and \n" +
                 "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
                stopBits = Console.ReadLine();

                if (stopBits == "")
                {
                    stopBits = defaultPortStopBits.ToString();
                }

                return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
            }
            public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
            {
                string handshake;

                Console.WriteLine("Available Handshake options:");
                foreach (string s in Enum.GetNames(typeof(Handshake)))
                {
                    Console.WriteLine("   {0}", s);
                }

                Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
                handshake = Console.ReadLine();

                if (handshake == "")
                {
                    handshake = defaultPortHandshake.ToString();
                }

                return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
            }

        }
    }
}
